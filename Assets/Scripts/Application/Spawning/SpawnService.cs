using ZooWorld.Application.Animals;
using ZooWorld.Domain.Common;

namespace ZooWorld.Application.Spawning
{
    public sealed class SpawnService : IAnimalSpawner
    {
        private readonly AnimalFactory factory;
        private readonly AnimalPopulation population;
        private readonly ISpawnPointSource positions;

        public SpawnService(AnimalFactory factory, AnimalPopulation population, ISpawnPointSource positions)
        {
            this.factory = factory;
            this.population = population;
            this.positions = positions;
        }

        public bool TrySpawn(string speciesId)
        {
            if (!positions.TryFind(speciesId, out var position)) return false;
            var animal = factory.Create(speciesId, position);
            try
            {
                population.Add(animal);
                animal.Body.Activate();
                return true;
            }
            catch
            {
                population.Remove(animal.Id);
                animal.Dispose();
                throw;
            }
        }
    }
}
