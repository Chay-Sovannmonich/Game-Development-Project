using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    private AudioSource music;

    void Awake()
    {
        music = GetComponent<AudioSource>(); // auto-assign the AudioSource

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex >= 2)
        {
            if (!music.isPlaying)
                music.Play();
        }
        else
        {
            if (music.isPlaying)
                music.Stop();
        }
    }
}
