using R3;

namespace ZooWorld.Application.Statistics
{
    public interface IDeathStatistics
    {
        ReadOnlyReactiveProperty<DeathCounts> Counts { get; }
    }
}
