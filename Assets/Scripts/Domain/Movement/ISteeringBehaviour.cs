using System;
using ZooWorld.Domain.Common;

namespace ZooWorld.Domain.Movement
{
    public interface ISteeringBehaviour
    {
        PlanarVector Evaluate(PlanarVector position, in WorldBounds bounds, float deltaTime);
    }
}
