namespace ZooWorld.Unity.Physics
{
    public interface IAnimalPrefabCatalog
    {
        AnimalBody Get(string speciesId);
    }
}
