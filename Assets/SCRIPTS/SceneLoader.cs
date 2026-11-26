using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace NED2024
{
    public class SceneLoader : MonoBehaviour
    {
        public Slider loadingBar;
        public GameObject loadingPanel;

        public void LoadGame(string sceneName) { StartCoroutine(Load(sceneName)); }  // Button OnClick se call

        IEnumerator Load(string sceneName)
        {
            Time.timeScale = 1f;
            loadingPanel.SetActive(true);
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone)
            {
                loadingBar.value = Mathf.Clamp01(op.progress / 0.9f);
                yield return null;
            }
        }
        public void Quit() { Application.Quit(); }
    }
}
