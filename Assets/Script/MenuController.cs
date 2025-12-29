using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuPanel;
    private bool isMenuOpen = false;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            ToggleMenu();
        }
    }
    
    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        menuPanel.SetActive(isMenuOpen);
        
        // Use TimeManager instead of directly setting Time.timeScale
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetMenuOpen(isMenuOpen);
        }
        else
        {
            // Fallback if TimeManager isn't set up yet
            Time.timeScale = isMenuOpen ? 0f : 1f;
            Debug.LogWarning("TimeManager not found, using direct Time.timeScale");
        }
        
        // Handle cursor
        Cursor.visible = isMenuOpen;
        Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
    
    // Call this from your menu buttons
    public void ResumeGame()
    {
        isMenuOpen = false;
        menuPanel.SetActive(false);
        
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetMenuOpen(false);
        }
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}