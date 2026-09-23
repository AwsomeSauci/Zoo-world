using System;
using ZooWorld.Application.Simulation;
using ZooWorld.Domain.Common;

namespace ZooWorld.Application.Spawning
{
    public readonly struct SpawnEntry
    {
        public string SpeciesId { get; }
        public float Weight { get; }
        public SpawnEntry(string speciesId, float weight)
        {
            if (string.IsNullOrWhiteSpace(speciesId)) throw new ArgumentException("Species ID is required.");
            SpeciesId = speciesId;
            Weight = Guard.Positive(weight, nameof(weight));
        }
    }
}
