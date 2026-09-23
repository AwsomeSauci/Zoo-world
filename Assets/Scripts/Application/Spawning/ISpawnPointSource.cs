using ZooWorld.Application.Animals;
using ZooWorld.Domain.Common;

namespace ZooWorld.Application.Spawning
{
    public interface ISpawnPointSource
    {
        bool TryFind(string speciesId, out PlanarVector position);
    }
}
