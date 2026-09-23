namespace ZooWorld.Domain.Common
{
    public interface IRandomSource
    {
        // Values are in [0, 1).
        float NextUnit();
    }
}
