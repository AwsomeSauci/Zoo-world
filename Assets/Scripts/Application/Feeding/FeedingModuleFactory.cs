using System;
using System.Collections.Generic;
using ZooWorld.Application.Animals;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Feeding;

namespace ZooWorld.Application.Feeding
{
    public sealed class FeedingModuleFactory : IAnimalModuleFactory
    {
        private readonly FoodChainRegistry registry;
        private readonly FoodRole role;
        public FeedingModuleFactory(FoodChainRegistry registry, FoodRole role)
        {
            this.registry = registry;
            this.role = role;
        }

        public IAnimalModule Create(AnimalId id, IAnimalBody body) => new FeedingModule(id, registry, role);
    }
}
