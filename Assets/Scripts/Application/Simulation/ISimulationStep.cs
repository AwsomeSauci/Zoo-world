using System;
using ZooWorld.Application.Animals;
using ZooWorld.Domain.Common;

namespace ZooWorld.Application.Simulation
{
    public interface ISimulationStep
    {
        void Tick(float deltaTime);
    }
}
