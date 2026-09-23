using System;
using ZooWorld.Application.Animals;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Application.Movement
{
    public sealed class MovementModuleFactory : IAnimalModuleFactory
    {
        private readonly Func<IRandomSource, ISteeringBehaviour> steering;
        private readonly Func<ILocomotionBehaviour> locomotion;
        private readonly IWorldBoundsProvider bounds;
        private readonly IAnimalRandomFactory random;

        public MovementModuleFactory(Func<IRandomSource, ISteeringBehaviour> steering,
            Func<ILocomotionBehaviour> locomotion,
            IWorldBoundsProvider bounds, IAnimalRandomFactory random)
        {
            this.steering = steering;
            this.locomotion = locomotion;
            this.bounds = bounds;
            this.random = random;
        }

        public IAnimalModule Create(AnimalId id, IAnimalBody body) =>
            new MovementModule(body, steering(random.Create(id)), locomotion(), bounds);
    }
}
