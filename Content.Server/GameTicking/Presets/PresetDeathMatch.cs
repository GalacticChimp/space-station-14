﻿using System.Collections.Generic;
using Content.Server.GameTicking.Rules;
using Robust.Server.Player;
using Robust.Shared.IoC;

namespace Content.Server.GameTicking.Presets
{
    [GamePreset("deathmatch")]
    public sealed class PresetDeathMatch : GamePreset
    {
        [Dependency] private readonly IGameTicker _gameTicker = default!;

        public override bool Start(IReadOnlyList<IPlayerSession> readyPlayers, bool force = false)
        {
            _gameTicker.AddGameRule<RuleDeathMatch>();
            return true;
        }

        public override string ModeTitle => "Deathmatch";
        public override string Description => "Kill anything that moves!";
    }
}
