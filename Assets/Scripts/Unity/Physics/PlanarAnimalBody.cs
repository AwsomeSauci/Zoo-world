using System;
using UnityEngine;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Unity.Physics
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlanarAnimalBody : AnimalBody
    {
        [SerializeField] private Rigidbody physicsBody;
        [SerializeField] private AnimalView view;
        [SerializeField, Min(0.01f)] private float collisionRecovery = 0.45f;
        private Vector3 lastDrive;

        public override PlanarVector Position => new PlanarVector(physicsBody.position.x, physicsBody.position.z);
        public Rigidbody PhysicsBody => physicsBody;

        // A conservative circle encloses the collider footprint, even before the prefab is activated.
        public override float SpawnRadius
        {
            get
            {
                var shape = GetComponent<Collider>();
                switch (shape)
                {
                    case SphereCollider sphere:
                        return HorizontalLength(sphere.center) + sphere.radius;
                    case BoxCollider box:
                        return HorizontalLength(box.center) + HorizontalLength(box.size * 0.5f);
                    case CapsuleCollider capsule:
                        var halfSegment = Mathf.Max(0, capsule.height * 0.5f - capsule.radius);
                        return HorizontalLength(capsule.center) + capsule.radius + (capsule.direction == 1 ? 0 : halfSegment);
                    default:
                        throw new InvalidOperationException($"'{name}' needs a sphere, box or capsule collider.");
                }
            }
        }

        protected override void ResetBody(PlanarVector position)
        {
            lastDrive = Vector3.zero;
            var spawnPosition = new Vector3(position.X, 0, position.Y);
            // Inactive bodies rebuild their physics pose from Transform on activation. Teleport both representations.
            transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            physicsBody.position = spawnPosition;
            physicsBody.rotation = Quaternion.identity;
            physicsBody.linearVelocity = Vector3.zero;
            physicsBody.angularVelocity = Vector3.zero;
            view.ResetPose();
        }

        public override void ApplyMotion(in MotionIntent intent, float deltaTime)
        {
            var desired = new Vector3(intent.Velocity.X, 0, intent.Velocity.Y);
            // Preserve the solver's collision impulse while changing the propulsion contribution.
            var disturbance = physicsBody.linearVelocity - lastDrive;
            disturbance.y = 0;
            var decay = Mathf.Exp(-deltaTime / collisionRecovery);
            physicsBody.AddForce(desired - lastDrive + disturbance * (decay - 1), ForceMode.VelocityChange);
            lastDrive = desired;
            view.SetMotion(desired, intent.VisualElevation);
        }

        private static float HorizontalLength(Vector3 vector) => new Vector2(vector.x, vector.z).magnitude;
    }
}
