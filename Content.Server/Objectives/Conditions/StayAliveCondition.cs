﻿#nullable enable
using Content.Server.Objectives.Interfaces;
using JetBrains.Annotations;
using Robust.Shared.Localization;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;

namespace Content.Server.Objectives.Conditions
{
    [UsedImplicitly]
    [DataDefinition]
    public class StayAliveCondition : IObjectiveCondition
    {
        private Mind.Mind? _mind;

        public IObjectiveCondition GetAssigned(Mind.Mind mind)
        {
            return new StayAliveCondition {_mind = mind};
        }

        public string Title => Loc.GetString("Stay alive.");

        public string Description => Loc.GetString("Survive this shift, we need you for another assignment.");

        public SpriteSpecifier Icon => new SpriteSpecifier.Rsi(new ResourcePath("Objects/Misc/skub.rsi"), "icon"); //didn't know what else would have been a good icon for staying alive

        public float Progress => (_mind?.CharacterDeadIC ?? false) ? 0f : 1f;

        public float Difficulty => 1f;

        public bool Equals(IObjectiveCondition? other)
        {
            return other is StayAliveCondition sac && Equals(_mind, sac._mind);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((StayAliveCondition) obj);
        }

        public override int GetHashCode()
        {
            return (_mind != null ? _mind.GetHashCode() : 0);
        }
    }
}
