using System.Text;
using SqlKata.Execution;
using Tack.Database;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Modules;
internal sealed class UserCollection : ChatModule
{
    private readonly FixedStack<TwitchUser> _users = new(500);

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
        if (!_users.IsFull) return;

        var db = new DbQueries();
        StringBuilder sb = new();

        foreach (var user in _users)
        {
            sb.Append($"('{user.Username}', {user.Id}), ");
        }
        sb[^2] = ' ';
        _users.Clear();

        int inserted = await db.QueryFactory.StatementAsync($"INSERT INTO twitch_users (username, id) " +
            $"VALUES {sb} " +
            $"ON CONFLICT ON CONSTRAINT unique_username DO NOTHING;");
        Log.Debug("{c} users inserted", inserted);

        await Task.Delay(TimeSpan.FromSeconds(5));
        await UpdateRandomUsers(db);
    }

    private async Task UpdateRandomUsers(DbQueries db)
    {
        var rows = await db.QueryFactory.Query().SelectRaw("id FROM twitch_users OFFSET floor(random() * (SELECT count(*) FROM twitch_users)) LIMIT 45").GetAsync();
        var castedRows = rows.Select(x => (int)x.id).ToArray();

        await db.UpdateUsers(castedRows);
    }

    private record struct TwitchUser(string Username, string Id);
}
