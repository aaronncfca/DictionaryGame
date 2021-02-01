using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DictionaryGame.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace DictionaryGame
{
    /// <summary>
    /// Hub for communication with game clients. Includes most in-game logic.
    /// </summary>
    public class GameHub : Hub
    {
        public class JoinGameReqArgs
        {
            public int GameId { get; set; }
            public string Username { get; set; }
        }

        /// <summary>
        /// Adds this user to the SignalR group with the game ID. Should be called on game creation.
        /// </summary>
        public async Task JoinGame(JoinGameReqArgs args)
        {
            string groupName = args.GameId.ToString();
            int gameId = args.GameId;

            Context.Items.Add("gameId", gameId);
            Context.Items.Add("username", args.Username);

            Game game;

            lock (Program.ActiveGames)
            {
                game = Program.ActiveGames[gameId];
            }

            var player = GetCurrPlayer(game);


            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // If the player is reconnecting mid - round try to get them straight
            // into the round. Otherwise, they will join once the next round starts.
            if (game.Round != null && !player.IsPending)
            {
                await SendRoundAsync(gameId, game.Round);
            }

            await SendPlayerListAsync(gameId, game.Players);
        }

        private void GetCurrGame(out int gameId, out Game game)
        {
            gameId = (int)Context.Items["gameId"];

            lock (Program.ActiveGames)
            {
                game = Program.ActiveGames[gameId];
            }
        }

        private Player GetCurrPlayer(Game game)
        {
            string userName = (string)Context.Items["username"];

            Player player = game.Players.FirstOrDefault((entry) => entry.Name == userName);
            if (player == null)
            {
                // Should never happen in theory, since the hub connection is established
                // after the player calls GameController.Create/JoinGame successfully,
                // and the user is not removed from the Game until the hub connection is closed.
                throw new KeyNotFoundException("Player not found!");
            }

            return player;
        }

        /// <summary>
        /// On disconnection, remove the user from the game.
        /// If this user was the host, assign a new host (TODO).
        /// If this user was the last user, delete the Game.
        /// If this user was "it," end the round but not the game.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);

            // If the page has been refreshed without properly joining, the connection
            // will be closed without gameId being set (etc.). In this case, there is
            // nothing to clean up here.
            if (!Context.Items.ContainsKey("gameId")) return;


            GetCurrGame(out int gameId, out Game game);
            string groupName = gameId.ToString();
            
            Player player = GetCurrPlayer(game);

            player.IsActive = false;

            // Delete the game if no players remain active.
            lock(Program.ActiveGames)
            {
                if(game.ActivePlayers.Count() == 0)
                {
                    Program.ActiveGames.Remove(gameId);
                }
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await SendPlayerListAsync(gameId, game.Players);

            // End the round if the player is "it." 
            if (game.Round != null && game.Round.RoundState != RoundState.Review
                && game.Round.PlayerIt == player)
            {
                await SendMessageAsync(gameId, $"Oops! {player.Name} has disconnected, and they were it! " +
                    $"Cancelling this round and starting a new one.");

                await StartNewRound(gameId, game);
            }

            // Let other players know if the host is gone.
            else if ((game.Round == null || game.Round.RoundState == RoundState.Lobby)
                && game.Host == player)
            {
                await SendMessageAsync(gameId, $"Oops! {player.Name} has disconnected, and they were the host! " +
                    $"Please leave this game and try creating or joining a new one.");

                // Remove this game from the list of active ones. Since we are in Lobby mode
                // and the host is the only one who can submit anything, this should not
                // produce any errors.
                lock (Program.ActiveGames)
                {
                    Program.ActiveGames.Remove(gameId);
                }
            }
        }


        public async Task StartGame()
        {
            GetCurrGame(out int gameId, out Game game);

            await StartNewRound(gameId, game);
        }

        private async Task StartNewRound(int gameId, Game game)
        {
            game.NewRound();

            game.Round.RoundState = RoundState.GetDict;

            // Some players may have been let in from pending status, so check that.
            await SendPlayerListAsync(gameId, game.Players);
            await SendRoundAsync(gameId, game.Round);
        }

        public class SubmitDictDefArgs
        {
            public string Word { get; set; }
            public string Definition { get; set; }
        }

        public async Task SubmitDictDef(SubmitDictDefArgs args)
        {
            GetCurrGame(out int gameId, out Game game);

            game.Round.Word = args.Word;
            game.Round.DictDef = args.Definition;

            // Advance to the next RoundState!

            game.Round.RoundState = RoundState.GetDefs;
            await SendRoundAsync(gameId, game.Round);
        }

        public class SubmitDefArgs
        {
            public string Definition { get; set; }
        }

        public async Task SubmitDef(SubmitDefArgs args)
        {
            GetCurrGame(out int gameId, out Game game);

            Player player = GetCurrPlayer(game);

            game.Round.Responses.Add(player.Name, args.Definition);

            await CheckDefsSubmitted(gameId, game, false);
        }

        private async Task CheckDefsSubmitted(int gameId, Game game, bool timedOut)
        {
            // The Round may have already moved on due to timeout.
            if (game.Round?.RoundState != RoundState.GetDefs) return;

            if (!timedOut)
            {
                // Check if all players have submitted a definition, unless timed out.
                foreach (var p in game.ActivePlayers)
                {
                    if (p != game.Round.PlayerIt && !game.Round.Responses.ContainsKey(p.Name))
                    {
                        return;
                    }
                }
            }

            game.Round.RoundState = RoundState.Vote;
            await SendRoundAsync(gameId, game.Round);
        }

        public class SubmitVoteArgs
        {
            // Set to the user whose def they voted for. Set to game.Round.PlayerIt.Name if
            // they voted for the dictionary definition.
            public string UserName { get; set; }
        }

        public async Task SubmitVote(SubmitVoteArgs args)
        {
            GetCurrGame(out int gameId, out Game game);
            Player player = GetCurrPlayer(game);

            game.Round.Votes.Add(player.Name, args.UserName);

            await CheckConcludeVoting(gameId, game, false);
        }

        public class SubmitVoteItArgs
        {
            public List<string> AccurateDefs { get; set; }
        }

        public async Task SubmitVoteIt(SubmitVoteItArgs args)
        {
            GetCurrGame(out int gameId, out Game game);

            // TODO: assert that GetCurrPlayer(game) == game.Round.PlayerIt?
            // TODO: assert game.Round.AccurateDefs is empty.

            game.Round.AccurateDefs.AddRange(args.AccurateDefs);
            game.Round.AccurateDefsSubmitted = true;

            await CheckConcludeVoting(gameId, game, false);
        }

        /// <summary>
        /// Check if all votes have been placed; if so, calculate points for the round
        /// and move on. Extracted as separate method since it should be run after
        /// SubmitVote and SubmitVoteIt.
        /// </summary>
        private async Task CheckConcludeVoting(int gameId, Game game, bool timedOut)
        {
            // The Round may have already moved on due to timeout.
            if (game.Round?.RoundState != RoundState.Vote) return;

            if (!timedOut)
            {
                if (!game.Round.AccurateDefsSubmitted) return;

                // Ensure players have submitted a definition before moving on (unless timed out
                foreach (var p in game.ActivePlayers)
                {
                    if (p != game.Round.PlayerIt && !game.Round.Votes.ContainsKey(p.Name))
                    {
                        return;
                    }
                }
            }

            var round = game.Round;

            // If this remains false and round.AccurateDefs is empty, then PlayerIt
            // is awarded Points.UnguessableWord.
            bool someoneVotedAccurate = false;

            //Calculate points earned.
            foreach(var p in game.Players)
            {
                if (p == round.PlayerIt) continue; // PlayerIt doesn't earn these points.
                if (p.IsPending) continue; // Nor pending players (obviously).
                // However, we do include inactive players just in case they contributed something.

                int points = 0;
                if(round.AccurateDefs.Contains(p.Name))
                {
                    points += Points.AccurateDef;
                }

                if (round.Votes.ContainsKey(p.Name))
                {
                    var votedFor = round.Votes[p.Name];
                    if (votedFor == round.PlayerIt.Name || round.AccurateDefs.Contains(votedFor))
                    {
                        points += Points.AccurateVote;
                        someoneVotedAccurate = true;
                    }
                }

                int votedForMe = round.Votes.Count((v) => v.Value == p.Name);
                points += Points.VotedForMe * votedForMe;

                if(points > 0)
                {
                    round.PointsAwarded.Add(p.Name, points);
                    p.Points += points;
                }
            }

            // Award Points.UnguessableWord if noone guessed the correct definition.
            if(round.AccurateDefs.Count == 0 && !someoneVotedAccurate)
            {
                round.PointsAwarded.Add(round.PlayerIt.Name, Points.UnguessableWord);
                round.PlayerIt.Points += Points.UnguessableWord;
            }

            // Advance to the next RoundState

            game.Round.RoundState = RoundState.Review;
            await SendRoundAsync(gameId, game.Round);

            // Update player list, since scores may have changed.
            await SendPlayerListAsync(gameId, game.Players);
        }

        public async Task SubmitDoneReviewing()
        {
            GetCurrGame(out int gameId, out Game game);
            Player player = GetCurrPlayer(game);

            game.Round.DoneReviewing.Add(player);

            await CheckDoneReviewing(gameId, game, false);
        }

        private async Task CheckDoneReviewing(int gameId, Game game, bool timedOut)
        {
            // The Round may have already moved on due to timeout.
            if (game.Round?.RoundState != RoundState.Review) return;

            if (!timedOut)
            {
                // Ensure all players are done reviewing, unless timed out.
                foreach (var p in game.ActivePlayers)
                {
                    if (!game.Round.DoneReviewing.Contains(p))
                    {
                        return;
                    }
                }
            }

            await StartNewRound(gameId, game);
        }

        public class StepTimeoutArgs
        {
            public RoundState RoundState { get; set; }
        }

        public async Task StepTimeout(StepTimeoutArgs args)
        {

            if(args.RoundState != )
        }

        private async Task SendMessageAsync(int gameId, string message)
        {
            await Clients.Group(gameId.ToString()).SendAsync("showMessage", message);
        }

        private async Task SendPlayerListAsync(int gameId, LinkedList<Player> players)
        {
            await Clients.Group(gameId.ToString()).SendAsync("updatePlayerList", players);
        }

        private async Task SendRoundAsync(int gameId, Round round)
        {
            await Clients.Group(gameId.ToString()).SendAsync("updateRound", round);
        }
    }
}
