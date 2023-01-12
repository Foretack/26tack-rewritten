using System.Text;
using SqlKata.Execution;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Modules;
internal class UserCollection : ChatModule
{
    private readonly FixedStack<TwitchUser> _users = new(100);

    public UserCollection()
    {
#if !DEBUG
        Time.DoEvery(TimeSpan.FromMinutes(15.01), Commit);
#endif
    }

    protected override ValueTask OnMessage(TwitchMessage ircMessage)
    {
        _users.Push(new(ircMessage.Username, ircMessage.UserId));
        return default;
    }

    private async Task Commit()
    {
        var db = new DbQueries();
        StringBuilder sb = new();

        foreach (var user in _users)
        {
            sb.Append($"('{user.Username}', {user.Id}), ");
        }
        sb[^2] = ' ';
        _users.Clear();

        await db.QueryFactory.StatementAsync($"INSERT INTO twitch_users (username, id) " +
            $"VALUES {sb} " +
            $"ON CONFLICT ON CONSTRAINT unique_username DO NOTHING;");
        await Task.Delay(TimeSpan.FromSeconds(5));
        await UpdateRandomUsers(db);
    }

    private async Task UpdateRandomUsers(DbQueries db)
    {
        var rows = await db.QueryFactory.Query().SelectRaw("(id) FROM twitch_users " +
            $"OFFSET floor(random() * (SELECT COUNT(*) FROM twitch_users)) LIMIT 25;").GetAsync();
        var castedRows = rows.Select(x => (int)x.id).ToArray();

        var users = await ExternalAPIHandler.GetIvrUsersById(castedRows);
        foreach (var user in users)
        {
            if (user.Banned && user.BanReason == "TOS_INDEFINITE")
            {
                await db["twitch_users"].Where("id", "=", user.Id).UpdateAsync(new { banned = true });
            }

            await db["twitch_users"].Where("id", "=", user.Id).UpdateAsync(new
            {
                account = new
                {
                    display_name = user.DisplayName,
                    username = user.Login,
                    id = user.Id,
                    avatar_url = user.Logo,
                    created_at = user.CreatedAt,
                    added_at = DateTime.Now
                }
            });

            await Task.Delay(1000);
        }
    }

    private record struct TwitchUser(string Username, string Id);
}
