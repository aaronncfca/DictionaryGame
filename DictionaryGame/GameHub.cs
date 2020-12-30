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
        public async Task SendPlayerList(int gameId)
        {
            Game game;
            bool success;

            lock (Program.ActiveGames)
            {
                success = Program.ActiveGames.TryGetValue(gameId, out game);
            }

            if (!success) throw new ArgumentException("Game not found!");

            await Clients.Group(gameId.ToString()).SendAsync("setPlayerList", game.Players);
        }

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

            Context.Items.Add("gameId", args.GameId);
            Context.Items.Add("username", args.Username);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await SendPlayerList(args.GameId);
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

            game.Players.Remove(player);

            // Delete the game if no players remain.
            lock(Program.ActiveGames)
            {
                if(game.Players.First == null)
                {
                    Program.ActiveGames.Remove(gameId);
                }
            }

            // End the round if the player is "it." 

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await SendPlayerList(gameId);
        }


        public async Task StartGame() //TODO: rename StartRound . . . no, extract StartRound.
        {
            int gameId = (int) Context.Items["gameId"];
            Game game;

            lock (Program.ActiveGames)
            {
                game = Program.ActiveGames[gameId];
            }

            game.NewRound();

            game.Round.RoundState = RoundState.GetDict;
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
            foreach(var p in game.Players)
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
                foreach (var p in game.Players)
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

                    int points = 0;
                    if(round.AccurateDefs.Contains(p.Name))
                    {
                        points += Points.AccurateDef;
                    }

                    var votedFor = round.Votes[p.Name];
                    if(votedFor == round.PlayerIt.Name || round.AccurateDefs.Contains(votedFor))
                    {
                        points += Points.AccurateVote;
                        someoneVotedAccurate = true;
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

                await SendPlayerList(gameId);
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
            foreach (var p in game.Players)
            {
                if (!game.Round.DoneReviewing.Contains(p))
                {
                    allPlayersDone = false;
                }
            }

            if (allPlayersDone)
            {
                await StartGame(); //Start a new game!
                // TODO: Keep track of the round number and show that.
            }

        }

        private async Task SendUpdateRoundAsync(int gameId, Round round)
        {
            // TODO: send the entire Round. This solves the problem of stale Round data on new round
            // and simplifies the code.
            await Clients.Group(gameId.ToString()).SendAsync("updateRound", round);
        }

        // TODO: implement submitVote and SubmitVoteIt!
        // Then implement the next screen!

    }
}
