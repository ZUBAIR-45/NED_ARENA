using UnityEngine;
using UnityEngine.UI;
namespace NED2024
{
    [RequireComponent(typeof(Button), typeof(AudioSource))]
    public class ButtonSound : MonoBehaviour
    {
        public AudioClip clickSfx;
        void Start()
        {
            AudioSource src = GetComponent<AudioSource>();
            GetComponent<Button>().onClick.AddListener(() => src.PlayOneShot(clickSfx));
        }
    }
}
