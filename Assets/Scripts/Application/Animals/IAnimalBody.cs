using System;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Application.Animals
{
    public interface IAnimalBody : IDisposable
    {
        PlanarVector Position { get; }
        void ApplyMotion(in MotionIntent intent, float deltaTime);
        void Activate();
        void Deactivate();
    }
}
