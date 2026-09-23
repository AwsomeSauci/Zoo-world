using System;
using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Application.Animals;
using ZooWorld.Application.Contacts;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Common;

namespace ZooWorld.Unity.Physics
{
    public sealed class AnimalBodyPool : IAnimalBodyPool, IDisposable
    {
        private readonly IAnimalPrefabCatalog catalog;
        private readonly IContactSink contacts;
        private readonly AnimalContactFilter contactFilter;
        private readonly Transform activeRoot;
        private readonly Transform inactiveRoot;
        private readonly Dictionary<AnimalBody, Stack<AnimalBody>> pools = new Dictionary<AnimalBody, Stack<AnimalBody>>();
        private readonly List<AnimalBody> created = new List<AnimalBody>();
        private bool disposed;

        public AnimalBodyPool(IAnimalPrefabCatalog catalog, IContactSink contacts, Transform activeRoot,
            Transform inactiveRoot, AnimalContactFilter contactFilter)
        {
            this.catalog = catalog;
            this.contacts = contacts;
            this.contactFilter = contactFilter;
            this.activeRoot = activeRoot;
            this.inactiveRoot = inactiveRoot;
        }

        public IAnimalBody Rent(string speciesId, AnimalId id, PlanarVector position)
        {
            if (disposed) throw new ObjectDisposedException(nameof(AnimalBodyPool));
            var prefab = catalog.Get(speciesId);
            if (!pools.TryGetValue(prefab, out var pool)) pools.Add(prefab, pool = new Stack<AnimalBody>());
            AnimalBody body;
            if (pool.Count > 0) body = pool.Pop();
            else
            {
                body = UnityEngine.Object.Instantiate(prefab, inactiveRoot);
                body.gameObject.SetActive(false);
                created.Add(body);
            }

            var registered = new List<EntityId>(body.ContactColliders.Length);
            try
            {
                body.transform.SetParent(activeRoot, false);
                body.Bind(id, position, contacts, returned => Return(returned, pool, registered));
                foreach (var collider in body.ContactColliders)
                {
                    contactFilter.Register(collider, id);
                    registered.Add(collider.GetEntityId());
                }
                return body;
            }
            catch
            {
                // A partially initialized lease must never enter the reusable pool.
                foreach (var collider in registered) contactFilter.Unregister(collider);
                created.Remove(body);
                UnityEngine.Object.Destroy(body.gameObject);
                throw;
            }
        }

        private void Return(AnimalBody body, Stack<AnimalBody> pool, List<EntityId> registered)
        {
            foreach (var collider in registered) contactFilter.Unregister(collider);
            if (disposed || !inactiveRoot)
            {
                UnityEngine.Object.Destroy(body.gameObject);
                return;
            }
            body.transform.SetParent(inactiveRoot, false);
            pool.Push(body);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (var body in created)
                if (body) UnityEngine.Object.Destroy(body.gameObject);
            created.Clear();
            pools.Clear();
        }
    }
}
