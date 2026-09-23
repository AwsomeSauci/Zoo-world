using System;
using ZooWorld.Domain.Common;

namespace ZooWorld.Domain.Movement
{
    public sealed class WanderBehaviour : ISteeringBehaviour
    {
        private readonly IRandomSource random;
        private readonly float minInterval;
        private readonly float maxInterval;
        private float remaining;
        private PlanarVector direction;

        public WanderBehaviour(IRandomSource random, float minInterval, float maxInterval)
        {
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            this.minInterval = Guard.Positive(minInterval, nameof(minInterval));
            this.maxInterval = Guard.Positive(maxInterval, nameof(maxInterval));
            if (maxInterval < minInterval) throw new ArgumentException("Wander interval range is inverted.");
        }

        public PlanarVector Evaluate(PlanarVector position, in WorldBounds bounds, float deltaTime)
        {
            remaining -= deltaTime;
            if (remaining <= 0)
            {
                var angle = random.NextUnit() * Math.PI * 2;
                direction = new PlanarVector((float)Math.Cos(angle), (float)Math.Sin(angle));
                remaining = minInterval + random.NextUnit() * (maxInterval - minInterval);
            }
            return direction;
        }
    }
}
