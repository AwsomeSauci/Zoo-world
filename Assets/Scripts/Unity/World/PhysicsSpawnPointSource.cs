using UnityEngine;
using ZooWorld.Application.Movement;
using ZooWorld.Application.Spawning;
using ZooWorld.Domain.Common;
using ZooWorld.Unity.Physics;

namespace ZooWorld.Unity.World
{
    public sealed class PhysicsSpawnPointSource : ISpawnPointSource
    {
        private readonly IWorldBoundsProvider bounds;
        private readonly IAnimalPrefabCatalog catalog;
        private readonly IRandomSource random;
        private readonly int blockingLayers;

        public PhysicsSpawnPointSource(IWorldBoundsProvider bounds, IAnimalPrefabCatalog catalog, IRandomSource random, int blockingLayers)
        {
            this.bounds = bounds;
            this.catalog = catalog;
            this.random = random;
            this.blockingLayers = blockingLayers;
        }

        public bool TryFind(string speciesId, out PlanarVector position)
        {
            var area = bounds.Bounds;
            var radius = catalog.Get(speciesId).SpawnRadius;
            var margin = radius + 0.35f;
            var width = area.Max.X - area.Min.X - 2 * margin;
            var depth = area.Max.Y - area.Min.Y - 2 * margin;
            position = default;
            if (width <= 0 || depth <= 0) return false;
            for (var attempt = 0; attempt < 24; attempt++)
            {
                position = new PlanarVector(area.Min.X + margin + random.NextUnit() * width,
                    area.Min.Y + margin + random.NextUnit() * depth);
                if (!UnityEngine.Physics.CheckSphere(new Vector3(position.X, 0, position.Y), radius + 0.1f,
                        blockingLayers, QueryTriggerInteraction.Ignore)) return true;
            }
            return false;
        }
    }
}
