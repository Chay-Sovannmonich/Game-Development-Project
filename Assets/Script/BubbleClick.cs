using UnityEngine;

public class BubbleClick : MonoBehaviour
{
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private bool hideAfterClick = true;

    private bool clicked = false;

    void OnMouseDown()
    {
        if (clicked) return;
        clicked = true;

        if (menuCanvas == null)
        {
            Debug.LogWarning("Menu Canvas not assigned!");
            return;
        }

        // Show menu canvas
        menuCanvas.SetActive(true);

        // Trigger all ShowPanelOnce panels
        ShowPanelOnce[] panels = menuCanvas.GetComponentsInChildren<ShowPanelOnce>(true);
        foreach (ShowPanelOnce panel in panels)
        {
            panel.TriggerShowOnce();
        }

        // Pause game
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetMenuOpen(true);
        }
        else
        {
            Time.timeScale = 0f;
        }

        // Hide bubble if needed
        if (hideAfterClick)
        {
            gameObject.SetActive(false);
        }
    }

    // =========================
    // 🔁 METHODS YOUR SpawnManager NEEDS
    // =========================

    // Reappear bubble AND unpause game
    public void RespawnBubble()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetMenuOpen(false);
        }
        else
        {
            Time.timeScale = 1f;
        }

        clicked = false;
        gameObject.SetActive(true);

        Debug.Log("Bubble respawned and game unpaused");
    }

    // Show bubble only (do NOT change pause state)
    public void ShowBubbleOnly()
    {
        clicked = false;
        gameObject.SetActive(true);

        Debug.Log("Bubble shown only");
    }

    // Close menu safely
    public void CloseMenu()
    {
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false);
        }

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetMenuOpen(false);
        }
        else
        {
            Time.timeScale = 1f;
        }

        clicked = false;
    }

    // Reset panels (testing)
    public void ResetAllPanels()
    {
        if (menuCanvas == null) return;

        ShowPanelOnce[] panels = menuCanvas.GetComponentsInChildren<ShowPanelOnce>(true);
        foreach (ShowPanelOnce panel in panels)
        {
            panel.ResetPanel();
        }

        Debug.Log("All panels reset");
    }
}
