using System;
using ZooWorld.Application.Simulation;
using ZooWorld.Domain.Common;

namespace ZooWorld.Application.Spawning
{
    public sealed class SpawnTable
    {
        private readonly SpawnEntry[] entries;
        private readonly float totalWeight;

        public SpawnTable(params SpawnEntry[] entries)
        {
            if (entries == null || entries.Length == 0) throw new ArgumentException("At least one spawn entry is required.");
            this.entries = (SpawnEntry[])entries.Clone();
            foreach (var entry in entries) totalWeight += entry.Weight;
            Guard.Positive(totalWeight, nameof(totalWeight));
        }

        public string Choose(IRandomSource random)
        {
            var value = random.NextUnit() * totalWeight;
            foreach (var entry in entries)
            {
                value -= entry.Weight;
                if (value < 0) return entry.SpeciesId;
            }
            return entries[entries.Length - 1].SpeciesId;
        }
    }
}
