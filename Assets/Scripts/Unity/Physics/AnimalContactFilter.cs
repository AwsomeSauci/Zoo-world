using System;
using System.Collections.Concurrent;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using ZooWorld.Application.Contacts;
using ZooWorld.Domain.Animals;

namespace ZooWorld.Unity.Physics
{
    public sealed class AnimalContactFilter : IDisposable
    {
        private readonly ConcurrentDictionary<EntityId, AnimalId> animals = new ConcurrentDictionary<EntityId, AnimalId>();
        private readonly IContactResponsePolicy policy;
        private readonly IContactSink contacts;
        private int disposed;

        public AnimalContactFilter(IContactResponsePolicy policy, IContactSink contacts)
        {
            this.policy = policy;
            this.contacts = contacts;
            UnityEngine.Physics.ContactModifyEvent += ModifyContacts;
            UnityEngine.Physics.ContactModifyEventCCD += ModifyContacts;
        }

        public void Register(Collider collider, AnimalId animal)
        {
            if (Volatile.Read(ref disposed) != 0) throw new ObjectDisposedException(nameof(AnimalContactFilter));
            if (!animals.TryAdd(collider.GetEntityId(), animal))
                throw new InvalidOperationException("The collider already belongs to an animal lease.");
            collider.hasModifiableContacts = true;
        }

        public void Unregister(EntityId collider) => animals.TryRemove(collider, out _);

        private void ModifyContacts(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs)
        {
            if (Volatile.Read(ref disposed) != 0) return;
            // Unity may invoke this callback concurrently on solver threads. Do not access GameObjects here.
            foreach (var pair in pairs)
            {
                if (!animals.TryGetValue(pair.colliderEntityId, out var first) ||
                    !animals.TryGetValue(pair.otherColliderEntityId, out var second) ||
                    !policy.ShouldSuppressImpulse(first, second)) continue;

                for (var i = 0; i < pair.contactCount; i++) pair.IgnoreContact(i);
                // An ignored solver contact need not result in OnCollisionEnter. Preserve the gameplay contact explicitly.
                if (pair.contactCount > 0) contacts.Report(first, second);
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;
            UnityEngine.Physics.ContactModifyEvent -= ModifyContacts;
            UnityEngine.Physics.ContactModifyEventCCD -= ModifyContacts;
            animals.Clear();
        }
    }
}
