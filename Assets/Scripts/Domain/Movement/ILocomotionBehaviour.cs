using System;
using ZooWorld.Domain.Common;

namespace ZooWorld.Domain.Movement
{
    public interface ILocomotionBehaviour
    {
        MotionIntent Step(PlanarVector direction, float deltaTime);
    }
}
