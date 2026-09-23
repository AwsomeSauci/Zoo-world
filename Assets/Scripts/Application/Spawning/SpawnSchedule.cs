using System;
using ZooWorld.Application.Simulation;
using ZooWorld.Domain.Common;

namespace ZooWorld.Application.Spawning
{
    public sealed class SpawnSchedule : ISimulationStep
    {
        private readonly IAnimalSpawner spawner;
        private readonly SpawnTable table;
        private readonly IRandomSource random;
        private readonly float minimum;
        private readonly float maximum;
        private float remaining;
        private string pendingSpecies;

        public SpawnSchedule(IAnimalSpawner spawner, SpawnTable table, IRandomSource random, float minimum, float maximum)
        {
            this.spawner = spawner;
            this.table = table;
            this.random = random;
            this.minimum = Guard.Positive(minimum, nameof(minimum));
            this.maximum = Guard.Positive(maximum, nameof(maximum));
            if (maximum < minimum) throw new ArgumentException("Spawn interval range is inverted.");
            remaining = NextInterval();
        }

        public void Tick(float deltaTime)
        {
            remaining -= deltaTime;
            if (remaining > 0) return;
            pendingSpecies ??= table.Choose(random);
            if (spawner.TrySpawn(pendingSpecies))
            {
                pendingSpecies = null;
                remaining = NextInterval();
            }
            else remaining = 0.1f; // Retry occupied spawn space without an unbounded search or a burst of overdue spawns.
        }

        private float NextInterval() => minimum + random.NextUnit() * (maximum - minimum);
    }
}
