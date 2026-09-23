using System.Collections.Generic;
using ZooWorld.Application.Simulation;
using ZooWorld.Domain.Animals;

namespace ZooWorld.Application.Animals
{
    public interface IAnimalDeathService
    {
        bool TryKill(AnimalId id);
    }
}
