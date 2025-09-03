using UnityEngine;

namespace Project.Scripts.Entities.SpawnData.Strategy
{
    public interface ISpawnPointStrategy {
        Transform NextSpawnPoint();
    }
}