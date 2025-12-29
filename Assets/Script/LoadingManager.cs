using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public string sceneToLoad = "GameScene";
    public Slider progressBar;

    void Start()
    {
        StartCoroutine(LoadingRoutine());
    }

    IEnumerator LoadingRoutine()
    {
        float duration = 5f;  // 5 seconds
        float timePassed = 0f;

        // Progress bar animation for 5 seconds
        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float progress = timePassed / duration;
            progressBar.value = progress;

            yield return null;
        }

        // After 5 seconds → load scene
        SceneManager.LoadScene(sceneToLoad);
    }
}
