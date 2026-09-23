using System;

namespace ZooWorld.Domain.Animals
{
    public readonly struct AnimalId : IEquatable<AnimalId>, IComparable<AnimalId>
    {
        public long Value { get; }

        public AnimalId(long value)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            Value = value;
        }

        public bool Equals(AnimalId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is AnimalId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public int CompareTo(AnimalId other) => Value.CompareTo(other.Value);
        public override string ToString() => Value.ToString();
        public static bool operator ==(AnimalId a, AnimalId b) => a.Equals(b);
        public static bool operator !=(AnimalId a, AnimalId b) => !a.Equals(b);
    }
}
