using UnityEngine;
using UnityEngine.UI;

public class CameraSwitchButton : MonoBehaviour
{
    [Header("=== MUST ASSIGN THESE ===")]
    public Camera mainCamera;      // ⬅️ DRAG MAIN CAMERA HERE
    public Camera camera2;         // ⬅️ DRAG CAMERA2 HERE
    public GameObject panelToClose; // ⬅️ DRAG PANEL HERE
    
    // ADD THIS EVENT FOR SPAWNING
    public System.Action onSwitchedToCamera2;
    
    private Button button;
    private bool isCamera2Active = false;
    
    void Start()
    {
        Debug.Log("=== CAMERA SWITCH BUTTON STARTING ===");
        
        // Get button
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("No button component found!");
            return;
        }
        
        // FORCE proper camera setup
        ForceCameraSetup();
        
        // Setup button listener
        button.onClick.AddListener(OnButtonClick);
        
        Debug.Log("Camera switch button ready.");
    }
    
    void ForceCameraSetup()
    {
        Debug.Log("=== FORCING CAMERA SETUP ===");
        
        // FIND cameras if not assigned
        if (mainCamera == null)
        {
            Debug.Log("Searching for Main Camera...");
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Camera[] allCams = FindObjectsOfType<Camera>(true);
                foreach (Camera cam in allCams)
                {
                    if (cam.gameObject.activeSelf)
                    {
                        mainCamera = cam;
                        break;
                    }
                }
            }
        }
        
        if (camera2 == null)
        {
            Debug.Log("Searching for Camera2...");
            camera2 = GameObject.Find("Camera2")?.GetComponent<Camera>();
        }
        
        // FORCE Main Camera ACTIVE at start
        if (mainCamera != null)
        {
            if (!mainCamera.gameObject.activeSelf)
            {
                mainCamera.gameObject.SetActive(true);
                Debug.Log($"✓ FORCED Main Camera ACTIVE: {mainCamera.name}");
            }
            else
            {
                Debug.Log($"✓ Main Camera already active: {mainCamera.name}");
            }
        }
        else
        {
            Debug.LogError("❌ MAIN CAMERA NOT FOUND!");
        }
        
        // FORCE Camera2 INACTIVE at start
        if (camera2 != null)
        {
            if (camera2.gameObject.activeSelf)
            {
                camera2.gameObject.SetActive(false);
                Debug.Log($"✓ FORCED Camera2 INACTIVE: {camera2.name}");
            }
            else
            {
                Debug.Log($"✓ Camera2 already inactive: {camera2.name}");
            }
        }
        else
        {
            Debug.LogWarning("⚠ Camera2 not found");
        }
        
        // Set initial state - FIXED: Should be false at start
        isCamera2Active = false; // Camera2 is inactive at start
        
        DebugCameraState();
    }
    
    void OnButtonClick()
    {
        Debug.Log("=== BUTTON CLICKED ===");
        
        // Close panel if assigned
        if (panelToClose != null)
        {
            panelToClose.SetActive(false);
            Debug.Log($"Closed panel: {panelToClose.name}");
        }
        
        // SWITCH CAMERAS
        if (isCamera2Active)
        {
            SwitchToMainCamera();
        }
        else
        {
            SwitchToCamera2();
        }
    }
    
    void SwitchToCamera2()
    {
        Debug.Log("SWITCHING TO CAMERA2...");
        
        // CRITICAL: Check if cameras exist
        if (camera2 == null)
        {
            Debug.LogError("Cannot switch: Camera2 is null!");
            return;
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("Cannot switch: Main Camera is null!");
            return;
        }
        
        // STEP 1: Activate Camera2 FIRST
        camera2.gameObject.SetActive(true);
        Debug.Log($"✓ ACTIVATED Camera2: {camera2.name}");
        
        // STEP 2: Deactivate Main Camera AFTER Camera2 is active
        mainCamera.gameObject.SetActive(false);
        Debug.Log($"✓ DEACTIVATED Main Camera: {mainCamera.name}");
        
        // Update state - FIXED: Must be true when Camera2 is active
        isCamera2Active = true;
        
        // TRIGGER SPAWN EVENT - FIXED: Invoke AFTER setting the state
        Debug.Log("✓ Triggered spawn event for worker!");
        onSwitchedToCamera2?.Invoke();
        
        DebugCameraState();
    }
    
    void SwitchToMainCamera()
    {
        Debug.Log("SWITCHING BACK TO MAIN CAMERA...");
        
        // CRITICAL: Check if cameras exist
        if (mainCamera == null)
        {
            Debug.LogError("Cannot switch: Main Camera is null!");
            return;
        }
        
        // STEP 1: Activate Main Camera FIRST
        mainCamera.gameObject.SetActive(true);
        Debug.Log($"✓ ACTIVATED Main Camera: {mainCamera.name}");
        
        // STEP 2: Deactivate Camera2 AFTER Main Camera is active
        if (camera2 != null && camera2.gameObject.activeSelf)
        {
            camera2.gameObject.SetActive(false);
            Debug.Log($"✓ DEACTIVATED Camera2: {camera2.name}");
        }
        
        // Update state - FIXED: Must be false when Main Camera is active
        isCamera2Active = false;
        
        DebugCameraState();
    }
    
    void DebugCameraState()
    {
        Debug.Log("=== CURRENT CAMERA STATE ===");
        
        Camera[] allCameras = FindObjectsOfType<Camera>(true);
        int activeCount = 0;
        
        foreach (Camera cam in allCameras)
        {
            bool isActive = cam.gameObject.activeSelf;
            if (isActive) activeCount++;
            Debug.Log($"{cam.name}: {(isActive ? "✅ ACTIVE" : "❌ INACTIVE")}");
        }
        
        Debug.Log($"Total: {allCameras.Length} cameras, {activeCount} active");
        Debug.Log($"isCamera2Active state: {isCamera2Active}");
        
        // EMERGENCY: If no cameras are active, fix it!
        if (activeCount == 0)
        {
            Debug.LogError("❌❌❌ NO ACTIVE CAMERAS! EMERGENCY FIX...");
            EmergencyActivateCamera();
        }
    }
    
    void EmergencyActivateCamera()
    {
        Debug.Log("=== EMERGENCY CAMERA ACTIVATION ===");
        
        // Try Main Camera first
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
            isCamera2Active = false;
            Debug.Log($"EMERGENCY: Activated Main Camera");
            return;
        }
        
        // Try Camera2
        if (camera2 != null)
        {
            camera2.gameObject.SetActive(true);
            isCamera2Active = true;
            Debug.Log($"EMERGENCY: Activated Camera2");
            return;
        }
        
        // Find ANY camera
        Camera[] allCams = FindObjectsOfType<Camera>(true);
        if (allCams.Length > 0)
        {
            allCams[0].gameObject.SetActive(true);
            Debug.Log($"EMERGENCY: Activated {allCams[0].name}");
        }
        else
        {
            Debug.LogError("NO CAMERAS EXIST IN SCENE!");
        }
    }
    
    void Update()
    {
        // Debug hotkeys
        if (Input.GetKeyDown(KeyCode.F1))
        {
            DebugCameraState();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            // Force switch to Camera2
            if (camera2 != null)
            {
                if (!camera2.gameObject.activeSelf)
                {
                    SwitchToCamera2();
                }
            }
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            // Force switch to Main Camera
            if (mainCamera != null)
            {
                if (!mainCamera.gameObject.activeSelf)
                {
                    SwitchToMainCamera();
                }
            }
        }
    }
    
    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
    
    // ADD THIS METHOD - Required by WorkerAI
    public bool IsCamera2Active()
    {
        return isCamera2Active;
    }
}