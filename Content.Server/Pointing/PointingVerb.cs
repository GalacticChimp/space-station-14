using Content.Server.Pointing.Components;
using Content.Server.Pointing.EntitySystems;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;

namespace Content.Server.Pointing
{
    /// <summary>
    ///     Global verb that points at an entity.
    /// </summary>
    [GlobalVerb]
    public class PointingVerb : GlobalVerb
    {
        public override bool RequireInteractionRange => false;

        public override void GetData(IEntity user, IEntity target, VerbData data)
        {
            data.Visibility = VerbVisibility.Invisible;
            data.IconTexture = "/Textures/Interface/VerbIcons/point.svg.192dpi.png";

            if (!user.HasComponent<ActorComponent>())
            {
                return;
            }

            if (!EntitySystem.Get<PointingSystem>().InRange(user, target.Transform.Coordinates))
            {
                return;
            }

            if (target.HasComponent<PointingArrowComponent>())
            {
                return;
            }

            data.Visibility = VerbVisibility.Visible;

            data.Text = Loc.GetString("Point at");
        }

        public override void Activate(IEntity user, IEntity target)
        {
            if (!user.TryGetComponent(out ActorComponent? actor))
            {
                return;
            }

            EntitySystem.Get<PointingSystem>().TryPoint(actor.PlayerSession, target.Transform.Coordinates, target.Uid);
        }
    }
}
