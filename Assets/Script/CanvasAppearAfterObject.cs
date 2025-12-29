using UnityEngine;
using System.Collections;

public class CanvasShowOnDestroy : MonoBehaviour
{
    public GameObject targetObject; // The object that will be destroyed
    public Canvas canvasToShow; // Canvas to show when target is destroyed
    
    [Header("Disappear Settings")]
    public float showDuration = 3f; // How long to show the canvas before hiding it
    public bool hideInsteadOfDisable = false; // Hide canvas vs disable it
    public bool destroyCanvasAfter = false; // Destroy the canvas GameObject
    
    [Header("Call Another Object")]
    public GameObject objectToCall; // Object to call/activate after 1 second
    public float callDelay = 1f; // How many seconds after canvas appears to call the object
    public bool enableObject = true; // Enable the object
    public bool activateComponent = false; // Activate a specific component
    public string componentName = ""; // Name of component to activate
    public bool sendMessage = false; // Send a message to the object
    public string messageName = "OnCanvasShown"; // Message to send
    
    private Coroutine hideCoroutine;
    private Coroutine callCoroutine;
    private bool hasCalledObject = false;
    
    void Start()
    {
        // Get canvas if not assigned
        if (canvasToShow == null)
        {
            canvasToShow = GetComponent<Canvas>();
        }
        
        // Hide canvas initially
        if (canvasToShow != null)
        {
            canvasToShow.enabled = false;
        }
    }
    
    void Update()
    {
        // Simple check - if target is null/destroyed, show canvas
        if (targetObject == null && canvasToShow != null && !canvasToShow.enabled)
        {
            canvasToShow.enabled = true;
            Debug.Log("Target destroyed - showing canvas!");
            
            // Start the call another object timer
            if (callCoroutine != null)
                StopCoroutine(callCoroutine);
            
            callCoroutine = StartCoroutine(CallObjectAfterDelay());
            
            // Start the hide timer
            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);
            
            hideCoroutine = StartCoroutine(HideCanvasAfterDelay());
        }
    }
    
    IEnumerator CallObjectAfterDelay()
    {
        if (hasCalledObject) yield break;
        
        // Wait for the specified delay
        yield return new WaitForSeconds(callDelay);
        
        // Call/activate the other object
        CallOtherObject();
        
        hasCalledObject = true;
    }
    
    void CallOtherObject()
    {
        if (objectToCall == null)
        {
            Debug.LogWarning("No object to call specified!");
            return;
        }
        
        Debug.Log("Calling object: " + objectToCall.name);
        
        // Enable the GameObject if it's disabled
        if (enableObject && !objectToCall.activeSelf)
        {
            objectToCall.SetActive(true);
            Debug.Log("Enabled object: " + objectToCall.name);
        }
        
        // Activate a specific component
        if (activateComponent && !string.IsNullOrEmpty(componentName))
        {
            Component component = objectToCall.GetComponent(componentName);
            if (component is Behaviour)
            {
                Behaviour behaviour = component as Behaviour;
                behaviour.enabled = true;
                Debug.Log("Activated component: " + componentName);
            }
            else if (component is Renderer)
            {
                Renderer renderer = component as Renderer;
                renderer.enabled = true;
                Debug.Log("Activated renderer: " + componentName);
            }
        }
        
        // Send a message to the object
        if (sendMessage && !string.IsNullOrEmpty(messageName))
        {
            objectToCall.SendMessage(messageName, SendMessageOptions.DontRequireReceiver);
            Debug.Log("Sent message: " + messageName + " to " + objectToCall.name);
        }
    }
    
    IEnumerator HideCanvasAfterDelay()
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(showDuration);
        
        // Hide or disable the canvas
        if (hideInsteadOfDisable && canvasToShow != null)
        {
            // Just hide the canvas but keep it enabled
            canvasToShow.enabled = false;
        }
        else if (destroyCanvasAfter && canvasToShow != null)
        {
            // Destroy the canvas GameObject
            Destroy(canvasToShow.gameObject);
        }
        else if (canvasToShow != null)
        {
            // Disable the canvas
            canvasToShow.enabled = false;
        }
        
        Debug.Log("Canvas hidden after " + showDuration + " seconds");
        
        // Destroy this script component
        Destroy(this);
    }
    
    [ContextMenu("Show Canvas Now")]
    void ShowCanvasNow()
    {
        if (canvasToShow != null)
        {
            canvasToShow.enabled = true;
            
            // Start the call another object timer
            if (callCoroutine != null)
                StopCoroutine(callCoroutine);
            
            callCoroutine = StartCoroutine(CallObjectAfterDelay());
            
            // Start the hide timer
            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);
            
            hideCoroutine = StartCoroutine(HideCanvasAfterDelay());
        }
    }
    
    [ContextMenu("Hide Canvas Now")]
    void HideCanvasNow()
    {
        if (canvasToShow != null)
        {
            canvasToShow.enabled = false;
            
            // Stop any running coroutines
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }
            
            if (callCoroutine != null)
            {
                StopCoroutine(callCoroutine);
                callCoroutine = null;
            }
        }
    }
    
    [ContextMenu("Call Object Now")]
    void CallObjectNow()
    {
        CallOtherObject();
    }
}