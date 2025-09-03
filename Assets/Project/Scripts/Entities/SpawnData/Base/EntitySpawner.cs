using Project.Scripts.Entities.SpawnData.Strategy;

namespace Project.Scripts.Entities.SpawnData.Base
{
    public class EntitySpawner<T> where T : Entity
    {
        private IEntityFactory<T> entityFactory;
        private ISpawnPointStrategy spawnPointStrategy;

        public EntitySpawner(IEntityFactory<T> entityFactory, ISpawnPointStrategy spawnPointStrategy)
        {
            this.entityFactory = entityFactory;
            this.spawnPointStrategy = spawnPointStrategy;
        }

        public T Spawn()
        {
            return entityFactory.Create(spawnPointStrategy.NextSpawnPoint());
        }
    }
}