using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomerTimer : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image uiFill;
    [SerializeField] private Text uiText;
    [SerializeField] private Canvas timerCanvas;

    [Header("Timer Settings")]
    public float maxDuration = 30f;
    public float eatingDuration = 60f;
    private float currentDuration;
    private float remainingTime;
    
    private bool isPaused = false;
    private bool isActive = false;
    private TimerState currentState = TimerState.Inactive;
    
    private CharacterAI customer;
    private Vector3 offsetPosition = new Vector3(0, 2.5f, 0); // Height above customer's head

    public enum TimerState
    {
        Inactive,
        WaitingForFood,
        Eating,
        Complete
    }

    void Start()
    {
        // Ensure canvas is set to world space
        if (timerCanvas != null)
        {
            timerCanvas.worldCamera = Camera.main;
            timerCanvas.sortingOrder = 100; // High sorting order to be on top
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (customer != null && customer.showDebugInfo)
        {
            Debug.Log($"{customer.gameObject.name}: Timer clicked, state: {currentState}");
        }
    }

    public void Initialize(CharacterAI customerRef, float maxWaitTime, float eatTime)
    {
        customer = customerRef;
        maxDuration = maxWaitTime;
        eatingDuration = eatTime;
        
        // Position timer above customer's head
        PositionTimerAboveCustomer();
        
        // Show the timer
        ShowTimer();
        
        if (customer.showDebugInfo)
        {
            Debug.Log($"{customer.gameObject.name}: Timer initialized at position {transform.position}");
        }
    }

    void PositionTimerAboveCustomer()
    {
        if (customer == null || timerCanvas == null) return;
        
        // Position timer above customer's head
        Vector3 customerPosition = customer.transform.position;
        transform.position = customerPosition + offsetPosition;
        
        // Make canvas face camera
        FaceCamera();
    }

    void FaceCamera()
    {
        if (timerCanvas == null || Camera.main == null) return;
        
        // Make the canvas face the camera
        timerCanvas.transform.rotation = Camera.main.transform.rotation;
        
        // Optional: Keep it billboard style (always face camera)
        Vector3 lookDirection = transform.position - Camera.main.transform.position;
        lookDirection.y = 0; // Keep it upright
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    public void StartWaitingTimer()
    {
        if (customer == null) return;
        
        currentState = TimerState.WaitingForFood;
        currentDuration = maxDuration;
        remainingTime = maxDuration;
        isActive = true;
        isPaused = false;
        
        ShowTimer();
        StartCoroutine(UpdateTimer());
        
        if (customer.showDebugInfo)
        {
            Debug.Log($"{customer.gameObject.name}: Started waiting timer ({maxDuration}s)");
        }
    }

    public void StartEatingTimer()
    {
        if (customer == null) return;
        
        currentState = TimerState.Eating;
        currentDuration = eatingDuration;
        remainingTime = eatingDuration;
        isActive = true;
        isPaused = false;
        
        ShowTimer();
        StartCoroutine(UpdateEatingTimer());
        
        if (customer.showDebugInfo)
        {
            Debug.Log($"{customer.gameObject.name}: Started eating timer ({eatingDuration}s)");
        }
    }

    private System.Collections.IEnumerator UpdateTimer()
    {
        while (isActive && remainingTime > 0 && currentState == TimerState.WaitingForFood)
        {
            if (!isPaused)
            {
                // Update timer display
                uiText.text = $"{Mathf.FloorToInt(remainingTime)}s";
                uiFill.fillAmount = Mathf.InverseLerp(0, currentDuration, remainingTime);
                
                // Color changes based on remaining time
                if (remainingTime < currentDuration * 0.3f) // Less than 30%
                {
                    uiFill.color = Color.red;
                }
                else if (remainingTime < currentDuration * 0.6f) // Less than 60%
                {
                    uiFill.color = Color.yellow;
                }
                else
                {
                    uiFill.color = Color.green;
                }
                
                remainingTime -= Time.deltaTime;
                
                // Update position to follow customer
                PositionTimerAboveCustomer();
            }
            yield return null;
        }
        
        if (currentState == TimerState.WaitingForFood && remainingTime <= 0)
        {
            OnWaitingTimerComplete();
        }
    }

    private System.Collections.IEnumerator UpdateEatingTimer()
    {
        while (isActive && remainingTime > 0 && currentState == TimerState.Eating)
        {
            if (!isPaused)
            {
                // Update timer display
                uiText.text = $"Eat: {Mathf.FloorToInt(remainingTime)}s";
                uiFill.fillAmount = Mathf.InverseLerp(0, currentDuration, remainingTime);
                uiFill.color = Color.blue; // Different color for eating
                
                remainingTime -= Time.deltaTime;
                
                // Update position to follow customer
                PositionTimerAboveCustomer();
            }
            yield return null;
        }
        
        if (currentState == TimerState.Eating && remainingTime <= 0)
        {
            OnEatingTimerComplete();
        }
    }

    private void OnWaitingTimerComplete()
    {
        if (customer == null) return;
        
        if (customer.showDebugInfo)
        {
            Debug.Log($"{customer.gameObject.name}: Food waiting time expired!");
        }
        
        uiFill.color = Color.red;
        uiText.text = "TIME UP!";
        
        // Flash the timer before hiding
        StartCoroutine(FlashTimer(1f));
    }

    private void OnEatingTimerComplete()
    {
        if (customer == null) return;
        
        if (customer.showDebugInfo)
        {
            Debug.Log($"{customer.gameObject.name}: Finished eating!");
        }
        
        uiFill.color = Color.green;
        uiText.text = "DONE!";
        
        // Flash the timer before hiding
        StartCoroutine(FlashTimer(1f));
    }

    private System.Collections.IEnumerator FlashTimer(float duration)
    {
        float flashTime = 0f;
        bool show = true;
        
        while (flashTime < duration)
        {
            if (timerCanvas != null)
            {
                timerCanvas.enabled = show;
            }
            show = !show;
            flashTime += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }
        
        if (timerCanvas != null)
        {
            timerCanvas.enabled = true;
        }
        
        yield return new WaitForSeconds(0.5f);
        StopTimer();
    }

    public void StopTimer()
    {
        isActive = false;
        currentState = TimerState.Complete;
        HideTimer();
        
        if (customer != null && customer.showDebugInfo)
        {
            Debug.Log($"{customer.gameObject.name}: Timer stopped");
        }
    }

    public void PauseTimer(bool pause)
    {
        isPaused = pause;
    }

    private void ShowTimer()
    {
        if (timerCanvas != null)
        {
            timerCanvas.enabled = true;
            FaceCamera();
        }
    }

    private void HideTimer()
    {
        if (timerCanvas != null)
        {
            timerCanvas.enabled = false;
        }
    }

    void Update()
    {
        // Keep timer following customer if active
        if (isActive && customer != null)
        {
            PositionTimerAboveCustomer();
        }
    }

    // Clean up when destroyed
    void OnDestroy()
    {
        StopAllCoroutines();
    }

    // Public properties
    public bool IsActive => isActive;
    public TimerState State => currentState;
    public float RemainingTime => remainingTime;
}