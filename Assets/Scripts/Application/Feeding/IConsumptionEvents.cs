using R3;
using ZooWorld.Domain.Feeding;

namespace ZooWorld.Application.Feeding
{
    public interface IConsumptionEvents
    {
        Observable<Consumption> Consumed { get; }
    }
}
