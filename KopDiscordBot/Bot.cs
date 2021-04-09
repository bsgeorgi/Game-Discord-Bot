using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace KopDiscordBot
{
    internal class Bot
    {
        private readonly DiscordSocketClient _socketClient = new DiscordSocketClient ( );
        private CommandService _commandService;
        private IServiceProvider _serviceProvider;

        public Bot()
        {
            Utils.ReadFilesAsync ( ).Wait ( );

            var timer = new Timer ( e => UpdateStatistics ( ), null, TimeSpan.Zero, TimeSpan.FromMinutes ( 1 ) );

            var cleanUpTimer = new Timer ( e => Globals.HandledFileList.Clear ( ), null, TimeSpan.Zero,
                TimeSpan.FromMinutes ( 5 ) );

            var gameEventsWatcher = new FileSystemWatcher
            {
                Path = Globals.LogsGamePath,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                Filter = "*.*",
                IncludeSubdirectories = true
            };

            var gameMapsWatcher = new FileSystemWatcher
            {
                Path = Globals.LogsMapsPath,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                Filter = "*.*",
                IncludeSubdirectories = true
            };

            gameEventsWatcher.Created += OnGameFileCreated;
            gameMapsWatcher.Created += OnMapFileCreated;
        }

        private void OnMapFileCreated ( object sender, FileSystemEventArgs e )
        {
            if ( e.ChangeType != WatcherChangeTypes.Created ) return;

            var fileName = Path.GetFileName ( e.FullPath );
            if ( Globals.HandledFileList.Contains ( fileName ) ) return;

            var parentFolder = Path.GetFileName ( Path.GetDirectoryName ( e.FullPath ) );
            Console.WriteLine ( parentFolder );
            if ( string.IsNullOrEmpty ( parentFolder ) ) return;
            var channelId = Convert.ToUInt64 ( parentFolder );

            if ( WaitForFile ( e.FullPath ) )
                try
                {
                    using (var r = new StreamReader ( File.Open ( e.FullPath, FileMode.Open, FileAccess.Read,
                        FileShare.ReadWrite ) ))
                    {
                        var s = r.ReadToEnd ( );

                        var task = Task.Run ( async () => await SendGameMessageAsync ( s, channelId ) );

                        var res = task.Result ? "OK" : "FAIL";
                        Console.WriteLine ( $"Game event message sent to discord...{res}" );
                    }

                    Globals.HandledFileList.Add ( fileName );
                }
                catch
                {
                    Console.WriteLine ( $"An error occured when reading a log file {e.FullPath}" );
                }
                finally
                {
                    File.Delete ( e.FullPath );
                }
        }

        private void OnGameFileCreated ( object sender, FileSystemEventArgs e )
        {
            if ( e.ChangeType != WatcherChangeTypes.Created ) return;

            var fileName = Path.GetFileName ( e.FullPath );
            if ( Globals.HandledFileList.Contains ( fileName ) ) return;

            if ( WaitForFile ( e.FullPath ) )
                try
                {
                    using (var r = new StreamReader ( File.Open ( e.FullPath, FileMode.Open, FileAccess.Read,
                        FileShare.ReadWrite ) ))
                    {
                        var s = r.ReadToEnd ( );

                        var task = Task.Run ( async () => await SendGameMessageAsync ( s, Globals.GameChannelId ) );

                        var res = task.Result ? "OK" : "FAIL";
                        Console.WriteLine ( $"Game event message sent to discord...{res}" );
                    }

                    Globals.HandledFileList.Add ( fileName );
                }
                catch
                {
                    Console.WriteLine ( $"An error occured when reading a log file {e.FullPath}" );
                }
                finally
                {
                    File.Delete ( e.FullPath );
                }
        }

        private bool WaitForFile ( string filePath )
        {
            var tries = 0;

            while (true)
            {
                ++tries;
                FileStream stream = null;

                try
                {
                    stream = new FileStream ( filePath, FileMode.Open, FileAccess.Read, FileShare.None );
                    break;
                }
                catch
                {
                    if ( tries > 10 )
                    {
                        Console.WriteLine ( $"Access file {filePath} aborted the file after 10 tries" );
                        return false;
                    }
                }
                finally
                {
                    stream?.Close ( );
                }

                Thread.Sleep ( 250 );
            }

            Console.WriteLine ( $"Access file {filePath} got an exclusive lock after {tries} tries" );

            return true;
        }

        private async Task<bool> SendGameMessageAsync ( string s, ulong channelId )
        {
            try
            {
                if ( !(_socketClient.GetChannel ( channelId ) is IMessageChannel channel) )
                    return await Task.FromResult ( false );

                _ = await channel.SendMessageAsync ( s );
                return await Task.FromResult ( true );
            }
            catch (Exception e)
            {
                Console.WriteLine ( e.Message );
                return await Task.FromResult ( false );
            }
        }

        private static async void UpdateStatistics()
        {
            Console.WriteLine ( "Updating server statistics ..." );

            try
            {
                await Task.Run ( () =>
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    ServicePointManager.DefaultConnectionLimit = 20;


                    using (var wc = new WebClient ( ))
                    {
                        wc.Proxy = null;

                        var json = wc.DownloadString ( Globals.StatisticsUrl );
                        dynamic data = JObject.Parse ( json );

                        Globals.OnlinePlayers = data.online;
                    }
                } );

                Console.WriteLine ( "Statistics updated...OK" );
            }
            catch (Exception e)
            {
                Console.WriteLine ( e.Message );
            }
        }

        public async Task RunBotAsync()
        {
            _commandService = new CommandService ( );
            _serviceProvider = new ServiceCollection ( ).AddSingleton ( _socketClient ).AddSingleton ( _commandService )
                .BuildServiceProvider ( );

            _socketClient.Log += _socketClient_Log;
            _socketClient.UserJoined += _socketClient_UserJoined;

            await RegisterCommandsAsync ( );
            await _socketClient.LoginAsync ( TokenType.Bot, Globals.Token );
            await _socketClient.StartAsync ( );
            await Task.Delay ( -1 );
        }

        private async Task _socketClient_UserJoined ( SocketGuildUser user )
        {
            var channel = (SocketTextChannel) _socketClient.GetChannel ( 716941878213476417 );
            if ( channel == null ) return;

            if ( user.Username.ToLowerInvariant ( ) == "memory" )
            {
                await user.BanAsync ( );
                await channel.SendMessageAsync (
                    $"{user.Username} has been banned for 1000 days for violating King of Pirates rules." );
            }
        }

        private Task _socketClient_Log ( LogMessage arg )
        {
            Console.WriteLine ( arg );

            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _socketClient.MessageReceived += _socketClient_MessageReceived;

            await _commandService.AddModulesAsync ( Assembly.GetEntryAssembly ( ), _serviceProvider );
        }

        private async Task _socketClient_MessageReceived ( SocketMessage arg )
        {
            if ( !(arg is SocketUserMessage message) || message.Author.IsBot ) return;

            var context = new SocketCommandContext ( _socketClient, message );


            var argPos = 0;
            if ( message.HasStringPrefix ( Globals.Prefix, ref argPos ) )
            {
                var result = await _commandService.ExecuteAsync ( context, argPos, _serviceProvider );

                if ( !result.IsSuccess )
                    Console.WriteLine ( result.ErrorReason );
            }
        }
    }
}