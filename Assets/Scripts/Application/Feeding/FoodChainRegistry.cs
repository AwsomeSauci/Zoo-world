using System;
using System.Collections.Generic;
using ZooWorld.Application.Animals;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Feeding;

namespace ZooWorld.Application.Feeding
{
    public sealed class FoodChainRegistry
    {
        private readonly Dictionary<AnimalId, FoodRole> roles = new Dictionary<AnimalId, FoodRole>();
        private readonly object sync = new object();

        public bool TryGet(AnimalId id, out FoodRole role)
        {
            lock (sync) return roles.TryGetValue(id, out role);
        }

        public void Register(AnimalId id, FoodRole role)
        {
            lock (sync) roles.Add(id, role);
        }

        public void Unregister(AnimalId id)
        {
            lock (sync) roles.Remove(id);
        }
    }
}
