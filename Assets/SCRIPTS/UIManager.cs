using UnityEngine;
using UnityEngine.UI;
namespace NED2024
{
    public class UIManager : MonoBehaviour
    {
        public Text keyText, timerText;
        public Slider healthSlider, volumeSlider;
        public Toggle muteToggle;
        public GameObject gameOverPanel;
        private float elapsed;

        void OnEnable()
        {
            GameEvents.OnKeyCollected += UpdateKeys;      // subscribe
            GameEvents.OnHealthChanged += UpdateHealth;
            GameEvents.OnPlayerDied += ShowGameOver;
        }
        void OnDisable()
        {
            GameEvents.OnKeyCollected -= UpdateKeys;      // unsubscribe
            GameEvents.OnHealthChanged -= UpdateHealth;
            GameEvents.OnPlayerDied -= ShowGameOver;
        }
        void Start()
        {
            KeyPickup.keysCollected = 0;
            volumeSlider.onValueChanged.AddListener(v => AudioListener.volume = v);
            muteToggle.onValueChanged.AddListener(m => AudioListener.pause = m);
        }
        void Update()
        {
            elapsed += Time.deltaTime;   // elapsed time
            timerText.text = "Time: " + elapsed.ToString("F1");
        }
        void UpdateKeys(int n) { keyText.text = "Keys: " + n + "/3"; }
        void UpdateHealth(float pct) { healthSlider.value = pct; }
        void ShowGameOver() { gameOverPanel.SetActive(true); Time.timeScale = 0f; }
    }
}
