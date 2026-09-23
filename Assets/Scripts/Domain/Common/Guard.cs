using System;

namespace ZooWorld.Domain.Common
{
    public static class Guard
    {
        public static float Positive(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0)
                throw new ArgumentOutOfRangeException(name, "Expected a finite positive number.");
            return value;
        }

        public static float NonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0)
                throw new ArgumentOutOfRangeException(name, "Expected a finite non-negative number.");
            return value;
        }
    }
}
