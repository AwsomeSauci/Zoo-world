using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using ZooWorld.Application.Contacts;
using ZooWorld.Application.Feeding;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Feeding;

namespace ZooWorld.Tests.EditMode
{
    public sealed class ContactResponsePolicyTests
    {
        [TestCase(FoodRole.Prey, FoodRole.Prey, false)]
        [TestCase(FoodRole.Prey, FoodRole.Predator, true)]
        [TestCase(FoodRole.Predator, FoodRole.Prey, true)]
        [TestCase(FoodRole.Predator, FoodRole.Predator, true)]
        public void OnlyConsumptionContactsSuppressThePhysicsImpulse(FoodRole first, FoodRole second, bool suppressed)
        {
            var registry = new FoodChainRegistry();
            var a = new AnimalId(1);
            var b = new AnimalId(2);
            registry.Register(a, first);
            registry.Register(b, second);
            var policy = new FoodChainContactPolicy(registry, new FoodChainRules());
            Assert.That(policy.ShouldSuppressImpulse(a, b), Is.EqualTo(suppressed));
            Assert.That(policy.ShouldSuppressImpulse(b, a), Is.EqualTo(suppressed));
        }

        [Test]
        public void ContactPolicyUsesTheConfiguredRuleInsteadOfAssumingPredatorsAlwaysEat()
        {
            var registry = new FoodChainRegistry();
            registry.Register(new AnimalId(1), FoodRole.Predator);
            registry.Register(new AnimalId(2), FoodRole.Prey);
            var policy = new FoodChainContactPolicy(registry, new NeverEat());
            Assert.That(policy.ShouldSuppressImpulse(new AnimalId(1), new AnimalId(2)), Is.False);
        }

        [Test]
        public void ReleasedOrUnknownAnimalsDoNotSuppressPhysicalContacts()
        {
            var registry = new FoodChainRegistry();
            var a = new AnimalId(1);
            var b = new AnimalId(2);
            registry.Register(a, FoodRole.Predator);
            registry.Register(b, FoodRole.Prey);
            var policy = new FoodChainContactPolicy(registry, new FoodChainRules());
            registry.Unregister(b);
            Assert.That(policy.ShouldSuppressImpulse(a, b), Is.False);
            Assert.That(policy.ShouldSuppressImpulse(a, new AnimalId(100)), Is.False);
        }

        [Test]
        public void SolverThreadsCanReportDuplicateContactsWithoutLosingOtherPairs()
        {
            var buffer = new ContactBuffer();
            Parallel.For(0, 4000, i =>
            {
                var first = new AnimalId(1);
                var second = new AnimalId(2 + i % 32);
                buffer.Report(first, second);
                buffer.Report(second, first);
            });
            var pairs = new List<ContactPair>();
            buffer.DrainTo(pairs);
            Assert.That(pairs.Count, Is.EqualTo(32));
            buffer.DrainTo(pairs);
            Assert.That(pairs, Is.Empty);
        }

        private sealed class NeverEat : IContactRule
        {
            public bool TryResolve(AnimalId a, FoodRole roleA, AnimalId b, FoodRole roleB, out Consumption consumption)
            {
                consumption = default;
                return false;
            }
        }
    }
}
