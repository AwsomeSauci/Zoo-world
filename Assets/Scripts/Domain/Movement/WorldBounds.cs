using System;
using ZooWorld.Domain.Common;

namespace ZooWorld.Domain.Movement
{
    public readonly struct WorldBounds
    {
        public PlanarVector Min { get; }
        public PlanarVector Max { get; }
        public PlanarVector Center => (Min + Max) * 0.5f;

        public WorldBounds(PlanarVector min, PlanarVector max)
        {
            if (!(max.X > min.X) || !(max.Y > min.Y)) throw new ArgumentException("Bounds must have a positive area.");
            Min = min;
            Max = max;
        }

        public bool Contains(PlanarVector position, float inset = 0) =>
            position.X >= Min.X + inset && position.X <= Max.X - inset &&
            position.Y >= Min.Y + inset && position.Y <= Max.Y - inset;
    }
}
