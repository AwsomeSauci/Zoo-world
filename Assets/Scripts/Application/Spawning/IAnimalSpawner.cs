using ZooWorld.Application.Animals;
using ZooWorld.Domain.Common;

namespace ZooWorld.Application.Spawning
{
    public interface IAnimalSpawner
    {
        bool TrySpawn(string speciesId);
    }
}
