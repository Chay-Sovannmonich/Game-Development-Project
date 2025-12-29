using UnityEngine;
using System.Collections;

public class ShowPanelOnce : MonoBehaviour
{
    [SerializeField] private float showDuration = 5f;
    [SerializeField] private string uniqueID = "DefaultPanel";

    private bool hasShown = false;

    void Awake()
    {
        // Check saved state
        hasShown = PlayerPrefs.GetInt(GetKey(), 0) == 1;

        if (hasShown)
        {
            gameObject.SetActive(false);
        }
    }

    public void TriggerShowOnce()
    {
        if (hasShown) return;

        hasShown = true;
        PlayerPrefs.SetInt(GetKey(), 1);
        PlayerPrefs.Save();

        gameObject.SetActive(true);
        StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(showDuration);
        gameObject.SetActive(false);
    }

    public void ResetPanel()
    {
        PlayerPrefs.DeleteKey(GetKey());
        hasShown = false;
        gameObject.SetActive(false);
    }

    private string GetKey()
    {
        return "ShowPanelOnce_" + uniqueID;
    }
}
