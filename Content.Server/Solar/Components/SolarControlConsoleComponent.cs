﻿#nullable enable
using Content.Server.Power.Components;
using Content.Server.Solar.EntitySystems;
using Content.Server.UserInterface;
using Content.Shared.Interaction;
using Content.Shared.Solar;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.ViewVariables;

namespace Content.Server.Solar.Components
{
    [RegisterComponent]
    [ComponentReference(typeof(IActivate))]
    public class SolarControlConsoleComponent : SharedSolarControlConsoleComponent, IActivate
    {
        [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

        private PowerSolarSystem _powerSolarSystem = default!;
        private bool Powered => !Owner.TryGetComponent(out PowerReceiverComponent? receiver) || receiver.Powered;

        [ViewVariables] private BoundUserInterface? UserInterface => Owner.GetUIOrNull(SolarControlConsoleUiKey.Key);

        public override void Initialize()
        {
            base.Initialize();

            if (UserInterface != null)
            {
                UserInterface.OnReceiveMessage += UserInterfaceOnReceiveMessage;
            }

            Owner.EnsureComponent<PowerReceiverComponent>();
            _powerSolarSystem = _entitySystemManager.GetEntitySystem<PowerSolarSystem>();
        }

        public void UpdateUIState()
        {
            UserInterface?.SetState(new SolarControlConsoleBoundInterfaceState(_powerSolarSystem.TargetPanelRotation, _powerSolarSystem.TargetPanelVelocity, _powerSolarSystem.TotalPanelPower, _powerSolarSystem.TowardsSun));
        }

        private void UserInterfaceOnReceiveMessage(ServerBoundUserInterfaceMessage obj)
        {
            switch (obj.Message)
            {
                case SolarControlConsoleAdjustMessage msg:
                    if (double.IsFinite(msg.Rotation))
                    {
                        _powerSolarSystem.TargetPanelRotation = msg.Rotation.Reduced();
                    }
                    if (double.IsFinite(msg.AngularVelocity))
                    {
                        _powerSolarSystem.TargetPanelVelocity = msg.AngularVelocity.Reduced();
                    }
                    break;
            }
        }

        void IActivate.Activate(ActivateEventArgs eventArgs)
        {
            if (!eventArgs.User.TryGetComponent(out ActorComponent? actor))
            {
                return;
            }

            if (!Powered)
            {
                return;
            }

            // always update the UI immediately before opening, just in case
            UpdateUIState();
            UserInterface?.Open(actor.PlayerSession);
        }
    }
}
