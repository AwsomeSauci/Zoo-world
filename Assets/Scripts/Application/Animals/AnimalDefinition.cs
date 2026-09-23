using System;
using System.Collections.Generic;

namespace ZooWorld.Application.Animals
{
    public sealed class AnimalDefinition
    {
        public string SpeciesId { get; }
        public IReadOnlyList<IAnimalModuleFactory> Modules { get; }

        public AnimalDefinition(string speciesId, params IAnimalModuleFactory[] modules)
        {
            if (string.IsNullOrWhiteSpace(speciesId)) throw new ArgumentException("Species ID is required.", nameof(speciesId));
            if (modules == null || Array.Exists(modules, x => x == null)) throw new ArgumentException("Module factories must be assigned.", nameof(modules));
            SpeciesId = speciesId;
            Modules = Array.AsReadOnly((IAnimalModuleFactory[])modules.Clone());
        }
    }
}
