using ZooWorld.Application.Feeding;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Feeding;

namespace ZooWorld.Application.Contacts
{
    public sealed class FoodChainContactPolicy : IContactResponsePolicy
    {
        private readonly FoodChainRegistry foodChain;
        private readonly IContactRule rule;

        public FoodChainContactPolicy(FoodChainRegistry foodChain, IContactRule rule)
        {
            this.foodChain = foodChain;
            this.rule = rule;
        }

        public bool ShouldSuppressImpulse(AnimalId first, AnimalId second) =>
            foodChain.TryGet(first, out var firstRole) &&
            foodChain.TryGet(second, out var secondRole) &&
            rule.TryResolve(first, firstRole, second, secondRole, out _);
    }
}
