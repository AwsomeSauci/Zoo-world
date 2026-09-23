using System;
using UnityEngine;
using ZooWorld.Application.Animals;
using ZooWorld.Application.Contacts;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Unity.Physics
{
    // Shared lease/contact lifecycle; physical constraints and motion belong to the concrete adapter.
    public abstract class AnimalBody : MonoBehaviour, IAnimalBody
    {
        private IContactSink contacts;
        private Action<AnimalBody> release;
        private bool leased;
        private Collider[] contactColliders;

        public AnimalId Id { get; private set; }
        public abstract PlanarVector Position { get; }
        public abstract float SpawnRadius { get; }
        internal Collider[] ContactColliders => contactColliders ??= GetComponentsInChildren<Collider>(true);

        public abstract void ApplyMotion(in MotionIntent intent, float deltaTime);
        protected abstract void ResetBody(PlanarVector position);

        public void Bind(AnimalId id, PlanarVector position, IContactSink sink, Action<AnimalBody> returnToPool)
        {
            if (leased) throw new InvalidOperationException("The animal body is already leased.");
            leased = true;
            Id = id;
            contacts = sink;
            release = returnToPool;
            ResetBody(position);
        }

        public void Activate()
        {
            if (!leased) throw new InvalidOperationException("An animal body must be bound before activation.");
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            // Scene unload may destroy native objects before the DI scope is disposed.
            if (this) gameObject.SetActive(false);
        }

        public void Dispose()
        {
            if (!leased) return;
            leased = false;
            Deactivate();
            var returnToPool = release;
            contacts = null;
            release = null;
            Id = default;
            if (this) returnToPool?.Invoke(this);
        }

        protected void OnCollisionEnter(Collision collision)
        {
            if (!leased || !isActiveAndEnabled || !collision.rigidbody) return;
            if (collision.rigidbody.TryGetComponent<AnimalBody>(out var other) && other.leased && other.isActiveAndEnabled)
                contacts.Report(Id, other.Id);
        }
    }
}
