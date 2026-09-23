using ZooWorld.Domain.Common;

namespace ZooWorld.Domain.Movement
{
    public sealed class LinearLocomotion : ILocomotionBehaviour
    {
        private readonly float speed;
        public LinearLocomotion(float speed) => this.speed = Guard.Positive(speed, nameof(speed));
        public MotionIntent Step(PlanarVector direction, float deltaTime) => new MotionIntent(direction * speed);
    }
}
