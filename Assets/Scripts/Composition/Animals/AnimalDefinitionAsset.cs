using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using ZooWorld.Application.Animals;
using ZooWorld.Application.Feeding;
using ZooWorld.Application.Movement;
using ZooWorld.Unity.Physics;

namespace ZooWorld.Composition.Animals
{
    [CreateAssetMenu(menuName = "Zoo World/Animal")]
    public sealed class AnimalDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string speciesId;
        [SerializeField] private AnimalBody prefab;
        [SerializeField] private AnimalModuleAsset[] modules = Array.Empty<AnimalModuleAsset>();
        public string SpeciesId => speciesId;
        public AnimalBody Prefab => prefab;

        public AnimalDefinition Compile(DiContainer container)
        {
            if (!prefab) throw new InvalidOperationException($"'{name}' needs an animal prefab.");
            if (modules == null) throw new InvalidOperationException($"'{name}' needs animal modules.");
            var capabilities = new HashSet<Type>();
            foreach (var module in modules)
            {
                if (!module) throw new InvalidOperationException($"'{name}' has a missing module.");
                if (module.ExclusiveCapability == null || !capabilities.Add(module.ExclusiveCapability))
                    throw new InvalidOperationException($"'{name}' has conflicting module capability: {module.name}.");
            }
            if (!capabilities.Contains(typeof(IMovementModule)) || !capabilities.Contains(typeof(IFeedingModule)))
                throw new InvalidOperationException($"'{name}' needs movement and feeding modules.");
            var factories = new IAnimalModuleFactory[modules.Length];
            for (var i = 0; i < factories.Length; i++) factories[i] = modules[i].CreateFactory(container);
            return new AnimalDefinition(speciesId, factories);
        }
    }
}
