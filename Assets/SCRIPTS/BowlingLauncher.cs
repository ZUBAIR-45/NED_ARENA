using UnityEngine;
namespace NED2024
{
    public class BowlingLauncher : MonoBehaviour
    {
        public GameObject ballPrefab;
        public Transform spawnPoint;
        public float force = 25f;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                GameObject ball = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
                ball.GetComponent<Rigidbody>().AddForce(spawnPoint.forward * force, ForceMode.Impulse);
                Destroy(ball, 8f);
            }
        }
    }
}
