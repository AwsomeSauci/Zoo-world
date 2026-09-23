using NUnit.Framework;
using R3;
using ZooWorld.Application.Animals;
using ZooWorld.Application.Contacts;
using ZooWorld.Application.Feeding;
using ZooWorld.Application.Statistics;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Feeding;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Tests.EditMode
{
    public sealed class ContactTests
    {
        private AnimalPopulation population;
        private FoodChainRegistry roles;
        private AnimalDeathService deaths;
        private ContactBuffer buffer;
        private DeathStatistics statistics;
        private ContactResolutionService service;
        private ConsumptionEvents events;
        private int consumptionCount;

        [SetUp]
        public void SetUp()
        {
            population = new AnimalPopulation();
            roles = new FoodChainRegistry();
            deaths = new AnimalDeathService(population);
            buffer = new ContactBuffer();
            statistics = new DeathStatistics();
            events = new ConsumptionEvents();
            consumptionCount = 0;
            events.Consumed.Subscribe(_ => consumptionCount++);
            service = new ContactResolutionService(buffer, population, roles, new FoodChainRules(), deaths, statistics, events);
        }

        [TearDown]
        public void TearDown()
        {
            population.Dispose();
            events.Dispose();
            statistics.Dispose();
        }

        [TestCase(FoodRole.Prey, FoodRole.Prey, 0, 0)]
        [TestCase(FoodRole.Predator, FoodRole.Prey, 1, 0)]
        [TestCase(FoodRole.Prey, FoodRole.Predator, 1, 0)]
        [TestCase(FoodRole.Predator, FoodRole.Predator, 0, 1)]
        public void ContactAppliesFoodChainExactlyOnce(FoodRole first, FoodRole second, int deadPrey, int deadPredators)
        {
            var a = Add(1, first);
            var b = Add(2, second);
            buffer.Report(a.Id, b.Id);
            buffer.Report(b.Id, a.Id);
            service.Tick(0.02f);
            buffer.Report(a.Id, b.Id);
            service.Tick(0.02f);
            Assert.That(statistics.Counts.CurrentValue.Prey, Is.EqualTo(deadPrey));
            Assert.That(statistics.Counts.CurrentValue.Predators, Is.EqualTo(deadPredators));
            Assert.That(consumptionCount, Is.EqualTo(deadPrey + deadPredators));
        }

        [Test]
        public void KilledPredatorCannotEatInALaterContact()
        {
            var winner = Add(1, FoodRole.Predator);
            var loser = Add(2, FoodRole.Predator);
            var prey = Add(3, FoodRole.Prey);
            buffer.Report(loser.Id, prey.Id);
            buffer.Report(winner.Id, loser.Id);
            service.Tick(0.02f);
            Assert.That(prey.IsAlive, Is.True);
            Assert.That(loser.IsAlive, Is.False);
            Assert.That(consumptionCount, Is.EqualTo(1));
        }

        [Test]
        public void MultiplePredatorsCannotConsumeSamePrey()
        {
            var a = Add(1, FoodRole.Predator);
            var b = Add(2, FoodRole.Predator);
            var prey = Add(3, FoodRole.Prey);
            buffer.Report(b.Id, prey.Id);
            buffer.Report(a.Id, prey.Id);
            service.Tick(0.02f);
            Assert.That(statistics.Counts.CurrentValue.Prey, Is.EqualTo(1));
            Assert.That(consumptionCount, Is.EqualTo(1));
        }

        [Test]
        public void DeadBodyIsDisabledBeforeModulesAreReleased()
        {
            var a = Add(1, FoodRole.Predator);
            var b = Add(2, FoodRole.Prey);
            var body = (FakeBody)b.Body;
            buffer.Report(a.Id, b.Id);
            service.Tick(0.02f);
            Assert.That(body.Active, Is.False);
            Assert.That(body.Disposed, Is.False);
            deaths.Tick(0.02f);
            Assert.That(body.Disposed, Is.True);
            Assert.That(roles.TryGet(b.Id, out _), Is.False);
            Assert.That(population.Count, Is.EqualTo(1));
        }

        [Test]
        public void PredatorRuleIsIndependentOfCallbackOrder()
        {
            var rules = new FoodChainRules();
            rules.TryResolve(new AnimalId(8), FoodRole.Predator, new AnimalId(3), FoodRole.Predator, out var first);
            rules.TryResolve(new AnimalId(3), FoodRole.Predator, new AnimalId(8), FoodRole.Predator, out var second);
            Assert.That(first.Eater, Is.EqualTo(second.Eater));
            Assert.That(first.Eater.Value, Is.EqualTo(3));
        }

        private AnimalInstance Add(long value, FoodRole role)
        {
            var id = new AnimalId(value);
            var body = new FakeBody();
            var animal = new AnimalInstance(id, body, new IAnimalModule[] { new FeedingModule(id, roles, role) });
            population.Add(animal);
            return animal;
        }

        internal sealed class FakeBody : IAnimalBody
        {
            public bool Active = true;
            public bool Disposed;
            public PlanarVector Position => PlanarVector.Zero;
            public void ApplyMotion(in MotionIntent intent, float deltaTime) { }
            public void Activate() => Active = true;
            public void Deactivate() => Active = false;
            public void Dispose() => Disposed = true;
        }
    }
}
