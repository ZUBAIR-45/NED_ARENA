using UnityEngine;
namespace NED2024
{
    public class KeyPickup : MonoBehaviour
    {
        public static int keysCollected = 0;
        public AudioClip pickupSfx;

        void Update() { transform.Rotate(0, 90 * Time.deltaTime, 0); }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))            // gameobject tag
            {
                keysCollected++;
                GameEvents.OnKeyCollected?.Invoke(keysCollected);   // delegate
                if (pickupSfx) AudioSource.PlayClipAtPoint(pickupSfx, transform.position);
                Destroy(gameObject);
            }
        }
    }
}
