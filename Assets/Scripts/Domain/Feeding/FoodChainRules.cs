using ZooWorld.Domain.Animals;

namespace ZooWorld.Domain.Feeding
{
    public sealed class FoodChainRules : IContactRule
    {
        public bool TryResolve(AnimalId a, FoodRole roleA, AnimalId b, FoodRole roleB, out Consumption consumption)
        {
            consumption = default;
            if (a == b || (roleA == FoodRole.Prey && roleB == FoodRole.Prey)) return false;
            var aWins = roleA == FoodRole.Predator && (roleB == FoodRole.Prey || a.CompareTo(b) < 0);
            consumption = aWins ? new Consumption(a, b, roleB) : new Consumption(b, a, roleA);
            return true;
        }
    }
}
