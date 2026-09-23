using System.Collections.Generic;
using ZooWorld.Composition.Spawning;
using ZooWorld.Unity.Physics;

namespace ZooWorld.Composition.Animals
{
    public sealed class AnimalPrefabCatalog : IAnimalPrefabCatalog
    {
        private readonly Dictionary<string, AnimalBody> prefabs = new Dictionary<string, AnimalBody>();
        public AnimalPrefabCatalog(IReadOnlyList<AnimalSpawnEntry> animals)
        {
            foreach (var entry in animals) prefabs.Add(entry.Animal.SpeciesId, entry.Animal.Prefab);
        }

        public AnimalBody Get(string speciesId) => prefabs[speciesId];
    }
}
