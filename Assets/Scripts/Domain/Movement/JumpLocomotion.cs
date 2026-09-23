using System;
using ZooWorld.Domain.Common;

namespace ZooWorld.Domain.Movement
{
    public sealed class JumpLocomotion : ILocomotionBehaviour
    {
        private readonly float distance;
        private readonly float duration;
        private readonly float interval;
        private readonly float height;
        private float phase;
        private PlanarVector jumpDirection;
        private bool starting = true;

        public JumpLocomotion(float distance, float duration, float interval, float height)
        {
            this.distance = Guard.Positive(distance, nameof(distance));
            this.duration = Guard.Positive(duration, nameof(duration));
            this.interval = Guard.Positive(interval, nameof(interval));
            this.height = Guard.NonNegative(height, nameof(height));
            if (interval < duration) throw new ArgumentException("Jump interval cannot be shorter than its duration.");
        }

        public MotionIntent Step(PlanarVector direction, float deltaTime)
        {
            Guard.Positive(deltaTime, nameof(deltaTime));
            var remaining = deltaTime;
            var displacement = PlanarVector.Zero;
            while (remaining > 0)
            {
                if (starting) { jumpDirection = direction; starting = false; }
                var active = phase < duration;
                var boundary = active ? duration : interval;
                var step = Math.Min(remaining, boundary - phase);
                if (active) displacement += jumpDirection * (distance / duration * step);
                phase += step;
                remaining -= step;
                if (phase >= interval - 0.000001f) { phase = 0; starting = true; }
                else if (Math.Abs(phase - duration) < 0.000001f) phase = duration;
            }
            var t = Math.Min(phase / duration, 1);
            return new MotionIntent(displacement / deltaTime, 4 * height * t * (1 - t));
        }
    }
}
