using System.Collections.Generic;
using ZooWorld.Application.Simulation;
using ZooWorld.Domain.Animals;

namespace ZooWorld.Application.Animals
{
    public sealed class AnimalDeathService : IAnimalDeathService, ISimulationStep
    {
        private readonly AnimalPopulation population;
        private readonly Queue<AnimalInstance> pending = new Queue<AnimalInstance>();
        public AnimalDeathService(AnimalPopulation population) => this.population = population;

        public bool TryKill(AnimalId id)
        {
            if (!population.TryGetAlive(id, out var animal) || !animal.TryMarkDead()) return false;
            pending.Enqueue(animal);
            return true;
        }

        public void Tick(float deltaTime)
        {
            while (pending.Count > 0)
            {
                var animal = pending.Dequeue();
                population.Remove(animal.Id);
                animal.Dispose();
            }
        }
    }
}
