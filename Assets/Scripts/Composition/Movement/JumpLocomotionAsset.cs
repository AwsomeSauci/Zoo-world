using System;
using UnityEngine;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Composition.Movement
{
    [CreateAssetMenu(menuName = "Zoo World/Locomotion/Jump")]
    public sealed class JumpLocomotionAsset : LocomotionAsset
    {
        [SerializeField, Min(0.01f)] private float distance = 2.2f;
        [SerializeField, Min(0.01f)] private float duration = 0.35f;
        [SerializeField, Min(0.01f)] private float interval = 1.4f;
        [SerializeField, Min(0)] private float height = 0.7f;

        protected override Func<ILocomotionBehaviour> Build()
        {
            var configuredDistance = distance;
            var configuredDuration = duration;
            var configuredInterval = interval;
            var configuredHeight = height;
            return () => new JumpLocomotion(configuredDistance, configuredDuration, configuredInterval, configuredHeight);
        }
    }
}
