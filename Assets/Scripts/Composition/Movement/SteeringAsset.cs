using System;
using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Composition.Movement
{
    public abstract class SteeringAsset : ScriptableObject
    {
        public Func<IRandomSource, ISteeringBehaviour> CreateFactory() => CreateFactory(new HashSet<SteeringAsset>());

        internal Func<IRandomSource, ISteeringBehaviour> CreateFactory(HashSet<SteeringAsset> path)
        {
            if (!path.Add(this)) throw new InvalidOperationException($"Cyclic steering configuration: {name}.");
            try { return Build(path); }
            finally { path.Remove(this); }
        }

        protected abstract Func<IRandomSource, ISteeringBehaviour> Build(HashSet<SteeringAsset> path);
    }
}
