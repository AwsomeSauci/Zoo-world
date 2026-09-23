using System;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Common;

namespace ZooWorld.Application.Movement
{
    public sealed class SeededRandom : IRandomSource
    {
        private readonly Random random;
        public SeededRandom(int seed) => random = new Random(seed);
        public float NextUnit() => (float)(random.NextDouble() * 0.99999994);
    }
}
