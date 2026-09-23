using System;
using UnityEngine;
using Zenject;
using ZooWorld.Application.Animals;
using ZooWorld.Application.Feeding;
using ZooWorld.Composition.Animals;
using ZooWorld.Domain.Feeding;

namespace ZooWorld.Composition.Feeding
{
    [CreateAssetMenu(menuName = "Zoo World/Modules/Feeding")]
    public sealed class FeedingModuleAsset : AnimalModuleAsset
    {
        [SerializeField] private FoodRole role;
        public override Type ExclusiveCapability => typeof(IFeedingModule);

        public override IAnimalModuleFactory CreateFactory(DiContainer container)
        {
            if (!Enum.IsDefined(typeof(FoodRole), role)) throw new InvalidOperationException($"'{name}' has an invalid food role.");
            return container.Instantiate<FeedingModuleFactory>(new object[] { role });
        }
    }
}
