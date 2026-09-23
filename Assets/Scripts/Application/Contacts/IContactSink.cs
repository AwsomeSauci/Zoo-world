using System;
using System.Collections.Generic;
using ZooWorld.Domain.Animals;

namespace ZooWorld.Application.Contacts
{
    public interface IContactSink
    {
        void Report(AnimalId a, AnimalId b);
    }
}
