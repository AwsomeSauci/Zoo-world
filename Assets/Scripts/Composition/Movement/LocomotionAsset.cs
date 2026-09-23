using System;
using UnityEngine;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Composition.Movement
{
    public abstract class LocomotionAsset : ScriptableObject
    {
        public Func<ILocomotionBehaviour> CreateFactory() => Build();

        protected abstract Func<ILocomotionBehaviour> Build();
    }
}
