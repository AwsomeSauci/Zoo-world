using System;
using UnityEngine;
using Zenject;
using ZooWorld.Application.Animals;

namespace ZooWorld.Composition.Animals
{
    public abstract class AnimalModuleAsset : ScriptableObject
    {
        // A capability may have at most one provider in an animal definition.
        // Extensions introduce their own contract type; no central enum needs editing.
        public abstract Type ExclusiveCapability { get; }
        public abstract IAnimalModuleFactory CreateFactory(DiContainer container);
    }
}
