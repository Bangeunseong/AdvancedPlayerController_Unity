using Project.Scripts.Entities.SpawnData.Base;
using UnityEngine;

namespace Project.Scripts.Entities.SpawnData.Concrete
{
    [CreateAssetMenu(fileName = "CollectibleData", menuName = "Platformer/Collectible Data")]
    public class CollectibleData : EntityData
    {
        public int score;
        // additional properties specific to collectibles
    }
}