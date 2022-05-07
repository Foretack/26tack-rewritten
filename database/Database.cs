using _26tack_rewritten.handlers;
using Serilog;

namespace _26tack_rewritten.database;
internal class Database : DbConnection
{
    public async Task<bool> LogException(Exception exception)
    {
        var result = await
            Insert()
            .Table("errors")
            .Schema(new string[]
            {
                "data",
                "time"
            })
            .Values(new string[]
            {
                $"'{exception}'", // TODO: Format exception method
                $"CURRENT_TIMESTAMP"
            })
            .TryExecute();

        if (!result.Success)
        {
            // TODO: Discord message
            return false;
        }
        return true;
    }

    public async Task<bool> AddChannel(ChannelHandler.Channel channel)
    {
        var result = await
            Insert()
            .Table("channels")
            .Schema(new string[]
            {
                "name",
                "id",
                "priority",
                "logged",
                "date_joined"
            })
            .Values(new string[]
            {
                $"'{channel.Name}'",
                $"{channel.ID}",
                $"{channel.Priority}",
                "CURRENT_DATE"
            })
            .TryExecute();

        if (!result.Success)
        {
            // TODO: MainClient message
            return false;
        }
        return true;
    }

    public async Task<ChannelHandler.Channel[]> GetChannels()
    {
        var result = await
            Select()
            .Table("channels")
            .Schema(new string[]
            {
                "name",
                "id",
                "priority",
                "logged",
            })
            .TryExecute();

        if (!result.Success)
        {
            Log.Fatal("Failed to fetch channel list");
            throw new MissingFieldException("Channel list could not be loaded");
        }

        List<ChannelHandler.Channel> channels = new List<ChannelHandler.Channel>();
        foreach (object[] row in result.Results!)
        {
            channels.Add(new ChannelHandler.Channel((string)row[0], (string)row[1], (int)row[2], (bool)row[3]));
        }

        return channels.ToArray();
    }
}

