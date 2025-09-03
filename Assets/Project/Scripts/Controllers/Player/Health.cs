using System;
using Project.Scripts.EventSystems;
using UnityEngine;

namespace Project.Scripts.Controllers.Player
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private FloatEventChannel playerHealthChannel;

        private int currentHealth;
        
        public bool IsDead => currentHealth <= 0;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Start()
        {
            PublishHealthPercentage();
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            PublishHealthPercentage();
        }

        private void PublishHealthPercentage()
        {
            if (playerHealthChannel != null)
                playerHealthChannel.Invoke(currentHealth / (float)maxHealth);
        }
    }
}