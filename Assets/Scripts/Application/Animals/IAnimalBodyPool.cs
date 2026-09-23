using System;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Application.Animals
{
    public interface IAnimalBodyPool
    {
        IAnimalBody Rent(string speciesId, AnimalId id, PlanarVector position);
    }
}
