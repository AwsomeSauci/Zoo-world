using ZooWorld.Domain.Common;

namespace ZooWorld.Domain.Movement
{
    public readonly struct MotionIntent
    {
        public PlanarVector Velocity { get; }
        public float VisualElevation { get; }

        public MotionIntent(PlanarVector velocity, float visualElevation = 0)
        {
            Velocity = velocity;
            VisualElevation = visualElevation;
        }
    }
}
