using System;
using ZooWorld.Domain.Feeding;

namespace ZooWorld.Application.Statistics
{
    public readonly struct DeathCounts
    {
        public long Prey { get; }
        public long Predators { get; }
        public DeathCounts(long prey, long predators) { Prey = prey; Predators = predators; }
    }
}
