﻿using Content.Shared.CharacterAppearance;
using Content.Shared.Cuffs.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;

namespace Content.Client.Cuffs.Components
{
    [RegisterComponent]
    public class CuffableComponent : SharedCuffableComponent
    {
        [ViewVariables]
        private string? _currentRSI;

        [ViewVariables] [ComponentDependency] private readonly SpriteComponent? _spriteComponent = null;

        public override void HandleComponentState(ComponentState? curState, ComponentState? nextState)
        {
            if (curState is not CuffableComponentState cuffState)
            {
                return;
            }

            CanStillInteract = cuffState.CanStillInteract;

            if (_spriteComponent != null)
            {
                _spriteComponent.LayerSetVisible(HumanoidVisualLayers.Handcuffs, cuffState.NumHandsCuffed > 0);
                _spriteComponent.LayerSetColor(HumanoidVisualLayers.Handcuffs, cuffState.Color);

                if (cuffState.NumHandsCuffed > 0)
                {
                    if (_currentRSI != cuffState.RSI) // we don't want to keep loading the same RSI
                    {
                        _currentRSI = cuffState.RSI;

                        if (_currentRSI != null)
                        {
                            _spriteComponent.LayerSetState(HumanoidVisualLayers.Handcuffs, new RSI.StateId(cuffState.IconState), new ResourcePath(_currentRSI));
                        }
                    }
                    else
                    {
                        _spriteComponent.LayerSetState(HumanoidVisualLayers.Handcuffs, new RSI.StateId(cuffState.IconState)); // TODO: safety check to see if RSI contains the state?
                    }
                }
            }
        }

        public override void OnRemove()
        {
            base.OnRemove();

            _spriteComponent?.LayerSetVisible(HumanoidVisualLayers.Handcuffs, false);
        }
    }
}
