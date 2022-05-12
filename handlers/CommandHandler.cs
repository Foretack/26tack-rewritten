using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Models;
using _26tack_rewritten.models;

namespace _26tack_rewritten.handlers;
internal class CommandHandler
{
    public static async Task HandleCommand(CommandContext ctx)
    {

    }
}

public record CommandContext(ChatMessage IrcMessage, string[] Args, string CommandName, Permission Permission);
