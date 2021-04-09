using System.Threading.Tasks;
using Discord.Commands;

namespace KopDiscordBot
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command ( "monster" )]
        public async Task GetMonsterInfo ( [Remainder] string monsterName )
        {
            var info = DiscordUtils.GetMonsterByName ( monsterName );

            if ( info == null )
            {
                await ReplyAsync ( "I do not recognise this monster ..." );
            }
            else
            {
                await ReplyAsync ( $"I think you are looking for {info.Name}? Very well!" );

                var embed = DiscordUtils.GetEmbedByMonster ( info );
                if ( embed != null )
                    await Context.Channel.SendMessageAsync ( "", false, DiscordUtils.GetEmbedByMonster ( info ) );
            }
        }

        [Command ( "online" )]
        public async Task GetOnlineCount ( )
        {
            var playerWord = Globals.OnlinePlayers > 1 ? "players" : "player";
            var suffix = Globals.OnlinePlayers > 1 ? "are" : "is";

            await ReplyAsync ( $"Currently there {suffix} {Globals.OnlinePlayers} online {playerWord} in game." );
        }
    }
}