using ZooWorld.Application.Animals;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Application.Movement
{
    public interface IWorldBoundsProvider
    {
        WorldBounds Bounds { get; }
    }
}
