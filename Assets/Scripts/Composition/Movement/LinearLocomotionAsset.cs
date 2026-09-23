using System;
using UnityEngine;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Composition.Movement
{
    [CreateAssetMenu(menuName = "Zoo World/Locomotion/Linear")]
    public sealed class LinearLocomotionAsset : LocomotionAsset
    {
        [SerializeField, Min(0.01f)] private float speed = 2.2f;

        protected override Func<ILocomotionBehaviour> Build()
        {
            var configuredSpeed = speed;
            return () => new LinearLocomotion(configuredSpeed);
        }
    }
}
