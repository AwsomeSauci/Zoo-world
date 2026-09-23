using System;
using R3;
using ZooWorld.Domain.Feeding;

namespace ZooWorld.Application.Statistics
{
    public sealed class DeathStatistics : IDeathStatistics, IDisposable
    {
        private readonly ReactiveProperty<DeathCounts> counts = new ReactiveProperty<DeathCounts>(default);
        public ReadOnlyReactiveProperty<DeathCounts> Counts => counts;

        public void Record(FoodRole role)
        {
            var current = counts.Value;
            counts.Value = role == FoodRole.Prey
                ? new DeathCounts(current.Prey + 1, current.Predators)
                : new DeathCounts(current.Prey, current.Predators + 1);
        }

        public void Dispose() => counts.Dispose();
    }
}
