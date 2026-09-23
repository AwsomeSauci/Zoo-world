using System;
using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Application.Spawning;

namespace ZooWorld.Composition.Spawning
{
    [CreateAssetMenu(menuName = "Zoo World/Zoo Settings")]
    public sealed class ZooSettings : ScriptableObject
    {
        [SerializeField] private AnimalSpawnEntry[] animals;
        [SerializeField, Min(0.01f)] private float minimumSpawnInterval = 1;
        [SerializeField, Min(0.01f)] private float maximumSpawnInterval = 2;
        [SerializeField] private int randomSeed = 1729;
        public IReadOnlyList<AnimalSpawnEntry> Animals => animals;
        public float MinimumSpawnInterval => minimumSpawnInterval;
        public float MaximumSpawnInterval => maximumSpawnInterval;
        public int RandomSeed => randomSeed;

        public SpawnTable CreateSpawnTable()
        {
            if (animals == null || animals.Length == 0) throw new InvalidOperationException("The zoo needs at least one animal.");
            var entries = new SpawnEntry[animals.Length];
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = animals[i];
                if (entry == null || !entry.Animal) throw new InvalidOperationException("The spawn catalog has a missing animal.");
                entries[i] = new SpawnEntry(entry.Animal.SpeciesId, entry.Weight);
            }
            return new SpawnTable(entries);
        }
    }
}
