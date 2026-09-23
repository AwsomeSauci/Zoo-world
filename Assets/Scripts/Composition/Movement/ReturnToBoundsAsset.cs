using System;
using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Composition.Movement
{
    [CreateAssetMenu(menuName = "Zoo World/Steering/Return To Bounds")]
    public sealed class ReturnToBoundsAsset : SteeringAsset
    {
        [SerializeField] private SteeringAsset inner;
        [SerializeField, Min(0)] private float returnInset = 1;

        protected override Func<IRandomSource, ISteeringBehaviour> Build(HashSet<SteeringAsset> path)
        {
            if (!inner) throw new InvalidOperationException($"'{name}' needs an inner steering behaviour.");
            var factory = inner.CreateFactory(path);
            var inset = returnInset;
            return random => new ReturnToBoundsBehaviour(factory(random), inset);
        }
    }
}
