using UnityEngine;
using Zenject;
using ZooWorld.Application.Animals;
using ZooWorld.Application.Simulation;

namespace ZooWorld.Unity
{
    public sealed class SimulationDriver : MonoBehaviour
    {
        private SimulationPipeline pipeline;
        private AnimalPopulation population;

        [Inject]
        public void Construct(SimulationPipeline pipeline, AnimalPopulation population)
        {
            this.pipeline = pipeline;
            this.population = population;
        }

        private void FixedUpdate()
        {
            try
            {
                pipeline.Tick(Time.fixedDeltaTime);
            }
            catch (System.Exception error)
            {
                enabled = false;
                // A failed step is not resumable: release this scene's animals, including their physics bodies.
                // Other Unity scenes, the UI and Time.timeScale are left under their own owners' control.
                try { population.Dispose(); }
                catch (System.Exception cleanupError) { error = new System.AggregateException(error, cleanupError); }
                Debug.LogException(error, this);
            }
        }
    }
}
