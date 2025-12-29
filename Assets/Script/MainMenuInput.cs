using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuInput : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "GameScene"; // Change to your game scene name

    void Update()
    {
        // Left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            LoadGame();
        }

        // Space or Enter key
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            LoadGame();
        }
    }

    void LoadGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
