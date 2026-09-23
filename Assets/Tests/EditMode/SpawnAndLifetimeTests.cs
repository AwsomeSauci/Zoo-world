using System;
using NUnit.Framework;
using ZooWorld.Application.Animals;
using ZooWorld.Application.Spawning;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Tests.EditMode
{
    public sealed class SpawnAndLifetimeTests
    {
        [Test]
        public void SpawnsOneAnimalPerChosenInterval()
        {
            var spawner = new FakeSpawner();
            var schedule = new SpawnSchedule(spawner, new SpawnTable(new SpawnEntry("frog", 1)), new ConstantRandom(0.5f), 1, 2);
            schedule.Tick(1.49f);
            Assert.That(spawner.Count, Is.Zero);
            schedule.Tick(0.02f);
            Assert.That(spawner.Count, Is.EqualTo(1));
            schedule.Tick(0.5f);
            Assert.That(spawner.Count, Is.EqualTo(1));
        }

        [Test]
        public void OccupiedSpawnSpaceRetriesWithoutBurstingOrChangingSpecies()
        {
            var spawner = new FakeSpawner { CanSpawn = false };
            var schedule = new SpawnSchedule(spawner, new SpawnTable(new SpawnEntry("frog", 1)), new ConstantRandom(0), 1, 2);
            schedule.Tick(100);
            Assert.That(spawner.Count, Is.Zero);
            spawner.CanSpawn = true;
            schedule.Tick(0.11f);
            Assert.That(spawner.Count, Is.EqualTo(1));
            schedule.Tick(0.5f);
            Assert.That(spawner.Count, Is.EqualTo(1));
        }

        [Test]
        public void NewModuleCanBeAddedWithoutChangingAnimalFactory()
        {
            var module = new RecordingFactory();
            var factory = new AnimalFactory(new AnimalCatalog(new[] { new AnimalDefinition("custom", module) }), new FakePool());
            using (var first = factory.Create("custom", PlanarVector.Zero))
            using (var second = factory.Create("custom", PlanarVector.Zero))
            {
                first.Tick(0.02f);
                Assert.That(module.Created, Is.EqualTo(2));
                Assert.That(first.Id, Is.Not.EqualTo(second.Id));
                Assert.That(module.First.Ticks, Is.EqualTo(1));
                Assert.That(module.Second.Ticks, Is.Zero);
            }
            Assert.That(module.First.Disposed, Is.True);
            Assert.That(module.Second.Disposed, Is.True);
        }

        [Test]
        public void FailedModuleCreationReleasesEarlierModulesAndBody()
        {
            var module = new RecordingFactory();
            var pool = new FakePool();
            var factory = new AnimalFactory(new AnimalCatalog(new[] { new AnimalDefinition("broken", module, new ThrowingFactory()) }), pool);
            Assert.Throws<AggregateException>(() => factory.Create("broken", PlanarVector.Zero));
            Assert.That(module.First.Disposed, Is.True);
            Assert.That(pool.Last.Disposed, Is.True);
        }

        [Test]
        public void SpeciesIdentifiersMustBeUnique()
        {
            Assert.Throws<ArgumentException>(() => new AnimalCatalog(new[] { new AnimalDefinition("frog"), new AnimalDefinition("frog") }));
        }

        [Test]
        public void ModuleDeathStopsTheCurrentTickAndDefersResourceRelease()
        {
            using (var population = new AnimalPopulation())
            {
                var deaths = new AnimalDeathService(population);
                var id = new AnimalId(1);
                var body = new ContactTests.FakeBody();
                var later = new RecordingModule();
                var animal = new AnimalInstance(id, body, new IAnimalModule[]
                {
                    new ActionModule(() => deaths.TryKill(id)), later
                });
                population.Add(animal);
                animal.Tick(0.02f);
                Assert.That(animal.IsAlive, Is.False);
                Assert.That(body.Active, Is.False);
                Assert.That(later.Ticks, Is.Zero);
                Assert.That(later.Disposed, Is.False);
                deaths.Tick(0.02f);
                Assert.That(later.Disposed, Is.True);
                Assert.That(body.Disposed, Is.True);
                Assert.That(population.Count, Is.Zero);
            }
        }

        [Test]
        public void DisposalReleasesModulesAndBodyEvenIfDeactivationFails()
        {
            var body = new FailingBody();
            var module = new RecordingModule();
            var animal = new AnimalInstance(new AnimalId(1), body, new IAnimalModule[] { module });
            Assert.Throws<AggregateException>(() => animal.Dispose());
            Assert.That(module.Disposed, Is.True);
            Assert.That(body.Disposed, Is.True);
            Assert.DoesNotThrow(() => animal.Dispose());
        }

        [Test]
        public void ClosedPopulationRejectsNewAnimals()
        {
            var population = new AnimalPopulation();
            population.Dispose();
            using (var animal = new AnimalInstance(new AnimalId(1), new ContactTests.FakeBody(), Array.Empty<IAnimalModule>()))
                Assert.Throws<ObjectDisposedException>(() => population.Add(animal));
        }

        private sealed class ActionModule : IAnimalTickModule
        {
            private readonly Action action;
            public ActionModule(Action action) => this.action = action;
            public void Tick(float deltaTime) => action();
            public void Dispose() { }
        }

        private sealed class FailingBody : IAnimalBody
        {
            public bool Disposed;
            public PlanarVector Position => PlanarVector.Zero;
            public void ApplyMotion(in MotionIntent intent, float deltaTime) { }
            public void Activate() { }
            public void Deactivate() => throw new InvalidOperationException("Expected deactivation failure.");
            public void Dispose() => Disposed = true;
        }

        private sealed class ConstantRandom : IRandomSource
        {
            private readonly float value;
            public ConstantRandom(float value) => this.value = value;
            public float NextUnit() => value;
        }

        private sealed class FakeSpawner : IAnimalSpawner
        {
            public int Count;
            public bool CanSpawn = true;
            public bool TrySpawn(string speciesId) { if (CanSpawn) Count++; return CanSpawn; }
        }

        private sealed class FakePool : IAnimalBodyPool
        {
            public ContactTests.FakeBody Last;
            public IAnimalBody Rent(string speciesId, AnimalId id, PlanarVector position) => Last = new ContactTests.FakeBody();
        }

        private sealed class RecordingFactory : IAnimalModuleFactory
        {
            public int Created;
            public RecordingModule First;
            public RecordingModule Second;
            public IAnimalModule Create(AnimalId id, IAnimalBody body)
            {
                Created++;
                var module = new RecordingModule();
                if (First == null) First = module; else Second = module;
                return module;
            }
        }

        private sealed class RecordingModule : IAnimalTickModule
        {
            public int Ticks;
            public bool Disposed;
            public void Tick(float deltaTime) => Ticks++;
            public void Dispose() => Disposed = true;
        }

        private sealed class ThrowingFactory : IAnimalModuleFactory
        {
            public IAnimalModule Create(AnimalId id, IAnimalBody body) => throw new InvalidOperationException("Expected creation failure.");
        }
    }
}
