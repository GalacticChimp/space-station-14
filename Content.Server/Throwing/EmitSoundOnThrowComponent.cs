using Content.Shared.Audio;
using Content.Shared.Throwing;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Throwing
{
    /// <summary>
    /// Simple sound emitter that emits sound on use in hand
    /// </summary>
    [RegisterComponent]
    public class EmitSoundOnThrowComponent : Component, ILand
    {
        /// <inheritdoc />
        ///
        public override string Name => "EmitSoundOnThrow";

        [DataField("sound")]
        public string? _soundName;
        [DataField("variation")]
        public float _pitchVariation;

        public void PlaySoundEffect()
        {
            if (!string.IsNullOrWhiteSpace(_soundName))
            {
                if (_pitchVariation > 0.0)
                {
                    SoundSystem.Play(Filter.Pvs(Owner), _soundName, Owner, AudioHelpers.WithVariation(_pitchVariation).WithVolume(-2f));
                }
                SoundSystem.Play(Filter.Pvs(Owner), _soundName, Owner, AudioParams.Default.WithVolume(-2f));
            }
        }

        void ILand.Land(LandEventArgs eventArgs)
        {
            PlaySoundEffect();
        }
    }
}
