#region Using

using System;
using System.IO;
using Libra;
using Libra.Graphics;
using Libra.Graphics.Toolkit;
using Libra.IO;
using Noctua.Asset;

#endregion

namespace Noctua.Serialization
{
    public sealed class ParticleSystemSerializer : AssetSerializer<ParticleSystem>
    {
        public override object ReadAsset(Stream stream, IResource resource)
        {
            var definition = ReadObject<ParticleSystemDefinition>(stream);

            var particleSystem = new ParticleSystem(DeviceContext, definition.MaxParticleCount)
            {
                Name = definition.Name,
                Duration = definition.Duration,
                DurationRandomness = definition.DurationRandomness,
                EmitterVelocitySensitivity = definition.EmitterVelocitySensitivity,
                MinHorizontalVelocity = definition.MinHorizontalVelocity,
                MaxHorizontalVelocity = definition.MaxHorizontalVelocity,
                MinVerticalVelocity = definition.MinVerticalVelocity,
                MaxVerticalVelocity = definition.MaxVerticalVelocity,
                Gravity = definition.Gravity,
                EndVelocity = definition.EndVelocity,
                MinColor = definition.MinColor,
                MaxColor = definition.MaxColor,
                MinRotateSpeed = definition.MinRotateSpeed,
                MaxRotateSpeed = definition.MaxRotateSpeed,
                MinStartSize = definition.MinStartSize,
                MaxStartSize = definition.MaxStartSize,
                MinEndSize = definition.MinEndSize,
                MaxEndSize = definition.MaxEndSize,
                BlendState = definition.BlendState
            };

            particleSystem.Texture = Load<Texture2D>(resource, definition.Texture);

            return particleSystem;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var particleSystem = asset as ParticleSystem;

            if (particleSystem.Texture == null)
                throw new InvalidOperationException(string.Format("ParticleSystem '{0}' have no texture.", particleSystem.Name));

            var definition = new ParticleSystemDefinition
            {
                Name = particleSystem.Name,
                MaxParticleCount = particleSystem.MaxParticleCount,
                Duration = particleSystem.Duration,
                DurationRandomness = particleSystem.DurationRandomness,
                EmitterVelocitySensitivity = particleSystem.EmitterVelocitySensitivity,
                MinHorizontalVelocity = particleSystem.MinHorizontalVelocity,
                MaxHorizontalVelocity = particleSystem.MaxHorizontalVelocity,
                MinVerticalVelocity = particleSystem.MinVerticalVelocity,
                MaxVerticalVelocity = particleSystem.MaxVerticalVelocity,
                Gravity = particleSystem.Gravity,
                EndVelocity = particleSystem.EndVelocity,
                MinColor = particleSystem.MinColor,
                MaxColor = particleSystem.MaxColor,
                MinRotateSpeed = particleSystem.MinRotateSpeed,
                MaxRotateSpeed = particleSystem.MaxRotateSpeed,
                MinStartSize = particleSystem.MinStartSize,
                MaxStartSize = particleSystem.MaxStartSize,
                MinEndSize = particleSystem.MinEndSize,
                MaxEndSize = particleSystem.MaxEndSize,
                Texture = CreateRelativeUri(resource, particleSystem.Texture),
                BlendState = particleSystem.BlendState
            };

            WriteObject(stream, definition);
        }
    }
}
