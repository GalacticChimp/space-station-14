#nullable enable
using System;
using Content.Server.APC;
using Content.Shared.Examine;
using Content.Shared.Power;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Physics;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;

namespace Content.Server.Power.Components
{
    /// <summary>
    ///     Attempts to link with a nearby <see cref="IPowerProvider"/>s so that it can receive power from a <see cref="IApcNet"/>.
    /// </summary>
    [RegisterComponent]
    public class PowerReceiverComponent : Component, IExamine
    {
        [ViewVariables] [ComponentDependency] private readonly IPhysBody? _physicsComponent = null;

        public override string Name => "PowerReceiver";

        [ViewVariables]
        public bool Powered => (HasApcPower || !NeedsPower) && !PowerDisabled;

        /// <summary>
        ///     If this is being powered by an Apc.
        /// </summary>
        [ViewVariables]
        public bool HasApcPower { get; private set; }

        /// <summary>
        ///     The max distance from a <see cref="PowerProviderComponent"/> that this can receive power from.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public int PowerReceptionRange { get => _powerReceptionRange; set => SetPowerReceptionRange(value); }
        [DataField("powerReceptionRange")]
        private int _powerReceptionRange = 3;

        [ViewVariables]
        public IPowerProvider Provider { get => _provider; set => SetProvider(value); }
        private IPowerProvider _provider = PowerProviderComponent.NullProvider;

        /// <summary>
        ///     If this should be considered for connection by <see cref="PowerProviderComponent"/>s.
        /// </summary>
        public bool Connectable => Anchored;

        private bool Anchored => _physicsComponent == null || _physicsComponent.BodyType == BodyType.Static;

        [ViewVariables]
        public bool NeedsProvider { get; private set; } = true;

        /// <summary>
        ///     Amount of charge this needs from an APC per second to function.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public int Load { get => _load; set => SetLoad(value); }
        [DataField("powerLoad")]
        private int _load = 5;

        /// <summary>
        ///     When false, causes this to appear powered even if not receiving power from an Apc.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool NeedsPower { get => _needsPower; set => SetNeedsPower(value); }
        [DataField("needsPower")]
        private bool _needsPower = true;

        /// <summary>
        ///     When true, causes this to never appear powered.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool PowerDisabled { get => _powerDisabled; set => SetPowerDisabled(value); }
        [DataField("powerDisabled")]
        private bool _powerDisabled;

        protected override void Startup()
        {
            base.Startup();
            if (NeedsProvider)
            {
                TryFindAndSetProvider();
            }
            if (_physicsComponent != null)
            {
                AnchorUpdate();
            }
        }

        public override void OnRemove()
        {
            _provider.RemoveReceiver(this);
            base.OnRemove();
        }

        public void TryFindAndSetProvider()
        {
            if (TryFindAvailableProvider(out var provider))
            {
                Provider = provider;
            }
        }

        public void ApcPowerChanged()
        {
            var oldPowered = Powered;
            HasApcPower = Provider.HasApcPower;
            if (Powered != oldPowered)
                OnNewPowerState();
        }

        private bool TryFindAvailableProvider(out IPowerProvider foundProvider)
        {
            var nearbyEntities = IoCManager.Resolve<IEntityLookup>()
                .GetEntitiesInRange(Owner, PowerReceptionRange);

            foreach (var entity in nearbyEntities)
            {
                if (entity.TryGetComponent<PowerProviderComponent>(out var provider))
                {
                    if (provider.Connectable)
                    {
                        if (provider.Owner.Transform.Coordinates.TryDistance(Owner.EntityManager, Owner.Transform.Coordinates, out var distance))
                        {
                            if (distance < Math.Min(PowerReceptionRange, provider.PowerTransferRange))
                            {
                                foundProvider = provider;
                                return true;
                            }
                        }
                    }
                }
            }
            foundProvider = default!;
            return false;
        }

        public void ClearProvider()
        {
            _provider.RemoveReceiver(this);
            _provider = PowerProviderComponent.NullProvider;
            NeedsProvider = true;
            ApcPowerChanged();
        }

        private void SetProvider(IPowerProvider newProvider)
        {
            _provider.RemoveReceiver(this);
            _provider = newProvider;
            newProvider.AddReceiver(this);
            NeedsProvider = false;
            ApcPowerChanged();
        }

        private void SetPowerReceptionRange(int newPowerReceptionRange)
        {
            ClearProvider();
            _powerReceptionRange = newPowerReceptionRange;
            TryFindAndSetProvider();
        }

        private void SetLoad(int newLoad)
        {
            Provider.UpdateReceiverLoad(Load, newLoad);
            _load = newLoad;
        }

        private void SetNeedsPower(bool newNeedsPower)
        {
            var oldPowered = Powered;
            _needsPower = newNeedsPower;
            if (oldPowered != Powered)
            {
                OnNewPowerState();
            }
        }

        private void SetPowerDisabled(bool newPowerDisabled)
        {
            var oldPowered = Powered;
            _powerDisabled = newPowerDisabled;
            if (oldPowered != Powered)
            {
                OnNewPowerState();
            }
        }

        private void OnNewPowerState()
        {
            SendMessage(new PowerChangedMessage(Powered));

            if (Owner.TryGetComponent<AppearanceComponent>(out var appearance))
            {
                appearance.SetData(PowerDeviceVisuals.Powered, Powered);
            }
        }

        public void AnchorUpdate()
        {
            if (Anchored)
            {
                if (NeedsProvider)
                {
                    TryFindAndSetProvider();
                }
            }
            else
            {
                ClearProvider();
            }
        }

        ///<summary>
        ///Adds some markup to the examine text of whatever object is using this component to tell you if it's powered or not, even if it doesn't have an icon state to do this for you.
        ///</summary>
        public void Examine(FormattedMessage message, bool inDetailsRange)
        {
            message.AddMarkup(Loc.GetString("It appears to be {0}.", Powered ? "[color=darkgreen]powered[/color]" : "[color=darkred]un-powered[/color]"));
        }
    }

    public class PowerChangedMessage : ComponentMessage
    {
        public readonly bool Powered;

        public PowerChangedMessage(bool powered)
        {
            Powered = powered;
        }
    }
}
