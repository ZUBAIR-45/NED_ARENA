using UnityEngine;
namespace NED2024
{
    public class PlayerCombat : MonoBehaviour
    {
        public Collider hitbox;          // chota trigger collider (hand/sword), isTrigger ON
        public LayerMask enemyMask;      // Enemy layer
        public float rayRange = 10f;
        public float attackCooldown = 0.6f;
        private float lastAttack;
        private Animator anim;

        void Start() { anim = GetComponentInChildren<Animator>(); if (hitbox) hitbox.enabled = false; }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) && Time.time - lastAttack > attackCooldown)
            {
                lastAttack = Time.time;
                if (anim) anim.SetTrigger("Attack");
                hitbox.enabled = true;               // close quarter hitbox
                Invoke(nameof(DisableHitbox), 0.3f);
            }

            // Raycast: enemy samne dikhe to Debug mein dikhao
            if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out RaycastHit hit, rayRange, enemyMask))
                Debug.DrawLine(transform.position + Vector3.up, hit.point, Color.red);
        }
        void DisableHitbox() { hitbox.enabled = false; }
    }
}
