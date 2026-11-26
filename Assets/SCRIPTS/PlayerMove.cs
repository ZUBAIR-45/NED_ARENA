using UnityEngine;
namespace NED2024
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMove : MonoBehaviour
    {
        #region Variables
        public float moveSpeed = 6f;      // public: Inspector mein dikhta hai
        public float turnSpeed = 10f;
        public float jumpImpulse = 5f;
        private Rigidbody rb;             // private
        private Animator anim;
        private Transform cam;
        private bool grounded;
        #endregion

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            anim = GetComponentInChildren<Animator>();
            cam = Camera.main.transform;
        }

        void Update()
        {
            float h = Input.GetAxis("Horizontal");   // legacy input
            float v = Input.GetAxis("Vertical");

            // Camera-relative direction
            Vector3 camF = cam.forward; camF.y = 0; camF.Normalize();
            Vector3 camR = cam.right;   camR.y = 0; camR.Normalize();
            Vector3 dir = camF * v + camR * h;

            Vector3 vel = dir.normalized * moveSpeed * Mathf.Clamp01(dir.magnitude); // magnitude
            rb.velocity = new Vector3(vel.x, rb.velocity.y, vel.z);
            // Unity 2022 ya purana ho to: rb.velocity use karo

            if (dir.magnitude > 0.1f)
            {
                Quaternion target = Quaternion.LookRotation(dir);          // Quaternion
                transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.Space) && grounded)
                rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);   // Impulse

            if (anim != null) anim.SetFloat("Speed", dir.magnitude);        // animation parameter
        }

        void OnCollisionStay(Collision c) { grounded = true; }
        void OnCollisionExit(Collision c) { grounded = false; }
    }
}
