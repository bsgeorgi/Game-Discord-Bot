using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Discord;
using KopDiscordBot.Models;

namespace KopDiscordBot
{
    public class DiscordUtils
    {
        public static string GetItemNameById ( int id )
        {
            try
            {
                if ( !Globals.ItemInfoLines.ContainsKey ( id ) ) return null;

                var line = Globals.ItemInfoLines[id].Split ( '\t' );

                return line[1];
            }
            catch (Exception e)
            {
                Console.WriteLine ( e.Message );
                return null;
            }
        }

        public static Embed GetEmbedByMonster ( MonsterInfo monster )
        {
            try
            {
                var eb = new EmbedBuilder {Title = monster.Name};

                eb.WithThumbnailUrl ( $"https://kingofpirates.net/assets/images/monsters/images/{monster.ID}.png" );

                eb.AddField ( "Level", monster.Level, true );
                eb.AddField ( "Hit Points", monster.HP, true );
                eb.AddField ( "Mana Points", monster.SP, true );

                eb.AddField ( "Minimum Attack", monster.MinAttack, true );
                eb.AddField ( "Maximum Attack", monster.MaxAttack, true );
                eb.AddField ( "Defense", monster.Defence, true );

                eb.AddField ( "Hit Rate", monster.HitRate, true );
                eb.AddField ( "Dodge", monster.Dodge, true );
                eb.AddField ( "Physical Resistance", monster.PhysicalResistance, true );

                eb.AddField ( "Attack Speed", monster.AttackSpeed, true );
                eb.AddField ( "Movement Speed", monster.MovementSpeed, true );
                eb.AddField ( "Experience", monster.Experience, true );

                if ( monster.DropDictionary.Count <= 0 ) return eb.Build ( );

                var dropField = monster.DropDictionary.Aggregate ( "```",
                    ( current, kvp ) =>
                        current +
                        $"{(GetItemNameById ( kvp.Key ) == null ? "Unknown" : GetItemNameById ( kvp.Key ))} - {kvp.Value}% chance\n" );
                dropField += "```";
                eb.AddField ( "Monster Drop Info", dropField );

                return eb.Build ( );
            }
            catch (Exception e)
            {
                Console.WriteLine ( e.Message );
                return null;
            }
        }

        public static MonsterInfo GetMonsterByName ( string searchParam )
        {
            if ( string.IsNullOrEmpty ( searchParam ) ) return null;

            var searchStringToLower = searchParam.ToLowerInvariant ( );

            var getDataByDistance = new Dictionary<string, int> ( );

            foreach (var kvp in Globals.CharacterInfoLines)
            {
                var line = kvp.Value;
                if ( getDataByDistance.ContainsKey ( line ) ) continue;

                var distance = LevenshteinDistance.Calculate ( searchStringToLower, kvp.Key );
                getDataByDistance.Add ( line, distance );
            }

            Console.WriteLine ( getDataByDistance.Count );

            if ( getDataByDistance.Count == 0 ) return null;

            var items = from pair in getDataByDistance orderby pair.Value select pair;

            var contents = items.FirstOrDefault ( ).Key.Split ( '\t' );

            var dropKeys = contents[46].Split ( ',' );
            var dropValues = contents[47].Split ( ',' );

            dropKeys = dropKeys.Where ( n => n != "-1" && n != "0" ).ToArray ( );
            dropValues = dropValues.Where ( n => n != "-1" && n != "0" ).ToArray ( );

            var dropDict = new Dictionary<short, double> ( );

            if ( dropValues.Length > 0 )
            {
                foreach (var value in dropValues)
                {
                    var toDouble = Convert.ToDouble ( value );
                    var toNorm = Math.Round ( 10000 / toDouble, 2 );
                    dropValues[Array.IndexOf ( dropValues, value )] = toNorm.ToString ( CultureInfo.InvariantCulture );
                }

                dropDict = Utils.Zip ( dropKeys.ToList ( ), dropValues.ToList ( ) )
                    .Where ( pair => pair.Second != "-1" ).ToDictionary ( pair => Convert.ToInt16 ( pair.First ),
                        pair => Convert.ToDouble ( pair.Second ) );
            }

            return new MonsterInfo
            {
                ID                 = Convert.ToInt16 ( contents[0] ),
                Name               = contents[1],
                Level              = Convert.ToInt16 ( contents[60] ),
                HP                 = Convert.ToInt64 ( contents[61] ),
                SP                 = Convert.ToInt64 ( contents[63] ),
                MinAttack          = Convert.ToInt32 ( contents[65] ),
                MaxAttack          = Convert.ToInt32 ( contents[66] ),
                Defence            = Convert.ToInt32 ( contents[68] ),
                HitRate            = Convert.ToInt32 ( contents[69] ),
                Dodge              = Convert.ToInt32 ( contents[70] ),
                PhysicalResistance = Convert.ToInt16 ( contents[67] ),
                AttackSpeed        = Convert.ToInt32 ( contents[75] ),
                MovementSpeed      = Convert.ToInt32 ( contents[78] ),
                Experience         = Convert.ToInt64 ( contents[90] ),
                DropDictionary     = dropDict.Count == 0 ? null : dropDict,
                CorrectFind        = searchStringToLower == contents[1].ToLowerInvariant ( )
            };
        }
    }
}