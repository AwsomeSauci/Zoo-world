using ZooWorld.Application.Animals;

namespace ZooWorld.Application.Simulation
{
    public sealed class AnimalTickService : ISimulationStep
    {
        private readonly AnimalPopulation population;
        public AnimalTickService(AnimalPopulation population) => this.population = population;
        public void Tick(float deltaTime)
        {
            foreach (var animal in population.All) animal.Tick(deltaTime);
        }
    }
}
