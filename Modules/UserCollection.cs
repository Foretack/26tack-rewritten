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

    public UserCollection(bool enabled)
    {
#if !DEBUG
        Time.DoEvery(TimeSpan.FromMinutes(15), Commit);
#endif
        if (!enabled)
            Disable();
    }

    protected override ValueTask OnMessage(TwitchMessage ircMessage)
    {
        if (!_users.IsFull && !_users.Any(x => x.Username == ircMessage.Username))
        {
            _users.Push(new(ircMessage.Username, ircMessage.UserId));
            Log.Verbose("[{@header}] Added user to list: {user} ({count}/{max})", Name, ircMessage.Username, _users.Count, 500);
        }

        return default;
    }

    private async Task Commit()
    {
        if (!_users.IsFull)
            return;
        Log.Debug("[{@header}] Committing user list...", Name);
        var db = new DbQueries();
        StringBuilder sb = new();

        foreach (TwitchUser user in _users)
        {
            _ = sb.Append($"('{user.Username}', {user.Id}), ");
        }

        sb[^2] = ' ';
        _users.Clear();

        int inserted = await db.Enqueue($"INSERT INTO twitch_users (username, id) " +
            $"VALUES {sb} " +
            $"ON CONFLICT ON CONSTRAINT unique_username DO NOTHING;", 10000);
        Log.Debug("{c} users inserted", inserted);

        await Task.Delay(TimeSpan.FromSeconds(5));
        await UpdateRandomUsers(db);
    }

    private async Task UpdateRandomUsers(DbQueries db)
    {
        IEnumerable<dynamic> rows = await db.Enqueue(q => q.SelectRaw("id FROM twitch_users WHERE inserted = false OFFSET floor(random() * (SELECT count(*) FROM twitch_users WHERE inserted = false)) LIMIT 45").GetAsync());
        int[] castedRows = rows.Select(x => (int)x.id).ToArray();

        _ = await db.UpdateUsers(castedRows);
    }

    private record struct TwitchUser(string Username, string Id);
}