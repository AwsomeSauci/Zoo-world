using ZooWorld.Application.Animals;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Application.Movement
{
    public sealed class MovementModule : IMovementModule
    {
        private readonly IAnimalBody body;
        private readonly ISteeringBehaviour steering;
        private readonly ILocomotionBehaviour locomotion;
        private readonly IWorldBoundsProvider bounds;

        public MovementModule(IAnimalBody body, ISteeringBehaviour steering, ILocomotionBehaviour locomotion,
            IWorldBoundsProvider bounds)
        {
            this.body = body;
            this.steering = steering;
            this.locomotion = locomotion;
            this.bounds = bounds;
        }

        public void Tick(float deltaTime)
        {
            var area = bounds.Bounds;
            var direction = steering.Evaluate(body.Position, in area, deltaTime);
            body.ApplyMotion(locomotion.Step(direction, deltaTime), deltaTime);
        }

        public void Dispose() { }
    }
}
