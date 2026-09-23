using System;

namespace ZooWorld.Domain.Common
{
    public readonly struct PlanarVector
    {
        public static readonly PlanarVector Zero = new PlanarVector(0, 0);
        public float X { get; }
        public float Y { get; }
        public float LengthSquared => X * X + Y * Y;
        public float Length => (float)Math.Sqrt(LengthSquared);
        public PlanarVector Normalized => Length > 0.00001f ? this / Length : Zero;

        public PlanarVector(float x, float y) { X = x; Y = y; }
        public static PlanarVector operator +(PlanarVector a, PlanarVector b) => new PlanarVector(a.X + b.X, a.Y + b.Y);
        public static PlanarVector operator -(PlanarVector a, PlanarVector b) => new PlanarVector(a.X - b.X, a.Y - b.Y);
        public static PlanarVector operator *(PlanarVector a, float b) => new PlanarVector(a.X * b, a.Y * b);
        public static PlanarVector operator /(PlanarVector a, float b) => new PlanarVector(a.X / b, a.Y / b);
    }
}
