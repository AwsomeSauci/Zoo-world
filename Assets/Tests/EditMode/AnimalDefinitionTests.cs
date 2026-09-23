using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Zenject;
using ZooWorld.Application.Animals;
using ZooWorld.Application.Feeding;
using ZooWorld.Application.Movement;
using ZooWorld.Composition.Animals;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Feeding;
using ZooWorld.Domain.Movement;
using Object = UnityEngine.Object;

namespace ZooWorld.Tests.EditMode
{
    public sealed class AnimalDefinitionTests
    {
        private AnimalDefinitionAsset animal;

        [SetUp]
        public void SetUp() => animal = Object.Instantiate(
            AssetDatabase.LoadAssetAtPath<AnimalDefinitionAsset>("Assets/Configurations/Animals/Frog.asset"));

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(animal);

        [TestCase("Movement/Jumping Movement")]
        [TestCase("Feeding/Prey")]
        public void AnimalMustHaveBothMovementAndFoodRole(string module)
        {
            SetModules(module);
            Assert.Throws<InvalidOperationException>(() => animal.Compile(new DiContainer()));
        }

        [Test]
        public void TwoModulesCannotOwnTheSameCapability()
        {
            SetModules("Movement/Jumping Movement", "Movement/Linear Movement", "Feeding/Prey");
            Assert.Throws<InvalidOperationException>(() => animal.Compile(new DiContainer()));
        }

        [Test]
        public void NewJumpingPredatorUsesExistingModulesWithIndependentState()
        {
            SetModules("Movement/Jumping Movement", "Feeding/Predator");
            var serialized = new SerializedObject(animal);
            serialized.FindProperty("speciesId").stringValue = "jumping-predator";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var container = new DiContainer();
            var roles = new FoodChainRegistry();
            container.Bind<FoodChainRegistry>().FromInstance(roles);
            container.Bind<IWorldBoundsProvider>().FromInstance(new BoundsProvider());
            container.Bind<IAnimalRandomFactory>().FromInstance(new AnimalRandomFactory(42));
            var definition = animal.Compile(container);
            var factory = new AnimalFactory(new AnimalCatalog(new[] { definition }), new BodyPool());
            AnimalId firstId;
            using (var first = factory.Create("jumping-predator", PlanarVector.Zero))
            using (var second = factory.Create("jumping-predator", PlanarVector.Zero))
            {
                firstId = first.Id;
                Assert.That(roles.TryGet(first.Id, out var role), Is.True);
                Assert.That(role, Is.EqualTo(FoodRole.Predator));
                first.Tick(0.5f);
                first.Tick(0.02f);
                second.Tick(0.02f);
                Assert.That(((Body)first.Body).Motion.Velocity.Length, Is.Zero);
                Assert.That(((Body)second.Body).Motion.Velocity.Length, Is.GreaterThan(0));
            }
            Assert.That(roles.TryGet(firstId, out _), Is.False);
        }

        private void SetModules(params string[] paths)
        {
            var serialized = new SerializedObject(animal);
            var modules = serialized.FindProperty("modules");
            modules.arraySize = paths.Length;
            for (var i = 0; i < paths.Length; i++)
                modules.GetArrayElementAtIndex(i).objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<AnimalModuleAsset>($"Assets/Configurations/{paths[i]}.asset");
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private sealed class BoundsProvider : IWorldBoundsProvider
        {
            public WorldBounds Bounds => new WorldBounds(new PlanarVector(-10, -10), new PlanarVector(10, 10));
        }

        private sealed class BodyPool : IAnimalBodyPool
        {
            public IAnimalBody Rent(string speciesId, AnimalId id, PlanarVector position) => new Body();
        }

        private sealed class Body : IAnimalBody
        {
            public PlanarVector Position => PlanarVector.Zero;
            public MotionIntent Motion { get; private set; }
            public void ApplyMotion(in MotionIntent intent, float deltaTime) => Motion = intent;
            public void Activate() { }
            public void Deactivate() { }
            public void Dispose() { }
        }
    }
}
