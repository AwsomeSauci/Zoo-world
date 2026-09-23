using System;
using System.Collections.Generic;

namespace ZooWorld.Application.Animals
{
    public sealed class AnimalCatalog
    {
        private readonly Dictionary<string, AnimalDefinition> definitions = new Dictionary<string, AnimalDefinition>(StringComparer.Ordinal);

        public AnimalCatalog(IEnumerable<AnimalDefinition> definitions)
        {
            foreach (var definition in definitions)
            {
                if (this.definitions.ContainsKey(definition.SpeciesId))
                    throw new ArgumentException($"Duplicate species ID: {definition.SpeciesId}");
                this.definitions.Add(definition.SpeciesId, definition);
            }
            if (this.definitions.Count == 0) throw new ArgumentException("The animal catalog cannot be empty.");
        }

        public AnimalDefinition Get(string speciesId) => definitions.TryGetValue(speciesId, out var definition)
            ? definition : throw new KeyNotFoundException($"Unknown species: {speciesId}");
    }
}
