using System;
using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Composition.Movement
{
    [CreateAssetMenu(menuName = "Zoo World/Steering/Wander")]
    public sealed class WanderAsset : SteeringAsset
    {
        [SerializeField, Min(0.01f)] private float minimumInterval = 1.5f;
        [SerializeField, Min(0.01f)] private float maximumInterval = 3.5f;

        protected override Func<IRandomSource, ISteeringBehaviour> Build(HashSet<SteeringAsset> path)
        {
            var minimum = minimumInterval;
            var maximum = maximumInterval;
            return random => new WanderBehaviour(random, minimum, maximum);
        }
    }
}
