using System.Reflection;
using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.models;
using Serilog;

namespace _26tack_rewritten.commands.adminset;
internal class Debug : IChatCommand
{
    public Command Info()
    {
        string name = "debug";
        string description = "command for testing stuff! Xdxd";
        PermissionLevels permission = PermissionLevels.Whitelisted;

        return new Command(name, description, permission: permission);
    }

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0) return;

        try
        {
            await Task.Run(() =>
            {
                string[] splitCall = args[0].Split('.');

                Type obj = Type.GetType(splitCall[0])!;
                MethodInfo method = obj.GetMethod(GetMethodName(splitCall[1]))!;
                method.Invoke(obj, new object[] { user, channel });
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "xd");
        }
    }

    private string GetMethodName(string methodString)
    {
        int i = methodString.IndexOf('(');
        return methodString.Substring(0, i);
    }

    private string GetMethodFirstParam(string methodString)
    {
        int start = methodString.IndexOf('(');
        int end = methodString.IndexOf(')');
        return methodString.Substring(start, end);
    }
}
