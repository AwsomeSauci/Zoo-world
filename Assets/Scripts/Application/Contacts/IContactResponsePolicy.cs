using ZooWorld.Domain.Animals;

namespace ZooWorld.Application.Contacts
{
    public interface IContactResponsePolicy
    {
        // Called by the physics adapter on solver threads. Implementations must be thread-safe and side-effect-free.
        bool ShouldSuppressImpulse(AnimalId first, AnimalId second);
    }
}
