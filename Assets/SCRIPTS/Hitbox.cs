using UnityEngine;
namespace NED2024
{
    public class Hitbox : MonoBehaviour
    {
        public float damage = 25f;
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy")) other.GetComponent<Health>()?.TakeDamage(damage);
        }
    }
}
