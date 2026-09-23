using ZooWorld.Domain.Common;

namespace ZooWorld.Application.Simulation
{
    public sealed class SimulationPipeline
    {
        private readonly ISimulationStep[] steps;
        public SimulationPipeline(params ISimulationStep[] steps) => this.steps = (ISimulationStep[])steps.Clone();

        public void Tick(float deltaTime)
        {
            Guard.Positive(deltaTime, nameof(deltaTime));
            foreach (var step in steps) step.Tick(deltaTime);
        }
    }
}
