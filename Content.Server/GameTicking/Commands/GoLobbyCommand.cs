﻿#nullable enable
using System;
using Content.Server.Administration;
using Content.Shared;
using Content.Shared.Administration;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.IoC;

namespace Content.Server.GameTicking.Commands
{
    [AdminCommand(AdminFlags.Server)]
    public class GoLobbyCommand : IConsoleCommand
    {
        public string Command => "golobby";
        public string Description => "Enables the lobby and restarts the round.";
        public string Help => $"Usage: {Command} / {Command} <preset>";
        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            Type? preset = null;
            var presetName = string.Join(" ", args);

            var ticker = IoCManager.Resolve<IGameTicker>();

            if (args.Length > 0)
            {
                if (!ticker.TryGetPreset(presetName, out preset))
                {
                    shell.WriteLine($"No preset found with name {presetName}");
                    return;
                }
            }

            var config = IoCManager.Resolve<IConfigurationManager>();
            config.SetCVar(CCVars.GameLobbyEnabled, true);

            ticker.RestartRound();

            if (preset != null)
            {
                ticker.SetStartPreset(preset);
            }

            shell.WriteLine($"Enabling the lobby and restarting the round.{(preset == null ? "" : $"\nPreset set to {presetName}")}");
        }
    }
}
