using System;

namespace KopDiscordBot
{
    internal class Program
    {
        private static void Main ( string[] args )
        {
            Console.Title = "KOP - Discord Server";

            new Bot ( ).RunBotAsync ( ).GetAwaiter ( ).GetResult ( );
        }
    }
}