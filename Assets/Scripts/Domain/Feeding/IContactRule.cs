using ZooWorld.Domain.Animals;

namespace ZooWorld.Domain.Feeding
{
    public interface IContactRule
    {
        // A pure decision shared by main-thread gameplay and the physics contact filter.
        bool TryResolve(AnimalId a, FoodRole roleA, AnimalId b, FoodRole roleB, out Consumption consumption);
    }
}
