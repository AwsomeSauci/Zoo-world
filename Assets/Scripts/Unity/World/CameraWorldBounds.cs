using System;
using UnityEngine;
using ZooWorld.Application.Movement;
using ZooWorld.Application.Simulation;
using ZooWorld.Domain.Common;
using ZooWorld.Domain.Movement;

namespace ZooWorld.Unity.World
{
    public sealed class CameraWorldBounds : IWorldBoundsProvider, ISimulationStep
    {
        private readonly Camera camera;
        private readonly Plane plane = new Plane(Vector3.up, Vector3.zero);
        public CameraWorldBounds(Camera camera)
        {
            this.camera = camera;
            Tick(0);
        }

        public WorldBounds Bounds { get; private set; }

        public void Tick(float deltaTime)
        {
            var a = Project(0, 0);
            var b = Project(1, 1);
            Bounds = new WorldBounds(new PlanarVector(Mathf.Min(a.x, b.x), Mathf.Min(a.z, b.z)),
                new PlanarVector(Mathf.Max(a.x, b.x), Mathf.Max(a.z, b.z)));
        }

        private Vector3 Project(float x, float y)
        {
            var ray = camera.ViewportPointToRay(new Vector3(x, y, 0));
            if (!plane.Raycast(ray, out var distance)) throw new InvalidOperationException("The zoo camera must face the ground plane.");
            return ray.GetPoint(distance);
        }
    }
}
