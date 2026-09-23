using System;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Application.Animals
{
    public interface IAnimalTickModule : IAnimalModule
    {
        void Tick(float deltaTime);
    }
}
