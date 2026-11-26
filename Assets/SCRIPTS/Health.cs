using UnityEngine;
namespace NED2024
{
    public class Health : MonoBehaviour
    {
        public float maxHealth = 100f;
        private float current;
        public bool isPlayer;

        void Start() { current = maxHealth; }

        public void TakeDamage(float amount)
        {
            current -= amount;
            if (isPlayer) GameEvents.OnHealthChanged?.Invoke(current / maxHealth);
            if (current <= 0)
            {
                if (isPlayer) GameEvents.OnPlayerDied?.Invoke();
                else Destroy(gameObject);
            }
        }
    }
}
