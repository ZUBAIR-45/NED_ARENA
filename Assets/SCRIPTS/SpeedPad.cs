using UnityEngine;
namespace NED2024
{
    public class SpeedPad : MonoBehaviour
    {
        public float boost = 20f;
        void OnTriggerStay(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null) rb.AddForce(transform.forward * boost, ForceMode.Acceleration); // Acceleration
        }
    }
}
