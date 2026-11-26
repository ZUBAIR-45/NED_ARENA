using UnityEngine;
namespace NED2024
{
    public class ExitDoor : MonoBehaviour
    {
        public int keysNeeded = 3;
        public float openSpeed = 2f;
        private bool open;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && KeyPickup.keysCollected >= keysNeeded) open = true;
        }
        void Update()
        {
            if (open && transform.position.y < 4f)
                transform.Translate(Vector3.up * openSpeed * Time.deltaTime);
        }
    }
}
