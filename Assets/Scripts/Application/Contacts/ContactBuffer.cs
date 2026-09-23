using System;
using System.Collections.Generic;
using ZooWorld.Domain.Animals;

namespace ZooWorld.Application.Contacts
{
    public sealed class ContactBuffer : IContactSink
    {
        private readonly HashSet<ContactPair> pending = new HashSet<ContactPair>();
        private readonly object sync = new object();

        public void Report(AnimalId a, AnimalId b)
        {
            if (a.Value <= 0 || b.Value <= 0 || a == b) return;
            lock (sync) pending.Add(new ContactPair(a, b));
        }

        public void DrainTo(List<ContactPair> destination)
        {
            destination.Clear();
            lock (sync)
            {
                destination.AddRange(pending);
                pending.Clear();
            }
            destination.Sort();
        }
    }
}
