﻿using System;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.IoC;

namespace Content.Server.GameTicking.Commands
{
    [AdminCommand(AdminFlags.Server)]
    class StartRoundCommand : IConsoleCommand
    {
        public string Command => "startround";
        public string Description => "Ends PreRoundLobby state and starts the round.";
        public string Help => String.Empty;

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var ticker = IoCManager.Resolve<IGameTicker>();

            if (ticker.RunLevel != GameRunLevel.PreRoundLobby)
            {
                shell.WriteLine("This can only be executed while the game is in the pre-round lobby.");
                return;
            }

            ticker.StartRound();
        }
    }
}
