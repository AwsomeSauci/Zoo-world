using System;
using System.Collections.Generic;
using ZooWorld.Domain.Animals;

namespace ZooWorld.Application.Animals
{
    public sealed class AnimalInstance : IDisposable
    {
        private readonly IAnimalModule[] modules;
        private readonly List<IAnimalTickModule> tickModules = new List<IAnimalTickModule>();
        private bool disposed;
        public AnimalId Id { get; }
        public IAnimalBody Body { get; }
        public bool IsAlive { get; private set; } = true;

        public AnimalInstance(AnimalId id, IAnimalBody body, IAnimalModule[] modules)
        {
            Id = id;
            Body = body;
            this.modules = (IAnimalModule[])modules.Clone();
            foreach (var module in modules)
                if (module is IAnimalTickModule tickModule) tickModules.Add(tickModule);
        }

        public void Tick(float deltaTime)
        {
            foreach (var module in tickModules)
            {
                if (!IsAlive) break;
                module.Tick(deltaTime);
            }
        }

        public bool TryMarkDead()
        {
            if (!IsAlive) return false;
            IsAlive = false;
            Body.Deactivate();
            return true;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            // Release every resource even when a custom module fails to dispose.
            List<Exception> errors = null;
            try { TryMarkDead(); }
            catch (Exception error) { (errors ??= new List<Exception>()).Add(error); }
            for (var i = modules.Length - 1; i >= 0; i--)
            {
                try { modules[i].Dispose(); }
                catch (Exception error) { (errors ??= new List<Exception>()).Add(error); }
            }
            try { Body.Dispose(); }
            catch (Exception error) { (errors ??= new List<Exception>()).Add(error); }
            if (errors != null) throw new AggregateException(errors);
        }
    }
}
