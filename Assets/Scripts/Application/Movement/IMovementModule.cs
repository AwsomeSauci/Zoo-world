using ZooWorld.Application.Animals;

namespace ZooWorld.Application.Movement
{
    // The exclusive owner of an animal's propulsion, shared by alternative movement modules.
    public interface IMovementModule : IAnimalTickModule { }
}
