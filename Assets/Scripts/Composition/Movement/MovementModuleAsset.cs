using System;
using UnityEngine;
using Zenject;
using ZooWorld.Application.Animals;
using ZooWorld.Application.Movement;
using ZooWorld.Composition.Animals;

namespace ZooWorld.Composition.Movement
{
    [CreateAssetMenu(menuName = "Zoo World/Modules/Movement")]
    public sealed class MovementModuleAsset : AnimalModuleAsset
    {
        [SerializeField] private SteeringAsset steering;
        [SerializeField] private LocomotionAsset locomotion;
        public override Type ExclusiveCapability => typeof(IMovementModule);

        public override IAnimalModuleFactory CreateFactory(DiContainer container)
        {
            if (!steering || !locomotion) throw new InvalidOperationException($"'{name}' needs steering and locomotion.");
            return container.Instantiate<MovementModuleFactory>(new object[] { steering.CreateFactory(), locomotion.CreateFactory() });
        }
    }
}
