using System;
using System.Collections.Generic;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Common;

namespace ZooWorld.Application.Animals
{
    public sealed class AnimalFactory
    {
        private readonly AnimalCatalog catalog;
        private readonly IAnimalBodyPool bodies;
        private long nextId;

        public AnimalFactory(AnimalCatalog catalog, IAnimalBodyPool bodies)
        {
            this.catalog = catalog;
            this.bodies = bodies;
        }

        public AnimalInstance Create(string speciesId, PlanarVector position)
        {
            var definition = catalog.Get(speciesId);
            var id = new AnimalId(checked(++nextId));
            var body = bodies.Rent(speciesId, id, position);
            var modules = new List<IAnimalModule>(definition.Modules.Count);
            try
            {
                foreach (var factory in definition.Modules)
                    modules.Add(factory.Create(id, body) ?? throw new InvalidOperationException("A module factory returned null."));
                return new AnimalInstance(id, body, modules.ToArray());
            }
            catch (Exception creationError)
            {
                var errors = new List<Exception> { creationError };
                for (var i = modules.Count - 1; i >= 0; i--)
                {
                    try { modules[i].Dispose(); }
                    catch (Exception cleanupError) { errors.Add(cleanupError); }
                }
                try { body.Dispose(); }
                catch (Exception cleanupError) { errors.Add(cleanupError); }
                throw new AggregateException($"Could not create species '{speciesId}'.", errors);
            }
        }
    }
}
