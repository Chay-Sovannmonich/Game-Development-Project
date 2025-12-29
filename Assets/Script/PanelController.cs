using UnityEngine;
using UnityEngine.UI;

public class PanelWithCloseButton : MonoBehaviour
{
    [Header("Main UI References")]
    [SerializeField] private Button openButton; // Button to open the panel
    [SerializeField] private GameObject panel;  // The panel to show/hide
    [SerializeField] private Button closeButton; // Button inside panel to close it

    [Header("Close Button Settings")]
    [SerializeField] private bool closeOnOutsideClick = false;
    [SerializeField] private bool closeOnEscape = true;

    [Header("Animation Settings")]
    [SerializeField] private bool useAnimation = true;
    [SerializeField] private float animationSpeed = 0.2f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isPanelVisible = false;
    private CanvasGroup canvasGroup;

    void Start()
    {
        InitializePanel();
        SetupButtonListeners();
    }

    void Update()
    {
        // Close panel with Escape key
        if (closeOnEscape && Input.GetKeyDown(KeyCode.Escape) && isPanelVisible)
        {
            HidePanel();
        }
    }

    void InitializePanel()
    {
        // Hide panel at start
        panel.SetActive(false);
        isPanelVisible = false;

        // Setup canvas group for animation
        if (useAnimation)
        {
            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    void SetupButtonListeners()
    {
        // Open button listener
        if (openButton != null)
        {
            openButton.onClick.AddListener(ShowPanel);
        }
        else
        {
            Debug.LogWarning("Open Button not assigned!");
        }

        // Close button listener
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePanel);
        }
        else
        {
            Debug.LogWarning("Close Button not assigned! You can add a button inside your panel.");
        }
    }

    public void ShowPanel()
    {
        if (isPanelVisible) return;

        panel.SetActive(true);
        isPanelVisible = true;

        if (useAnimation)
        {
            StartCoroutine(AnimatePanel(0f, 1f, true));
        }
        else
        {
            SetPanelInteractive(true);
        }
    }

    public void HidePanel()
    {
        if (!isPanelVisible) return;

        if (useAnimation)
        {
            StartCoroutine(AnimatePanel(1f, 0f, false));
        }
        else
        {
            panel.SetActive(false);
        }
        isPanelVisible = false;
    }

    private System.Collections.IEnumerator AnimatePanel(float startAlpha, float endAlpha, bool enableAfterAnimation)
    {
        float elapsedTime = 0f;

        while (elapsedTime < animationSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationSpeed;
            float curvedT = animationCurve.Evaluate(t);
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, curvedT);
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = currentAlpha;
            }
            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = endAlpha;
        }

        if (!enableAfterAnimation)
        {
            panel.SetActive(false);
        }
        else
        {
            SetPanelInteractive(true);
        }
    }

    void SetPanelInteractive(bool isInteractive)
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = isInteractive;
            canvasGroup.blocksRaycasts = isInteractive;
        }
    }

    // Optional: Method to close when clicking outside the panel
    public void CloseOnOutsideClick(Vector2 mousePosition)
    {
        if (!isPanelVisible || !closeOnOutsideClick) return;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (!RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePosition))
        {
            HidePanel();
        }
    }

    void OnDestroy()
    {
        // Clean up listeners
        if (openButton != null)
        {
            openButton.onClick.RemoveListener(ShowPanel);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HidePanel);
        }
    }
}