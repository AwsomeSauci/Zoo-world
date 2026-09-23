using UnityEngine;
using Zenject;
using ZooWorld.Application.Animals;
using ZooWorld.Application.Contacts;
using ZooWorld.Application.Feeding;
using ZooWorld.Application.Movement;
using ZooWorld.Application.Simulation;
using ZooWorld.Application.Spawning;
using ZooWorld.Application.Statistics;
using ZooWorld.Composition.Animals;
using ZooWorld.Composition.Spawning;
using ZooWorld.Domain.Feeding;
using ZooWorld.Unity.Physics;
using ZooWorld.Unity.World;

namespace ZooWorld.Composition
{
    public sealed class ZooInstaller : MonoInstaller
    {
        [SerializeField] private ZooSettings settings;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Transform animalsRoot;
        [SerializeField] private Transform poolRoot;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<AnimalPopulation>().AsSingle();
            Container.BindInterfacesAndSelfTo<ContactBuffer>().AsSingle();
            Container.Bind<FoodChainRegistry>().AsSingle();
            Container.BindInterfacesAndSelfTo<AnimalDeathService>().AsSingle();
            Container.BindInterfacesAndSelfTo<DeathStatistics>().AsSingle();
            Container.BindInterfacesAndSelfTo<ConsumptionEvents>().AsSingle();
            Container.Bind<IContactRule>().To<FoodChainRules>().AsSingle();
            Container.Bind<IContactResponsePolicy>().To<FoodChainContactPolicy>().AsSingle();
            Container.BindInterfacesAndSelfTo<AnimalContactFilter>().AsSingle();
            Container.BindInterfacesAndSelfTo<CameraWorldBounds>().FromInstance(new CameraWorldBounds(worldCamera));
            Container.Bind<IAnimalRandomFactory>().FromInstance(new AnimalRandomFactory(settings.RandomSeed));
            Container.Bind<IAnimalPrefabCatalog>().FromInstance(new AnimalPrefabCatalog(settings.Animals));
            Container.BindInterfacesAndSelfTo<AnimalBodyPool>().AsSingle().WithArguments(animalsRoot, poolRoot);
            Container.Bind<ISpawnPointSource>().To<PhysicsSpawnPointSource>().AsSingle()
                .WithArguments(new SeededRandom(settings.RandomSeed ^ 7919), LayerMask.GetMask("Animal", "Obstacle"));

            Container.Bind<AnimalCatalog>().FromMethod(context =>
            {
                var definitions = new AnimalDefinition[settings.Animals.Count];
                for (var i = 0; i < definitions.Length; i++) definitions[i] = settings.Animals[i].Animal.Compile(context.Container);
                return new AnimalCatalog(definitions);
            }).AsSingle();
            Container.Bind<AnimalFactory>().AsSingle();
            Container.Bind<IAnimalSpawner>().To<SpawnService>().AsSingle();
            Container.Bind<ContactResolutionService>().AsSingle();
            Container.Bind<AnimalTickService>().AsSingle();
            Container.Bind<SpawnSchedule>().AsSingle().WithArguments(settings.CreateSpawnTable(),
                new SeededRandom(settings.RandomSeed), settings.MinimumSpawnInterval, settings.MaximumSpawnInterval);
            Container.Bind<SimulationPipeline>().FromMethod(context => new SimulationPipeline(
                context.Container.Resolve<CameraWorldBounds>(),
                context.Container.Resolve<ContactResolutionService>(), context.Container.Resolve<AnimalDeathService>(),
                context.Container.Resolve<SpawnSchedule>(), context.Container.Resolve<AnimalTickService>())).AsSingle();

            // Zenject disposes in reverse priority: animal modules release before their view pools.
            Container.BindExecutionOrder<AnimalBodyPool>(-20);
            Container.BindExecutionOrder<AnimalPopulation>(-10);
            Container.BindExecutionOrder<AnimalContactFilter>(-30);
        }
    }
}
