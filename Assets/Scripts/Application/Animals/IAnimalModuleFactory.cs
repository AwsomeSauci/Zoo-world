using System;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Application.Animals
{
    public interface IAnimalModuleFactory
    {
        IAnimalModule Create(AnimalId id, IAnimalBody body);
    }
}
