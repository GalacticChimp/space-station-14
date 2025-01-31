﻿using Content.Client.HUD;
using Content.Shared.CombatMode;
using Content.Shared.Targeting;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Client.CombatMode
{
    [RegisterComponent]
    [ComponentReference(typeof(SharedCombatModeComponent))]
    public sealed class CombatModeComponent : SharedCombatModeComponent
    {
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IGameHud _gameHud = default!;

        public override bool IsInCombatMode
        {
            get => base.IsInCombatMode;
            set
            {
                base.IsInCombatMode = value;
                UpdateHud();
            }
        }

        public override TargetingZone ActiveZone
        {
            get => base.ActiveZone;
            set
            {
                base.ActiveZone = value;
                UpdateHud();
            }
        }

        public override void HandleMessage(ComponentMessage message, IComponent? component)
        {
            base.HandleMessage(message, component);

            switch (message)
            {
                case PlayerAttachedMsg _:
                    _gameHud.CombatPanelVisible = true;
                    UpdateHud();
                    break;

                case PlayerDetachedMsg _:
                    _gameHud.CombatPanelVisible = false;
                    break;
            }
        }

        private void UpdateHud()
        {
            if (Owner != _playerManager.LocalPlayer?.ControlledEntity)
            {
                return;
            }

            _gameHud.TargetingZone = ActiveZone;
        }
    }
}
