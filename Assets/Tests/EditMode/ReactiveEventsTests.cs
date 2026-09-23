using System;
using System.Collections.Generic;
using NUnit.Framework;
using R3;
using ZooWorld.Application.Feeding;
using ZooWorld.Application.Statistics;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Feeding;

namespace ZooWorld.Tests.EditMode
{
    public sealed class ReactiveEventsTests
    {
        [Test]
        public void StatisticsReplayLatestCountsAndReleaseSubscriptions()
        {
            using var statistics = new DeathStatistics();
            var received = new List<DeathCounts>();
            var subscription = statistics.Counts.Subscribe(received.Add);
            statistics.Record(FoodRole.Prey);
            Assert.That(received.Count, Is.EqualTo(2));
            Assert.That(received[0].Prey, Is.Zero);
            Assert.That(received[1].Prey, Is.EqualTo(1));
            subscription.Dispose();
            statistics.Record(FoodRole.Predator);
            Assert.That(received.Count, Is.EqualTo(2));
            using var resumed = statistics.Counts.Subscribe(received.Add);
            Assert.That(received.Count, Is.EqualTo(3));
            Assert.That(received[2].Prey, Is.EqualTo(1));
            Assert.That(received[2].Predators, Is.EqualTo(1));
        }

        [Test]
        public void FailingSubscriberDoesNotBlockOtherConsumersOrFutureMeals()
        {
            using var events = new ConsumptionEvents();
            var errors = 0;
            var meals = 0;
            using var broken = events.Consumed.Subscribe(
                _ => throw new InvalidOperationException("Broken view"), _ => errors++, _ => { });
            using var healthy = events.Consumed.Subscribe(_ => meals++);
            var meal = new Consumption(new AnimalId(1), new AnimalId(2), FoodRole.Prey);
            Assert.DoesNotThrow(() => events.Publish(meal));
            Assert.DoesNotThrow(() => events.Publish(meal));
            Assert.That(errors, Is.EqualTo(2));
            Assert.That(meals, Is.EqualTo(2));
        }

        [Test]
        public void DisposingServicesCompletesTheirStreams()
        {
            using var statistics = new DeathStatistics();
            using var events = new ConsumptionEvents();
            var countsCompleted = false;
            var mealsCompleted = false;
            using var counts = statistics.Counts.Subscribe(_ => { }, _ => { },
                result => countsCompleted = result.IsSuccess);
            using var meals = events.Consumed.Subscribe(_ => { }, _ => { },
                result => mealsCompleted = result.IsSuccess);
            statistics.Dispose();
            events.Dispose();
            Assert.That(countsCompleted && mealsCompleted, Is.True);
        }
    }
}
