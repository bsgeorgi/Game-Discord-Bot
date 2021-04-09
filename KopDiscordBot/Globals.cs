using System;
using System.Collections.Generic;
using System.IO;

namespace KopDiscordBot
{
    public class Globals
    {
        public static string RootPath = Environment.CurrentDirectory;
        public static string CharacterInfoPath = Path.Combine ( RootPath, "discord", "scripts", "characterinfo.txt" );
        public static string ItemInfoPath = Path.Combine ( RootPath, "discord", "scripts", "iteminfo.txt" );
        public static string LogsGamePath = Path.Combine ( RootPath, "discord", "logs", "game" );
        public static string LogsMapsPath = Path.Combine ( RootPath, "discord", "logs", "maps" );
        public static Dictionary<int, string> ItemInfoLines;
        public static Dictionary<string, string> CharacterInfoLines;
        public static short OnlinePlayers = 0;
        public const string Prefix = "!";
        public const string Token = "";
        public const string StatisticsUrl = "https://kingofpirates.net/game/statistics/get";
        public const ulong GameChannelId = 0;
        public static List<string> HandledFileList = new List<string> ( );
    }
}