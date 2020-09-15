using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DictionaryGame.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DictionaryGame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        //TODO: use a database. Clear out old games.
        public static Dictionary<int, Game> ActiveGames = new Dictionary<int, Game>();
    }
}
