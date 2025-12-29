using UnityEngine;
using UnityEngine.UI;

public class DisappearOnClick : MonoBehaviour
{
    [Header("Settings")]
    public GameObject targetObject; // The object that will disappear
    public Button triggerButton; // The button that makes it disappear
    
    [Header("Options")]
    public float delay = 0f; // Optional delay before disappearing
    public bool destroyInstead = false; // Destroy instead of just hiding

    void Start()
    {
        // Add click listener to the button
        if (triggerButton != null)
        {
            triggerButton.onClick.AddListener(OnButtonClicked);
        }
    }

    public void OnButtonClicked()
    {
        if (targetObject != null)
        {
            if (delay > 0)
            {
                Invoke("MakeObjectDisappear", delay);
            }
            else
            {
                MakeObjectDisappear();
            }
        }
    }

    void MakeObjectDisappear()
    {
        if (destroyInstead)
        {
            Destroy(targetObject);
            Debug.Log("Object destroyed!");
        }
        else
        {
            targetObject.SetActive(false);
            Debug.Log("Object hidden!");
        }
    }

    // Call this method directly from button's OnClick event
    public void MakeTargetDisappear()
    {
        OnButtonClicked();
    }
}