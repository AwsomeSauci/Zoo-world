using System;
using UnityEngine;
using ZooWorld.Composition.Animals;

namespace ZooWorld.Composition.Spawning
{
    [Serializable]
    public sealed class AnimalSpawnEntry
    {
        [SerializeField] private AnimalDefinitionAsset animal;
        [SerializeField, Min(0.01f)] private float weight = 1;
        public AnimalDefinitionAsset Animal => animal;
        public float Weight => weight;
    }
}
