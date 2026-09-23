using System;
using R3;
using ZooWorld.Domain.Feeding;

namespace ZooWorld.Application.Feeding
{
    public sealed class ConsumptionEvents : IConsumptionEvents, IDisposable
    {
        private readonly Subject<Consumption> consumed = new Subject<Consumption>();
        public Observable<Consumption> Consumed => consumed;

        public void Publish(in Consumption consumption) => consumed.OnNext(consumption);
        public void Dispose() => consumed.Dispose();
    }
}
