using System;
using System.Collections.Generic;
using ZooWorld.Domain.Animals;

namespace ZooWorld.Application.Contacts
{
    public readonly struct ContactPair : IEquatable<ContactPair>, IComparable<ContactPair>
    {
        public AnimalId A { get; }
        public AnimalId B { get; }

        public ContactPair(AnimalId a, AnimalId b)
        {
            A = a.CompareTo(b) < 0 ? a : b;
            B = a.CompareTo(b) < 0 ? b : a;
        }

        public bool Equals(ContactPair other) => A == other.A && B == other.B;
        public override bool Equals(object obj) => obj is ContactPair other && Equals(other);
        public override int GetHashCode() => unchecked(A.GetHashCode() * 397 ^ B.GetHashCode());
        public int CompareTo(ContactPair other) => A == other.A ? B.CompareTo(other.B) : A.CompareTo(other.A);
    }
}
