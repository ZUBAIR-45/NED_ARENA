using UnityEngine;
namespace NED2024
{
    // Animator ke Layer 0 mein "IK Pass" ON karna zaroori hai
    [RequireComponent(typeof(Animator))]
    public class IKLook : MonoBehaviour
    {
        public Transform lookTarget;
        [Range(0, 1)] public float weight = 1f;
        private Animator anim;
        void Start() { anim = GetComponent<Animator>(); }
        void OnAnimatorIK(int layerIndex)
        {
            if (lookTarget == null) return;
            anim.SetLookAtWeight(weight);
            anim.SetLookAtPosition(lookTarget.position);
        }
    }
}
