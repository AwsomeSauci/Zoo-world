using System;
using System.Collections.Generic;
using ZooWorld.Domain.Animals;

namespace ZooWorld.Application.Animals
{
    public sealed class AnimalPopulation : IAnimalLookup, IDisposable
    {
        private readonly Dictionary<AnimalId, AnimalInstance> animals = new Dictionary<AnimalId, AnimalInstance>();
        private bool disposed;
        public Dictionary<AnimalId, AnimalInstance>.ValueCollection All => animals.Values;
        public int Count => animals.Count;

        public void Add(AnimalInstance animal)
        {
            if (disposed) throw new ObjectDisposedException(nameof(AnimalPopulation));
            animals.Add(animal.Id, animal);
        }
        public bool Remove(AnimalId id) => animals.Remove(id);

        public bool TryGetAlive(AnimalId id, out AnimalInstance animal) => animals.TryGetValue(id, out animal) && animal.IsAlive;

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            List<Exception> errors = null;
            foreach (var animal in animals.Values)
            {
                try { animal.Dispose(); }
                catch (Exception error) { (errors ??= new List<Exception>()).Add(error); }
            }
            animals.Clear();
            if (errors != null) throw new AggregateException(errors);
        }
    }
}
