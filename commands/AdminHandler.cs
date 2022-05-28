using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _26tack_rewritten.commands.adminset;
using _26tack_rewritten.interfaces;

namespace _26tack_rewritten.commands;
internal class AdminHandler : ChatCommandHandler
{
    public AdminHandler()
    {
        Prefix = "func_";
        UseUnifiedCooldowns = true;
        Cooldowns = new int[] { 0, 0 };

        AddCommand(new Massping());
    }
}
