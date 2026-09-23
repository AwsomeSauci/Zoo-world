using System;
using ZooWorld.Domain.Common;

namespace ZooWorld.Domain.Movement
{
    public sealed class ReturnToBoundsBehaviour : ISteeringBehaviour
    {
        // Allow normal pauses between jumps; solver bounces alone do not count as progress.
        private const float ProgressTimeout = 3;
        private const float MinimumProgress = 0.1f;
        private const float DetourDuration = 3;
        private readonly ISteeringBehaviour inner;
        private readonly float inset;
        private bool returning;
        private WorldBounds returnBounds;
        private float bestDistance;
        private float stalledFor;
        private float detourRemaining;
        private PlanarVector detourDirection;

        public ReturnToBoundsBehaviour(ISteeringBehaviour inner, float inset)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
            this.inset = Guard.NonNegative(inset, nameof(inset));
        }

        public PlanarVector Evaluate(PlanarVector position, in WorldBounds bounds, float deltaTime)
        {
            var wandering = inner.Evaluate(position, bounds, deltaTime);
            if (!returning)
            {
                if (bounds.Contains(position)) return wandering;
                returning = true;
                ResetRecovery(position, in bounds);
            }
            else if ((returnBounds.Min - bounds.Min).LengthSquared > 0.000001f ||
                     (returnBounds.Max - bounds.Max).LengthSquared > 0.000001f)
                ResetRecovery(position, in bounds);

            var safeInset = Math.Min(inset, Math.Min(bounds.Max.X - bounds.Min.X, bounds.Max.Y - bounds.Min.Y) * 0.25f);
            if (bounds.Contains(position, safeInset))
            {
                returning = false;
                return wandering;
            }

            var toCenter = bounds.Center - position;
            if (detourRemaining > 0)
            {
                detourRemaining -= deltaTime;
                if (detourRemaining <= 0)
                {
                    bestDistance = toCenter.Length;
                    stalledFor = 0;
                }
                return detourDirection;
            }

            var direction = toCenter.Normalized;
            if (toCenter.Length < bestDistance - MinimumProgress)
            {
                bestDistance = toCenter.Length;
                stalledFor = 0;
            }
            else stalledFor += deltaTime;

            if (stalledFor < ProgressTimeout) return direction;

            // Keep one side for the whole detour, including pauses between jumps. A small
            // inward component lets physics slide along the wall until its edge is cleared.
            var tangent = new PlanarVector(-direction.Y, direction.X);
            detourDirection = (tangent + direction * 0.25f).Normalized;
            detourRemaining = DetourDuration;
            return detourDirection;
        }

        private void ResetRecovery(PlanarVector position, in WorldBounds bounds)
        {
            returnBounds = bounds;
            bestDistance = (bounds.Center - position).Length;
            stalledFor = 0;
            detourRemaining = 0;
        }
    }
}
