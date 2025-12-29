using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DelayedPanelController : MonoBehaviour
{
    [Header("Trigger (World Object)")]
    [SerializeField] private GameObject triggerObject;
    [SerializeField] private LayerMask triggerLayer = Physics.DefaultRaycastLayers;

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image image; // use Image (recommended)
    [SerializeField] private Vector2 panelSize = new Vector2(400, 300);
    [SerializeField] private float delayTime = 1f;

    [Header("Animation")]
    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeDuration = 0.4f;

    private Camera mainCam;
    private RectTransform panelRect;
    private CanvasGroup canvasGroup;
    private bool triggered = false;

    void Awake()
    {
        mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("No camera tagged MainCamera!");
        }
    }

    void Start()
    {
        // ===== PANEL SETUP =====
        panelRect = panel.GetComponent<RectTransform>();

        // FORCE NON-STRETCH (THIS FIXES YOUR SIZE ISSUE)
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;
        panelRect.anchoredPosition = Vector2.zero;

        panel.SetActive(false);

        // CanvasGroup for fade
        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        // Hide image at start
        if (image != null)
            image.gameObject.SetActive(false);

        // Make sure trigger has collider
        if (triggerObject.GetComponent<Collider>() == null)
        {
            triggerObject.AddComponent<BoxCollider>();
            Debug.Log("BoxCollider added to trigger object");
        }
    }

    void Update()
    {
        if (triggered) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, triggerLayer))
            {
                if (hit.collider.gameObject == triggerObject)
                {
                    triggered = true;
                    StartCoroutine(ShowPanelAfterDelay());
                }
            }
        }
    }

    IEnumerator ShowPanelAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);

        panel.SetActive(true);

        if (image != null)
            image.gameObject.SetActive(true);

        if (useFade)
            StartCoroutine(FadeIn());
        else
            canvasGroup.alpha = 1f;
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        canvasGroup.alpha = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = t / fadeDuration;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }
}
