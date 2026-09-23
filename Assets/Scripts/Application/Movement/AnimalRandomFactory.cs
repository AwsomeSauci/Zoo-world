using System;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Common;

namespace ZooWorld.Application.Movement
{
    public sealed class AnimalRandomFactory : IAnimalRandomFactory
    {
        private readonly int seed;
        public AnimalRandomFactory(int seed) => this.seed = seed;
        public IRandomSource Create(AnimalId id) => new SeededRandom(unchecked(seed * 397 ^ id.Value.GetHashCode()));
    }
}
