using MiniTwitch.Irc.Interfaces;
using MiniTwitch.Irc.Models;
using Tack.Core;
using Tack.Database;
using Tack.Models;
using Tack.Utils;
using TwitchLib.Api.Helix.Models.Chat;

namespace Tack.Handlers;
public static class ChannelHandler
{
    #region Properties
    public static List<ExtendedChannel> MainJoinedChannels { get; } = new List<ExtendedChannel>();
    public static List<string> MainJoinedChannelNames { get; } = new List<string>();
    public static List<ExtendedChannel> FetchedChannels { get; private set; } = SingleOf<DbQueries>.Obj.GetChannels().Result.ToList();

    private static readonly AnonymousClient _anon = SingleOf<AnonymousClient>.Obj;
    private static readonly MainClient _main = SingleOf<MainClient>.Obj;
    private static readonly List<ExtendedChannel> _joinFailureChannels = new();
    private static bool _isInProgress = false;
    #endregion

    #region Initialization
    internal static async Task Connect(bool isReconnect)
    {
        if (_isInProgress)
            return;
        _isInProgress = true;

        if (isReconnect)
        {
            MainJoinedChannels.Clear();
            MainJoinedChannelNames.Clear();
        }

        RegisterEvents(isReconnect);

        await Redis.Cache.SetObjectAsync("twitch:channels", FetchedChannels);
        foreach (ExtendedChannel channel in FetchedChannels)
        {
            await Task.Delay(600);
            if (channel.Priority >= 50)
            {
                if (!await _main.Client.JoinChannel(channel.Username))
                    Log.Warning("[{h}] Failed to join {c}", nameof(ChannelHandler), channel.Username);

                continue;
            }

            if (!await _anon.Client.JoinChannel(channel.Username))
                Log.Warning("[{h}] Failed to join {c}", nameof(ChannelHandler), channel.Username);
        }

        Log.Information("[{h}] {t} Joined: `{c}`", nameof(ChannelHandler), nameof(AnonymousClient), _anon.Client.JoinedChannels.Select(x => x.Name));
        Log.Information("[{h}] {t} Joined: `{c}`", nameof(ChannelHandler), nameof(MainClient), _main.Client.JoinedChannels.Select(x => x.Name));

        _isInProgress = false;
        Time.DoEvery(TimeSpan.FromHours(1), async () =>
        {
            await ReloadFetchedChannels();
            await Redis.Cache.SetObjectAsync("twitch:channels", FetchedChannels);
        });
    }
    #endregion

    #region Methods
    /// <returns>True if successful; Otherwise false</returns>
    public static async Task<bool> JoinChannel(string channel, int priority = 0, bool logged = true)
    {
        if (FetchedChannels.Any(x => x.Username == channel))
            return false;
        var c = new Channel(channel, priority, logged);
        Result<ExtendedChannel> extendedChannel = await User.GetChannel(c);
        if (!extendedChannel.Success)
            return false;
        FetchedChannels.Add(extendedChannel.Value);

        if (priority >= 50)
        {
            if (await _main.Client.JoinChannel(channel))
                Log.Information("[{h}] Joined {c}", nameof(ChannelHandler), channel);
            else
                Log.Warning("[{h}] Failed to join {c}", nameof(ChannelHandler), channel);
        }
        else
        {
            if (await _anon.Client.JoinChannel(channel))
                Log.Information("[{h}] Joined {c}", nameof(ChannelHandler), channel);
            else
                Log.Warning("[{h}] Failed to join {c}", nameof(ChannelHandler), channel);
        }

        DbQueries db = SingleOf<DbQueries>.Obj;
        bool s = await db.AddChannel(extendedChannel.Value);
        return s;
    }

    /// <returns>True if successful; Otherwise false</returns>
    public static async Task<bool> PartChannel(string channel)
    {
        ExtendedChannel? fetched = FetchedChannels.FirstOrDefault(x => x.Username == channel);
        if (fetched is null)
            return false;

        try
        {
            DbQueries db = SingleOf<DbQueries>.Obj;
            if (fetched.Priority >= 50)
            {

                _ = MainJoinedChannels.Remove(fetched);
                _ = MainJoinedChannelNames.Remove(channel);
                await _main.Client.PartChannel(channel);
            }
            else
            {
                _ = _anon.Client.PartChannel(channel);
            }

            db.RemoveChannel(fetched);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Errors occured whilst trying to part {channel} :", channel);
        }

        return true;
    }

    public static async Task ReloadFetchedChannels()
    {
        int pCount = FetchedChannels.Count;
        FetchedChannels = (await SingleOf<DbQueries>.Obj.GetChannels()).ToList();

        int cCount = FetchedChannels.Count;

        if (pCount != cCount)
            await MessageHandler.SendColoredMessage(AppConfig.RelayChannel, $"Channel size changed: {pCount} -> {cCount}", UserColors.YellowGreen);
    }

    private static void RegisterEvents(bool isReconnect)
    {
        if (isReconnect)
            return;

        _main.Client.OnChannelJoin += MainOnJoinedChannel;
        _main.Client.OnChannelPart += MainOnLeftChannel;
    }
    #endregion

    #region Client events
    private static ValueTask MainOnLeftChannel(IPartedChannel channel)
    {
        _ = MainJoinedChannels.Remove(FetchedChannels.First(x => x.Username == channel.Name));
        _ = MainJoinedChannelNames.Remove(channel.Name);
        return ValueTask.CompletedTask;
    }

    private static ValueTask MainOnJoinedChannel(IrcChannel channel)
    {
        MainJoinedChannels.Add(FetchedChannels.First(x => x.Username == channel.Name));
        MainJoinedChannelNames.Add(channel.Name);
        return ValueTask.CompletedTask;
    }
    #endregion

    public sealed record Channel(string Name, int Priority, bool Logged);
}
