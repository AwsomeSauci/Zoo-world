using System;
using System.Collections.Generic;
using ZooWorld.Domain.Animals;

namespace ZooWorld.Application.Animals
{
    public interface IAnimalLookup
    {
        bool TryGetAlive(AnimalId id, out AnimalInstance animal);
    }
}
