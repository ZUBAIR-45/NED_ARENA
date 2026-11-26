using UnityEngine;
using UnityEngine.AI;
namespace NED2024
{
    public enum EnemyState { Patrol, Chase, Attack }

    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        public Transform[] patrolPoints;
        public Transform player;
        public float chaseRange = 8f, attackRange = 1.8f, attackCooldown = 1.5f, damage = 10f;
        private NavMeshAgent agent;
        private EnemyState state = EnemyState.Patrol;
        private int index = 0;
        private float lastAttackTime;   // time-based variable

        void Start() { agent = GetComponent<NavMeshAgent>(); }

        void Update()
        {
            float dist = Vector3.Distance(transform.position, player.position);

            // state change (if else)
            if (dist <= attackRange) state = EnemyState.Attack;
            else if (dist <= chaseRange) state = EnemyState.Chase;
            else state = EnemyState.Patrol;

            switch (state)   // basic state machine
            {
                case EnemyState.Patrol:
                    agent.isStopped = false;
                    agent.SetDestination(patrolPoints[index].position);
                    if (!agent.pathPending && agent.remainingDistance < 0.5f)
                        index = (index + 1) % patrolPoints.Length;   // Mod %
                    break;
                case EnemyState.Chase:
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                    break;
                case EnemyState.Attack:
                    agent.isStopped = true;
                    if (Time.time - lastAttackTime >= attackCooldown)   // elapsed time
                    {
                        lastAttackTime = Time.time;
                        player.GetComponent<Health>().TakeDamage(damage);
                    }
                    break;
            }
        }
    }
}
