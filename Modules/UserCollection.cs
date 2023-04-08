using System.Text;
using MiniTwitch.Irc.Models;
using SqlKata.Execution;
using Tack.Core;
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

    protected override ValueTask OnMessage(Privmsg message)
    {
        if (!_users.IsFull && !_users.Any(x => x.Username == message.Author.Name))
        {
            if (message.Author.Name.Length > 25)
            {
                Log.Warning("[{@header}] @{guy} <- This guy's name is longer than 25??", nameof(UserCollection), message.Author.Name);
                return default;
            }

            _users.Push(new(message.Author.Name, message.Author.Id));
            Log.Verbose("[{@header}] Added user to list: {user} ({count}/{max})", Name, message.Author.Name, _users.Count, 500);
            return default;
        }

        SingleOf<MainClient>.Obj.Client.OnMessage -= OnMessage;
        SingleOf<AnonymousClient>.Obj.Client.OnMessage -= OnMessage;
        Log.Debug("[{h}] User list full. Unsubscribing from event {ev}", Name, OnMessage);
        return default;
    }

    private async Task Commit()
    {
        if (!_users.IsFull)
            return;

        Log.Debug("[{@header}] Committing user list...", Name);
        DbQueries db = SingleOf<DbQueries>.Obj;
        StringBuilder sb = new();
        foreach (TwitchUser user in _users)
            _ = sb.Append($"('{user.Username}', {user.Id}), ");

        sb[^2] = ' ';
        _users.Clear();
        SingleOf<MainClient>.Obj.Client.OnMessage += OnMessage;
        SingleOf<AnonymousClient>.Obj.Client.OnMessage += OnMessage;
        Log.Debug("[{h}] Resubscribed to {ev}", OnMessage);
        db.Enqueue(async qf =>
        {
            int inserted = await qf.StatementAsync(
                $"INSERT INTO twitch_users (username, id) "
                + $"VALUES {sb} "
                + $"ON CONFLICT ON CONSTRAINT unique_username DO NOTHING;");
            Log.Debug("{c} users inserted", inserted);
        });

        await Task.Delay(TimeSpan.FromSeconds(5));
        await UpdateRandomUsers();
    }

    private static async Task UpdateRandomUsers()
    {
        DbQueries db = SingleOf<DbQueries>.Obj;
        int[] uids = await db.ValueStatement(async qf =>
        {
            IEnumerable<dynamic> rows = await qf.Query().SelectRaw("id FROM twitch_users WHERE inserted = false OFFSET floor(random() * (SELECT count(*) FROM twitch_users WHERE inserted = false)) LIMIT 45")
                .GetAsync();
            return rows.Select(x => x.id).OfType<int>().ToArray();
        });
        await db.UpdateUsers(uids);
    }

    private record struct TwitchUser(string Username, long Id);
}
