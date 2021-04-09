using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KopDiscordBot
{
    public class Pair<T1, T2>
    {
        public T1 First { get; set; }
        public T2 Second { get; set; }
    }

    public class Utils
    {
        public static async Task<Dictionary<int, string>> ReadFileAsync ( string source )
        {
            try
            {
                var fileName = Path.GetFileName ( source );
                Console.WriteLine ( $"Begin read file {fileName}" );

                var lines = new Dictionary<int, string> ( );
                using (var reader = new StreamReader ( source ))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync ( );
                        if ( string.IsNullOrEmpty ( line ) || line.Substring ( 0, 2 ) == "//" ) continue;

                        var contents = line.Split ( '\t' );
                        int.TryParse ( contents[0], out var index );

                        if ( !lines.ContainsKey ( index ) )
                            lines.Add ( index, line );
                    }
                }

                Console.WriteLine ( $"End read file {fileName} ...OK" );
                return lines;
            }
            catch (Exception e)
            {
                Console.WriteLine ( e );
                throw;
            }
        }

        public static async Task<Dictionary<string, string>> ReadFileByNameAsync ( string source )
        {
            try
            {
                var fileName = Path.GetFileName ( source );
                Console.WriteLine ( $"Begin read file {fileName}" );

                var lines = new Dictionary<string, string> ( );
                using (var reader = new StreamReader ( source ))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync ( );
                        if ( string.IsNullOrEmpty ( line ) || line.Substring ( 0, 2 ) == "//" ) continue;

                        var contents = line.Split ( '\t' );
                        var name = contents[1].ToLowerInvariant ( );

                        if ( !lines.ContainsKey ( name ) )
                            lines.Add ( name, line );
                    }
                }

                Console.WriteLine ( $"End read file {fileName} ...OK" );
                return lines;
            }
            catch (Exception e)
            {
                Console.WriteLine ( e );
                throw;
            }
        }

        public static IEnumerable<Pair<T1, T2>> Zip<T1, T2> ( IEnumerable<T1> first, IEnumerable<T2> second )
        {
            var enumerable = first.ToList ( );
            var enumerable1 = second.ToList ( );
            if ( enumerable.Count ( ) != enumerable1.Count ( ) )
                throw new ArgumentException ( "List sizes do not match!" );

            using (var e1 = enumerable.GetEnumerator ( ))
            using (var e2 = enumerable1.GetEnumerator ( ))
            {
                while (e1.MoveNext ( ) && e2.MoveNext ( ))
                    yield return new Pair<T1, T2> {First = e1.Current, Second = e2.Current};
            }
        }

        public static async Task ReadFilesAsync()
        {
            var characterInfo = ReadFileByNameAsync ( Globals.CharacterInfoPath );
            var itemInfo = ReadFileAsync ( Globals.ItemInfoPath );

            _ = Task.WhenAll ( characterInfo, itemInfo );

            Globals.ItemInfoLines = await itemInfo;
            Console.WriteLine ( $"ItemInfo total entries found: {Globals.ItemInfoLines.Count}" );

            Globals.CharacterInfoLines = await characterInfo;
            Console.WriteLine ( $"CharacterInfo total entries found: {Globals.CharacterInfoLines.Count}" );
        }
    }
}