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

            if (!player.IsActive)
            {
                // This indicates that the player is reconnecting mid-round. Let's try to get them
                // back into the round.
                player.IsActive = true;
                await Clients.Client(Context.ConnectionId).SendAsync("updateRound", game.Round);
            }

            await SendPlayerList(gameId, game.Players);
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


            int gameId = (int)Context.Items["gameId"];
            string groupName = gameId.ToString();
            Game game;

            lock (Program.ActiveGames)
            {
                game = Program.ActiveGames[gameId];
            }

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

            // End the round if the player is "it." 
            // TODO!!!

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await SendPlayerList(gameId, game.Players);
        }


        public async Task StartGame()
        {
            int gameId = (int) Context.Items["gameId"];
            Game game;

            lock (Program.ActiveGames)
            {
                game = Program.ActiveGames[gameId];
            }

            await StartNewRound(gameId, game);
        }

        private async Task StartNewRound(int gameId, Game game)
        {
            game.NewRound();

            game.Round.RoundState = RoundState.GetDict;

            // Some players may have been let in from pending status, so check that.
            await SendPlayerList(gameId, game.Players);
            await SendUpdateRoundAsync(gameId, game.Round);
        }

        public class SubmitDictDefArgs
        {
            public string Word { get; set; }
            public string Definition { get; set; }
        }

        public async Task SubmitDictDef(SubmitDictDefArgs args)
        {
            int gameId = (int)Context.Items["gameId"];
            Game game;

            lock (Program.ActiveGames)
            {
                game = Program.ActiveGames[gameId];
            }

            game.Round.Word = args.Word;
            game.Round.DictDef = args.Definition;

            game.Round.RoundState = RoundState.GetDefs;
            await SendUpdateRoundAsync(gameId, game.Round);

        }

        public class SubmitDefArgs
        {
            public string Definition { get; set; }
        }

        public async Task SubmitDef(SubmitDefArgs args)
        {
            int gameId = (int)Context.Items["gameId"];
            Game game;

            lock (Program.ActiveGames)
            {
                game = Program.ActiveGames[gameId];
            }

            Player player = GetCurrPlayer(game);

            game.Round.Responses.Add(player.Name, args.Definition);

            bool allAnswersSubmitted = true;

            // Check if all players have submitted a definition. If so, move to the next step!
            // TODO: skip players automatically if they wait too long.
            foreach(var p in game.ActivePlayers)
            {
                if(p != game.Round.PlayerIt && !game.Round.Responses.ContainsKey(p.Name))
                {
                    allAnswersSubmitted = false;
                }
            }

            if(allAnswersSubmitted)
            {
                game.Round.RoundState = RoundState.Vote;
                await SendUpdateRoundAsync(gameId, game.Round);
            }
        }

        public class SubmitVoteArgs
        {
            // Set to the user whose def they voted for. Set to game.Round.PlayerIt.Name if
            // they voted for the dictionary definition.
            public string UserName { get; set; }
        }

        public async Task SubmitVote(SubmitVoteArgs args)
        {
            int gameId = (int)Context.Items["gameId"];
            Game game;

            lock (Program.ActiveGames)
            {
                game = Program.ActiveGames[gameId];
            }

            Player player = GetCurrPlayer(game);

            game.Round.Votes.Add(player.Name, args.UserName);

            await CheckConcludeVoting(gameId, game);
        }

        public class SubmitVoteItArgs
        {
            public List<string> AccurateDefs { get; set; }
        }

        public async Task SubmitVoteIt(SubmitVoteItArgs args)
        {
            int gameId = (int)Context.Items["gameId"];
            Game game;

            lock (Program.ActiveGames)
            {
                game = Program.ActiveGames[gameId];
            }

            // TODO: assert that GetCurrPlayer(game) == game.Round.PlayerIt?
            // TODO: assert game.Round.AccurateDefs is empty.
            
            game.Round.AccurateDefs.AddRange(args.AccurateDefs);
            game.Round.AccurateDefsSubmitted = true;

            await CheckConcludeVoting(gameId, game);
        }

        /// <summary>
        /// Check if all votes have been placed; if so, calculate points for the round
        /// and move on. Extracted as separate method since it should be run after
        /// SubmitVote and SubmitVoteIt.
        /// </summary>
        private async Task CheckConcludeVoting(int gameId, Game game)
        {
            bool allVotesSubmitted = game.Round.AccurateDefsSubmitted;

            if (allVotesSubmitted)
            {
                // Check if all players have submitted a definition. If so, move to the next step!
                // TODO: skip players automatically if they wait too long.
                foreach (var p in game.ActivePlayers)
                {
                    if (p != game.Round.PlayerIt && !game.Round.Votes.ContainsKey(p.Name))
                    {
                        allVotesSubmitted = false;
                    }
                }
            }


            if (allVotesSubmitted)
            {
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

                game.Round.RoundState = RoundState.Review;
                await SendUpdateRoundAsync(gameId, game.Round);

                await SendPlayerList(gameId, game.Players);
            }
        }

        public async Task SubmitDoneReviewing()
        {
            int gameId = (int)Context.Items["gameId"];
            Game game;

            lock (Program.ActiveGames)
            {
                game = Program.ActiveGames[gameId];
            }

            Player player = GetCurrPlayer(game);

            game.Round.DoneReviewing.Add(player);

            bool allPlayersDone = true;

            // Check if all players have submitted a definition. If so, move to the next step!
            // TODO: skip players automatically if they wait too long.
            foreach (var p in game.ActivePlayers)
            {
                if (!game.Round.DoneReviewing.Contains(p))
                {
                    allPlayersDone = false;
                }
            }

            if (allPlayersDone)
            {
                await StartNewRound(gameId, game);
            }

        }


        public async Task SendPlayerList(int gameId, LinkedList<Player> players)
        {
            await Clients.Group(gameId.ToString()).SendAsync("setPlayerList", players);
        }

        private async Task SendUpdateRoundAsync(int gameId, Round round)
        {
            await Clients.Group(gameId.ToString()).SendAsync("updateRound", round);
        }

    }
}
