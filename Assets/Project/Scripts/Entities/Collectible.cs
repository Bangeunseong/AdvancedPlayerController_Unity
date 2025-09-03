using System;
using Project.Scripts.EventSystems;
using UnityEngine;

namespace Project.Scripts.Entities
{
    public class Collectible : Entity
    {
        [SerializeField] private int score = 10;
        [SerializeField] private IntEventChannel scoreChannel;

        public void SetScoreChannel(IntEventChannel channel)
        {
            scoreChannel = channel;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                scoreChannel.Invoke(score);
                Destroy(gameObject);
            }
        }
    }
}