using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownIntegerRealtime : MonoBehaviour
{
    [Header("UI")]
    public GameObject countdownPanel;
    public TextMeshProUGUI countdownText;

    [Header("Settings")]
    public int startSeconds = 30;
    public bool useRealtime = true;

    [Header("On Countdown Finish")]
    public GameObject targetObject;      // Object to activate when done
    public MonoBehaviour targetScript;   // call a script function
    public string methodName;            // name of function to call

    private Coroutine countdownCoroutine;
    private int timeLeft;

    void Start()
    {
        // Make sure target object is hidden at start
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }

    public void StartCountdown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        timeLeft = startSeconds;
        
        // Hide target object when countdown starts (in case it was shown before)
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
        
        countdownPanel.SetActive(true);

        countdownCoroutine = StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        while (timeLeft > 0)
        {
            countdownText.text = $"  Rice Ball\n\n00 : 00 : {timeLeft}s";

            yield return useRealtime 
                ? new WaitForSecondsRealtime(1f) 
                : new WaitForSeconds(1f);

            timeLeft--;
        }

        // FINISHED
        countdownText.text = "         Time's Up!";
        
        // Wait a moment before hiding panel and showing object
        yield return useRealtime 
            ? new WaitForSecondsRealtime(1f) 
            : new WaitForSeconds(1f);
            
        countdownPanel.SetActive(false);

        // 1) Activate object - ONLY APPEARS WHEN COUNTDOWN FINISHES
        if (targetObject != null)
        {
            targetObject.SetActive(true);
            Debug.Log("Target object activated after countdown finished!");
        }

        // 2) Call method on script
        if (targetScript != null && !string.IsNullOrEmpty(methodName))
            targetScript.Invoke(methodName, 0f);

        countdownCoroutine = null;
    }

    // Optional: Public method to manually hide the target object
    public void HideTargetObject()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }

    // Optional: Public method to check if countdown is running
    public bool IsCountdownRunning()
    {
        return countdownCoroutine != null;
    }

    // Optional: Stop countdown early
    public void StopCountdownEarly()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
        
        countdownPanel.SetActive(false);
        
        // Don't show the target object if stopped early
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }
}