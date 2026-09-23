using System.Collections.Generic;
using ZooWorld.Application.Animals;
using ZooWorld.Application.Feeding;
using ZooWorld.Application.Simulation;
using ZooWorld.Application.Statistics;
using ZooWorld.Domain.Feeding;

namespace ZooWorld.Application.Contacts
{
    public sealed class ContactResolutionService : ISimulationStep
    {
        private readonly ContactBuffer contacts;
        private readonly IAnimalLookup animals;
        private readonly FoodChainRegistry foodChain;
        private readonly IContactRule rules;
        private readonly IAnimalDeathService deaths;
        private readonly DeathStatistics statistics;
        private readonly ConsumptionEvents events;
        private readonly List<ContactPair> processing = new List<ContactPair>();

        public ContactResolutionService(ContactBuffer contacts, IAnimalLookup animals, FoodChainRegistry foodChain,
            IContactRule rules, IAnimalDeathService deaths, DeathStatistics statistics, ConsumptionEvents events)
        {
            this.contacts = contacts;
            this.animals = animals;
            this.foodChain = foodChain;
            this.rules = rules;
            this.deaths = deaths;
            this.statistics = statistics;
            this.events = events;
        }

        public void Tick(float deltaTime)
        {
            contacts.DrainTo(processing);
            foreach (var contact in processing)
            {
                if (!animals.TryGetAlive(contact.A, out _) || !animals.TryGetAlive(contact.B, out _)) continue;
                if (!foodChain.TryGet(contact.A, out var a) || !foodChain.TryGet(contact.B, out var b)) continue;
                if (!rules.TryResolve(contact.A, a, contact.B, b, out var consumption)) continue;
                if (!deaths.TryKill(consumption.Victim)) continue;
                statistics.Record(consumption.VictimRole);
                events.Publish(in consumption);
            }
            processing.Clear();
        }
    }
}
