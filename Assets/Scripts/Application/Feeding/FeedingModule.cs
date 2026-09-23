using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Feeding;

namespace ZooWorld.Application.Feeding
{
    public sealed class FeedingModule : IFeedingModule
    {
        private readonly AnimalId id;
        private readonly FoodChainRegistry registry;
        public FeedingModule(AnimalId id, FoodChainRegistry registry, FoodRole role)
        {
            this.id = id;
            this.registry = registry;
            registry.Register(id, role);
        }

        public void Dispose() => registry.Unregister(id);
    }
}
