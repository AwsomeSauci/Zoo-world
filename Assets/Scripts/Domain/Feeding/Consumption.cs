using ZooWorld.Domain.Animals;

namespace ZooWorld.Domain.Feeding
{
    public readonly struct Consumption
    {
        public AnimalId Eater { get; }
        public AnimalId Victim { get; }
        public FoodRole VictimRole { get; }

        public Consumption(AnimalId eater, AnimalId victim, FoodRole victimRole)
        {
            Eater = eater;
            Victim = victim;
            VictimRole = victimRole;
        }
    }
}
