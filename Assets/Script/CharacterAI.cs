using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; // Added for UI components

public class CharacterAI : MonoBehaviour
{
    [Header("Character Settings")]
    public float movementSpeed = 3.5f;
    public float rotationSpeed = 8f;
    public bool showDebugInfo = true;
    
    [Header("Behavior Points")]
    public Vector3 spawnPosition = new Vector3(0f, 0f, 0f);
    public Vector3 waitLocation = new Vector3(10f, 0f, 0f);
    public GameObject chairObject;
    public Vector3 beforeExitPosition = new Vector3(25f, 0f, 5f);
    public Vector3 exitPosition = new Vector3(30f, 0f, 0f);
    
    [Header("Chair Approach Settings")]
    public float approachDistance = -1f;
    public ChairSide chairApproachSide = ChairSide.Right;
    public float sittingOffset = 0f;
    
    [Header("Grid/Tile Settings")]
    public float gridSize = 1f;
    public bool useGridMovement = true;
    
    [Header("Wait Times")]
    public float waitAtLocationTime = 5f;
    public float waitAtChairTime = 60f; // CHANGED: Now 60 seconds (1 minute)
    public float waitBeforeExitTime = 2f;
    
    [Header("Camera Switch Settings")]
    public CameraSwitchButton cameraSwitchButton;
    public float delayAfterCameraSwitch = 5f;
    public bool waitForCameraSwitch = true;
    
    [Header("Food Settings")]
    public float maxWaitForFoodTime = 60f;
    public float eatingTime = 60f;
    private float timeWaitingForFood = 0f;
    private bool hasFoodArrived = false;
    
    [Header("Animation Parameters")]
    public Animator characterAnimator;
    public string walkAnimationBool = "IsWalking";
    public string sitAnimationBool = "IsSitting";
    public string eatAnimationBool = "IsEating";
    public string idleAnimationTrigger = "Idle";
    
    [Header("Satisfaction Settings")]
    public GameObject satisfactionImage;
    public float satisfactionDisplayTime = 2f;
    private Coroutine satisfactionCoroutine;
    
    [Header("Additional Image Position")]
    public GameObject additionalImage;
    public Vector3 additionalImageLocalOffset = new Vector3(0, 0.5f, 0);
    public float additionalImageDisplayTime = 3f;
    public ImageDisplayMode additionalImageDisplayMode = ImageDisplayMode.AfterEating;
    
    [Header("Object Spawning")]
    public GameObject objectToSpawn;
    public Vector3 spawnObjectLocalOffset = new Vector3(0, 1f, 0);
    public float spawnObjectScale = 1f;
    public bool destroySpawnedObjectOnExit = true;
    private GameObject spawnedObjectInstance;
    
    [Header("Money Button Object Settings")] // NEW SECTION
    public GameObject moneyButtonPrefab; // Prefab with MoneyButtonObject script
    public Vector3 moneyButtonOffset = new Vector3(0, 2f, 0); // Spawn position offset
    public Transform customMoneySpawnPosition; // Optional custom spawn position
    public float moneyButtonScale = 0.5f; // Scale of the spawned object
    public bool destroyMoneyButtonOnExit = true; // Whether to destroy when character exits
    private GameObject spawnedMoneyButton;
    
    [Header("State")]
    public CharacterState currentState = CharacterState.Idle;

    [Header("Before Exit Auto Calculation")]
    public int beforeExitTiles = 3;

    [Header("Worker Interaction")]
    public WorkerAI assignedWorker;
    public bool isReadyForTable = false;

    [Header("Timer Settings")]
    public GameObject timerPrefab;
    private SimpleCustomerTimer customerTimer;
    
    private Coroutine behaviorCoroutine;
    private bool isActive = false;
    private Vector3 targetPosition;
    private List<Vector3> currentPath = new List<Vector3>();
    private Vector3 lookAtDirection = Vector3.forward;
    private Vector3 actualChairPosition;
    private Vector3 approachPosition;
    private Vector3 sideApproachPosition;
    private bool shouldAutoDetectApproachSide = true;
    private Vector3 tableFacingDirection;
    private List<Vector3> beforeExitSteps = new List<Vector3>();
    private GameObject foodOnTable = null;
    
    // Camera switch tracking
    private bool cameraSwitched = false;
    private bool waitingForCameraSwitch = false;
    
    // Image display tracking
    private bool showingSatisfactionImage = false;
    private bool showingAdditionalImage = false;
    
    public enum CharacterState
    {
        Idle,
        WaitingForCameraSwitch,
        MovingToLocation,
        WaitingAtLocation,
        MovingToApproachPoint,
        TurningToApproachSide,
        MovingToChairSide,
        TurningToFaceChair,
        MovingToChair,
        TurningToFaceTable,
        SittingAtChair,
        WaitingForFood,
        Eating,
        ShowingSatisfaction,
        ShowingAdditionalImage,
        SpawningObject,
        MovingToBeforeExit,
        WaitingBeforeExit,
        MovingToExit,
        Exited
    }
    
    public enum ChairSide
    {
        Right,
        Left,
        AutoDetect
    }
    
    // When to show additional image
    public enum ImageDisplayMode
    {
        Never,
        AfterEating,
        OnSpawn,
        OnSit,
        BeforeExit
    }
    
    void Start()
    {
        transform.position = spawnPosition;
        CalculateChairPosition();
        
        // Setup camera switch listener
        SetupCameraSwitchListener();
        
        // Setup images if not assigned
        if (satisfactionImage == null)
        {
            CreateDefaultSatisfactionImage();
        }
        else
        {
            // Hide the image initially
            satisfactionImage.SetActive(false);
        }
        
        // Setup additional image
        if (additionalImage != null)
        {
            additionalImage.SetActive(false);
        }
        
        // Show additional image on spawn if configured
        if (additionalImageDisplayMode == ImageDisplayMode.OnSpawn && additionalImage != null)
        {
            StartCoroutine(ShowAdditionalImageForTime(additionalImageDisplayTime));
        }
        
        // Modified: Don't start behavior immediately if waiting for camera switch
        if (waitForCameraSwitch && cameraSwitchButton != null)
        {
            currentState = CharacterState.WaitingForCameraSwitch;
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Waiting for camera switch before starting behavior...");
        }
        else
        {
            StartBehavior();
        }
    }
    
    // Create a default satisfaction image if none is assigned
    void CreateDefaultSatisfactionImage()
    {
        // Create a canvas on the character
        GameObject canvasObj = new GameObject("SatisfactionCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = new Vector3(0, 2.5f, 0);
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one * 0.01f;
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create image
        GameObject imageObj = new GameObject("SatisfactionImage");
        imageObj.transform.SetParent(canvasObj.transform);
        imageObj.transform.localPosition = Vector3.zero;
        imageObj.transform.localScale = Vector3.one * 50f;
        imageObj.transform.localRotation = Quaternion.identity;
        
        Image image = imageObj.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.9f);
        
        // Create a simple smiley texture
        Texture2D smileyTexture = CreateSmileyTexture(64, 64);
        if (smileyTexture != null)
        {
            Sprite smileySprite = Sprite.Create(smileyTexture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
            image.sprite = smileySprite;
        }
        
        satisfactionImage = imageObj;
        satisfactionImage.SetActive(false);
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Created default satisfaction image");
    }
    
    // Helper method to create a simple smiley texture
    Texture2D CreateSmileyTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        // Fill with transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        // Draw a yellow circle
        Vector2 center = new Vector2(width / 2, height / 2);
        float radius = width / 2 - 2;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius)
                {
                    pixels[y * width + x] = Color.yellow;
                }
                
                // Draw eyes
                if ((x >= width/2 - 10 && x <= width/2 - 5 && y >= height/2 + 5 && y <= height/2 + 10) ||
                    (x >= width/2 + 5 && x <= width/2 + 10 && y >= height/2 + 5 && y <= height/2 + 10))
                {
                    pixels[y * width + x] = Color.black;
                }
                
                // Draw smile (arc)
                if (distance >= radius - 10 && distance <= radius - 5)
                {
                    if (y < height/2 && x > width/2 - 15 && x < width/2 + 15)
                    {
                        pixels[y * width + x] = Color.black;
                    }
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    // Setup camera switch event listener
    void SetupCameraSwitchListener()
    {
        if (cameraSwitchButton != null)
        {
            cameraSwitchButton.onSwitchedToCamera2 += OnCameraSwitched;
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Registered for camera switch events");
        }
        else if (waitForCameraSwitch)
        {
            Debug.LogWarning($"{gameObject.name}: Wait for camera switch is enabled but no CameraSwitchButton assigned!");
        }
    }
    
    // Camera switch event handler
    void OnCameraSwitched()
    {
        if (waitingForCameraSwitch || !cameraSwitched)
        {
            cameraSwitched = true;
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Camera switch detected! Waiting {delayAfterCameraSwitch} seconds before starting...");
            
            // Start the delayed behavior
            StartCoroutine(DelayedBehaviorStart());
        }
    }
    
    // Delayed behavior start after camera switch
    IEnumerator DelayedBehaviorStart()
    {
        waitingForCameraSwitch = true;
        
        // Wait for the specified delay
        yield return new WaitForSeconds(delayAfterCameraSwitch);
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: {delayAfterCameraSwitch} seconds passed after camera switch. Starting behavior...");
        
        waitingForCameraSwitch = false;
        StartBehavior();
    }
    
    private void SpawnMoneyButton()
    {
        if (moneyButtonPrefab != null)
        {
            // Calculate spawn position
            Vector3 spawnPosition;
            Quaternion spawnRotation;
            
            if (customMoneySpawnPosition != null)
            {
                // Use custom spawn position
                spawnPosition = customMoneySpawnPosition.position;
                spawnRotation = customMoneySpawnPosition.rotation;
            }
            else
            {
                // Spawn above character with offset
                spawnPosition = transform.position + moneyButtonOffset;
                spawnRotation = Quaternion.identity;
            }
            
            // Spawn the object
            spawnedMoneyButton = Instantiate(moneyButtonPrefab, spawnPosition, spawnRotation);
            
            // Set scale
            spawnedMoneyButton.transform.localScale = Vector3.one * moneyButtonScale;
            
            // Make it face the camera (for better visibility)
            if (Camera.main != null)
            {
                spawnedMoneyButton.transform.LookAt(Camera.main.transform);
                spawnedMoneyButton.transform.rotation = Quaternion.Euler(0, spawnedMoneyButton.transform.rotation.eulerAngles.y + 180, 0);
            }
            
            // Configure the MoneyButtonObject component
            MoneyButtonObject moneyButtonObj = spawnedMoneyButton.GetComponent<MoneyButtonObject>();
            if (moneyButtonObj != null)
            {
                // Pass the spawn position
                moneyButtonObj.SetSpawnPosition(spawnPosition, spawnRotation);
                
                // NEW: The object will handle its own visibility with startHidden = true
                // No need to call ShowObject() as it will show automatically after delay
            }
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Spawned money button object at position: {spawnPosition}. It will appear after customer finishes eating.");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No money button prefab assigned!");
        }
    }
        
    // Show satisfaction image method
    private void ShowSatisfactionImage()
    {
        if (satisfactionImage != null)
        {
            satisfactionImage.SetActive(true);
            showingSatisfactionImage = true;
            currentState = CharacterState.ShowingSatisfaction;
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Showing satisfaction image");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No satisfaction image assigned!");
        }
    }
    
    // Hide satisfaction image method
    private void HideSatisfactionImage()
    {
        if (satisfactionImage != null)
        {
            satisfactionImage.SetActive(false);
            showingSatisfactionImage = false;
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Hiding satisfaction image");
        }
    }
    
    // Show additional image method
    private void ShowAdditionalImage()
    {
        if (additionalImage != null)
        {
            // Apply local offset
            additionalImage.transform.localPosition = additionalImageLocalOffset;
            additionalImage.SetActive(true);
            showingAdditionalImage = true;
            currentState = CharacterState.ShowingAdditionalImage;
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Showing additional image at position: {additionalImage.transform.position}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No additional image assigned!");
        }
    }
    
    // Hide additional image method
    private void HideAdditionalImage()
    {
        if (additionalImage != null)
        {
            additionalImage.SetActive(false);
            showingAdditionalImage = false;
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Hiding additional image");
        }
    }
    
    // Show additional image for specific time
    private IEnumerator ShowAdditionalImageForTime(float displayTime)
    {
        ShowAdditionalImage();
        yield return new WaitForSeconds(displayTime);
        HideAdditionalImage();
    }
    
    // Spawn object method
    private void SpawnObject()
    {
        if (objectToSpawn != null)
        {
            // Calculate spawn position (world space)
            Vector3 spawnPosition = transform.position + 
                                   (transform.right * spawnObjectLocalOffset.x) +
                                   (transform.up * spawnObjectLocalOffset.y) +
                                   (transform.forward * spawnObjectLocalOffset.z);
            
            // Spawn the object
            spawnedObjectInstance = Instantiate(objectToSpawn, spawnPosition, transform.rotation);
            
            // Set scale
            spawnedObjectInstance.transform.localScale = Vector3.one * spawnObjectScale;
            
            currentState = CharacterState.SpawningObject;
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Spawned object at position: {spawnPosition}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No object assigned to spawn!");
        }
    }
    
    // Destroy spawned object method
    private void DestroySpawnedObject()
    {
        if (spawnedObjectInstance != null)
        {
            Destroy(spawnedObjectInstance);
            spawnedObjectInstance = null;
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Destroyed spawned object");
        }
    }
    
    void CalculateChairPosition()
    {
        if (chairObject == null)
        {
            Debug.LogWarning("No chair assigned!");
            return;
        }

        Vector3 chairPos = chairObject.transform.position;
        Vector3 chairForward = chairObject.transform.forward;
        Vector3 chairRight = chairObject.transform.right;

        float stopDistance = 2f;
        actualChairPosition = chairPos + (-chairForward * ((stopDistance / 2f) - 1f));
        tableFacingDirection = chairForward;

        if (shouldAutoDetectApproachSide || chairApproachSide == ChairSide.AutoDetect)
        {
            Vector3 toChair = (chairPos - transform.position).normalized;
            float dot = Vector3.Dot(chairRight, toChair);
            chairApproachSide = (dot > 0) ? ChairSide.Right : ChairSide.Left;
        }

        float sideOffset = 2f;
        float frontOffset = 0f;

        if (chairApproachSide == ChairSide.Right)
            sideApproachPosition = chairPos - (chairRight * sideOffset);
        else
            sideApproachPosition = chairPos + (chairRight * sideOffset);

        approachPosition = sideApproachPosition + (-chairForward * frontOffset);

        float exitSideOffset = 2.5f;
        float exitForwardOffset = 1f;

        Vector3 firstSideStep;
        Vector3 secondSideStep;

        if (chairApproachSide == ChairSide.Right)
        {
            firstSideStep = -chairRight * exitSideOffset;
            secondSideStep = -chairRight * exitSideOffset;
        }
        else
        {
            firstSideStep = -chairRight * exitSideOffset;
            secondSideStep = chairRight * exitSideOffset;
        }

        Vector3 beforeExitStep1 = actualChairPosition + firstSideStep;
        Vector3 beforeExitStep2 = beforeExitStep1 + secondSideStep;
        Vector3 beforeExitStep3 = beforeExitStep2 - chairForward * exitForwardOffset;

        beforeExitPosition = beforeExitStep3;
        beforeExitSteps = new List<Vector3>() { beforeExitStep1, beforeExitStep2, beforeExitStep3 };

        if (showDebugInfo)
        {
            Debug.Log("Before Exit Steps:");
            for (int i = 0; i < beforeExitSteps.Count; i++)
                Debug.Log($"Step {i + 1}: {beforeExitSteps[i]}");
        }
    }

    public void StartBehavior()
    {
        if (isActive) return;
        
        CalculateChairPosition();
        isActive = true;
        currentState = CharacterState.MovingToLocation;
        
        if (behaviorCoroutine != null)
            StopCoroutine(behaviorCoroutine);
        
        behaviorCoroutine = StartCoroutine(BehaviorSequence());
        
        if (showDebugInfo) 
            Debug.Log($"{gameObject.name}: Starting behavior sequence");
    }

    public void NotifyTableReady()
    {
        if (currentState == CharacterState.WaitingAtLocation)
        {
            isReadyForTable = true;
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Notified table is ready, proceeding to table...");
        }
    }
    
    private void CleanupTimer()
    {
        if (customerTimer != null && customerTimer.gameObject != null)
        {
            Destroy(customerTimer.gameObject);
            customerTimer = null;
        }
    }
    
    public void FoodArrived(GameObject food)
    {
        hasFoodArrived = true;
        foodOnTable = food;
        
        if (customerTimer != null)
            customerTimer.StartEating();
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Food has arrived!");
    }

    public bool HasFoodNearby()
    {
        float checkRadius = 2f;
        
        string[] foodTags = { "RiceBall", "Board", "Extra", "ServedFood" };
        
        foreach (string tag in foodTags)
        {
            try
            {
                GameObject[] foodObjects = GameObject.FindGameObjectsWithTag(tag);
                foreach (GameObject food in foodObjects)
                {
                    float distance = Vector3.Distance(GetTableFoodPosition(), food.transform.position);
                    if (distance < checkRadius)
                    {
                        return true;
                    }
                }
            }
            catch (UnityException)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"{gameObject.name}: Tag '{tag}' is not defined in project settings");
                }
                continue;
            }
        }
        
        GameObject[] allNearbyObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allNearbyObjects)
        {
            if (obj.name.Contains("Rice") || obj.name.Contains("Board") || 
                obj.name.Contains("Food") || obj.name.Contains("Meal"))
            {
                float distance = Vector3.Distance(GetTableFoodPosition(), obj.transform.position);
                if (distance < checkRadius)
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    // Get the position where food should be placed on the table
    public Vector3 GetTableFoodPosition()
    {
        if (chairObject != null)
        {
            Vector3 chairForward = chairObject.transform.forward;
            Vector3 chairPosition = chairObject.transform.position;
            
            return chairPosition + (chairForward * 0.5f);
        }
        
        return transform.position + Vector3.forward * 0.5f;
    }

    IEnumerator BehaviorSequence()
    {
        // Show additional image on sit if configured
        if (additionalImageDisplayMode == ImageDisplayMode.OnSit)
        {
            while (currentState != CharacterState.SittingAtChair)
            {
                yield return null;
            }
            
            yield return StartCoroutine(ShowAdditionalImageForTime(additionalImageDisplayTime));
        }
        
        // Step 1: Move to wait location
        yield return StartCoroutine(MoveToPosition(waitLocation, "Wait Location"));
        
        // Step 2: Wait at location for worker notification
        currentState = CharacterState.WaitingAtLocation;
        SetAnimation(false, false);
        
        if (showDebugInfo) 
            Debug.Log($"{gameObject.name}: Waiting at location for worker notification...");
        
        float waitTimer = 0f;
        while (!isReadyForTable && waitTimer < waitAtLocationTime)
        {
            waitTimer += Time.deltaTime;
            yield return null;
        }
        
        // Step 3: Move to chair with proper approach
        yield return StartCoroutine(ApproachChairFromBehind());
        
        // Step 4: Wait at chair for 1 minute (60 seconds) before timer starts
        currentState = CharacterState.SittingAtChair;
        SetAnimation(false, true);
        
        if (showDebugInfo) 
            Debug.Log($"{gameObject.name}: Sitting at chair, waiting for 60 seconds before timer starts...");
        
        // Wait at chair for 60 seconds before checking for food
        float waitAtChairTimer = 0f;
        while (waitAtChairTimer < waitAtChairTime && !hasFoodArrived)
        {
            waitAtChairTimer += Time.deltaTime;
            yield return null;
        }
        
        // If food arrived during the waiting period, skip to eating
        if (hasFoodArrived)
        {
            currentState = CharacterState.Eating;
            SetAnimation(false, false, true);
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Food arrived during waiting period! Eating for {eatingTime}s...");
            
            float eatingTimer = 0f;
            while (eatingTimer < eatingTime)
            {
                eatingTimer += Time.deltaTime;
                yield return null;
            }
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Finished eating!");
            
            // Show satisfaction image for 2 seconds
            ShowSatisfactionImage();
            yield return new WaitForSeconds(satisfactionDisplayTime);
            HideSatisfactionImage();
            
            // Show additional image after eating if configured
            if (additionalImageDisplayMode == ImageDisplayMode.AfterEating)
            {
                yield return StartCoroutine(ShowAdditionalImageForTime(additionalImageDisplayTime));
            }
            
            // NEW: Spawn money button object after eating
            SpawnMoneyButton();
            
            // Spawn regular object after eating
            SpawnObject();
            
            if (foodOnTable != null)
                Destroy(foodOnTable);
            
            // Skip to exit sequence
            goto ExitSequence;
        }
        
        // Step 5: After 1 minute, spawn timer and wait for food
        currentState = CharacterState.WaitingForFood;
        SetAnimation(false, true);
        
        if (showDebugInfo) 
            Debug.Log($"{gameObject.name}: 1 minute passed, now waiting for food (max {maxWaitForFoodTime}s)...");
        
        // Create timer after 1 minute wait
        if (timerPrefab != null)
        {
            GameObject timerObj = Instantiate(timerPrefab);
            customerTimer = timerObj.GetComponent<SimpleCustomerTimer>();
            
            if (customerTimer != null)
            {
                customerTimer.Initialize(this, maxWaitForFoodTime, eatingTime);
                Debug.Log($"{gameObject.name}: Timer started after 1 minute wait");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Timer prefab doesn't have SimpleCustomerTimer component!");
            }
        }
        
        // Wait for food to arrive (additional wait time)
        timeWaitingForFood = 0f;
        while (timeWaitingForFood < maxWaitForFoodTime && !hasFoodArrived)
        {
            timeWaitingForFood += Time.deltaTime;
            yield return null;
        }
        
        if (hasFoodArrived)
        {
            // Food arrived! Start eating
            currentState = CharacterState.Eating;
            SetAnimation(false, false, true);
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Food arrived! Eating for {eatingTime}s...");
            
            float eatingTimer = 0f;
            while (eatingTimer < eatingTime)
            {
                eatingTimer += Time.deltaTime;
                yield return null;
            }
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Finished eating!");
            
            // Show satisfaction image for 2 seconds
            ShowSatisfactionImage();
            yield return new WaitForSeconds(satisfactionDisplayTime);
            HideSatisfactionImage();
            
            // Show additional image after eating if configured
            if (additionalImageDisplayMode == ImageDisplayMode.AfterEating)
            {
                yield return StartCoroutine(ShowAdditionalImageForTime(additionalImageDisplayTime));
            }
            
            // NEW: Spawn money button object after eating
            SpawnMoneyButton();
            
            // Spawn regular object after eating
            SpawnObject();
            
            if (foodOnTable != null)
                Destroy(foodOnTable);
        }
        else
        {
            // No food arrived
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: No food arrived within {maxWaitForFoodTime}s after 1 minute wait, leaving...");
        }
        
        ExitSequence:
        // Show additional image before exit if configured
        if (additionalImageDisplayMode == ImageDisplayMode.BeforeExit)
        {
            yield return StartCoroutine(ShowAdditionalImageForTime(additionalImageDisplayTime));
        }
        
        // Step 6: Move to before exit position
        currentState = CharacterState.MovingToBeforeExit;
        SetAnimation(true, false, false);

        foreach (Vector3 step in beforeExitSteps)
        {
            Vector3 turnDir = (step - transform.position).normalized;
            yield return StartCoroutine(TurnToDirection(turnDir, "Turn Before Exit Step"));
            yield return StartCoroutine(MoveToPosition(step, "Before Exit Step"));
        }

        // Step 7: Wait at before exit position
        currentState = CharacterState.WaitingBeforeExit;
        SetAnimation(false, false, false);
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Waiting before exit for {waitBeforeExitTime} seconds");
        yield return new WaitForSeconds(waitBeforeExitTime);
        
        // Step 8: Move to exit
        currentState = CharacterState.MovingToExit;
        SetAnimation(true, false, false);
        yield return StartCoroutine(MoveToPosition(exitPosition, "Exit Position"));
        
        // Step 9: Clean up and exit
        CleanupTimer();
        
        // Destroy spawned object if configured
        if (destroySpawnedObjectOnExit && spawnedObjectInstance != null)
        {
            DestroySpawnedObject();
        }
        
        // NEW: Destroy money button object if configured
        if (destroyMoneyButtonOnExit && spawnedMoneyButton != null)
        {
            Destroy(spawnedMoneyButton);
        }
        
        currentState = CharacterState.Exited;
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Behavior complete - exiting");
        Destroy(gameObject);
    }
    
    IEnumerator ApproachChairFromBehind()
    {
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Starting chair approach from BEHIND ({chairApproachSide} side)");
        
        currentState = CharacterState.MovingToApproachPoint;
        yield return StartCoroutine(MoveToGridPosition(approachPosition, "Approach Point (Behind)"));
        
        currentState = CharacterState.TurningToApproachSide;
        SetAnimation(false, false, false);
        Vector3 sideDirection = (sideApproachPosition - transform.position).normalized;
        yield return StartCoroutine(TurnToDirection(sideDirection, "Face Side Position"));
        
        currentState = CharacterState.MovingToChairSide;
        yield return StartCoroutine(MoveToGridPosition(sideApproachPosition, "Chair Side"));
        
        currentState = CharacterState.TurningToFaceChair;
        SetAnimation(false, false, false);
        Vector3 chairDirection = (actualChairPosition - transform.position).normalized;
        yield return StartCoroutine(TurnToDirection(chairDirection, "Face Chair"));
        
        currentState = CharacterState.MovingToChair;
        yield return StartCoroutine(MoveToGridPosition(actualChairPosition, "Sitting Position"));
        
        currentState = CharacterState.TurningToFaceTable;
        SetAnimation(false, false, false);
        yield return StartCoroutine(TurnToDirection(tableFacingDirection, "Face Table (90°)"));
        
        currentState = CharacterState.SittingAtChair;
        SetAnimation(false, true, false);
        
        if (showDebugInfo) 
            Debug.Log($"{gameObject.name}: Finished approach from behind");
    }
    
    List<Vector3> GeneratePath(Vector3 start, Vector3 end)
    {
        List<Vector3> path = new List<Vector3>();
        path.Add(start);
        
        Vector3 intermediatePoint1 = new Vector3(end.x, start.y, start.z);
        if (Vector3.Distance(start, intermediatePoint1) > 0.1f)
            path.Add(intermediatePoint1);
        
        Vector3 intermediatePoint2 = new Vector3(end.x, start.y, end.z);
        if (path.Count > 0 && Vector3.Distance(path[path.Count - 1], intermediatePoint2) > 0.1f)
            path.Add(intermediatePoint2);
        
        if (path.Count > 0 && Vector3.Distance(path[path.Count - 1], end) > 0.1f)
            path.Add(end);
        
        return path;
    }
    
    IEnumerator MoveToGridPosition(Vector3 targetPos, string destinationName)
    {
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Moving to {destinationName} at {targetPos}");
        SetAnimation(true, false, false);
        
        List<Vector3> path = GeneratePath(transform.position, targetPos);
        
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 currentTarget = path[i];
            float distance = Vector3.Distance(transform.position, currentTarget);
            
            while (distance > 0.05f)
            {
                distance = Vector3.Distance(transform.position, currentTarget);
                Vector3 moveDirection = (currentTarget - transform.position).normalized;
                
                float xDiff = Mathf.Abs(currentTarget.x - transform.position.x);
                float zDiff = Mathf.Abs(currentTarget.z - transform.position.z);
                
                if (xDiff > zDiff)
                    moveDirection = new Vector3(Mathf.Sign(currentTarget.x - transform.position.x), 0, 0);
                else
                    moveDirection = new Vector3(0, 0, Mathf.Sign(currentTarget.z - transform.position.z));
                
                lookAtDirection = moveDirection;
                transform.position = Vector3.MoveTowards(transform.position, currentTarget, movementSpeed * Time.deltaTime);
                
                if (lookAtDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookAtDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
                
                yield return null;
            }
            
            transform.position = currentTarget;
        }
        
        SetAnimation(false, false, false);
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Arrived at {destinationName}");
    }
    
    IEnumerator TurnToDirection(Vector3 targetDirection, string description)
    {
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Turning to {description}");
        
        if (targetDirection == Vector3.zero) yield break;
        
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        float turnProgress = 0f;
        
        while (turnProgress < 1f)
        {
            turnProgress += rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, turnProgress);
            yield return null;
        }
        
        transform.rotation = targetRotation;
        lookAtDirection = targetDirection;
        
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Finished turning to {description}");
    }
    
    IEnumerator MoveToPosition(Vector3 targetPos, string destinationName)
    {
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Moving to {destinationName} at {targetPos}");
        SetAnimation(true, false, false);
        
        float distance = Vector3.Distance(transform.position, targetPos);
        
        while (distance > 0.1f)
        {
            distance = Vector3.Distance(transform.position, targetPos);
            Vector3 direction = (targetPos - transform.position).normalized;
            lookAtDirection = direction;
            
            transform.position = Vector3.MoveTowards(transform.position, targetPos, movementSpeed * Time.deltaTime);
            
            if (lookAtDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookAtDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            yield return null;
        }
        
        transform.position = targetPos;
        SetAnimation(false, false, false);
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Arrived at {destinationName}");
    }
        
    void SetAnimation(bool isWalking, bool isSitting, bool isEating = false)
    {
        if (characterAnimator != null)
        {
            if (!string.IsNullOrEmpty(walkAnimationBool))
                characterAnimator.SetBool(walkAnimationBool, isWalking);
            
            if (!string.IsNullOrEmpty(sitAnimationBool))
                characterAnimator.SetBool(sitAnimationBool, isSitting && !isEating);
            
            if (!string.IsNullOrEmpty(eatAnimationBool))
                characterAnimator.SetBool(eatAnimationBool, isEating);
            
            if (!isWalking && !isSitting && !isEating && !string.IsNullOrEmpty(idleAnimationTrigger))
                characterAnimator.SetTrigger(idleAnimationTrigger);
        }
    }
    
    void OnDestroy()
    {
        // Clean up event subscription
        if (cameraSwitchButton != null)
        {
            cameraSwitchButton.onSwitchedToCamera2 -= OnCameraSwitched;
        }
        
        // Stop satisfaction coroutine if running
        if (satisfactionCoroutine != null)
        {
            StopCoroutine(satisfactionCoroutine);
        }
        
        // Hide images when destroyed
        HideSatisfactionImage();
        HideAdditionalImage();
        
        // Destroy spawned objects
        if (spawnedObjectInstance != null)
        {
            Destroy(spawnedObjectInstance);
        }
        
        // Destroy money button object
        if (spawnedMoneyButton != null)
        {
            Destroy(spawnedMoneyButton);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Calculate positions for gizmo drawing
        Vector3 chairPos = chairObject != null ? chairObject.transform.position : Vector3.zero;
        Vector3 approachPos = approachPosition;
        Vector3 sidePos = sideApproachPosition;
        Vector3 finalPos = actualChairPosition;
        Vector3 tableDir = tableFacingDirection;
        
        if (chairObject != null)
        {
            chairPos = chairObject.transform.position;
            Vector3 chairForward = chairObject.transform.forward;
            
            finalPos = chairPos + (chairForward * sittingOffset);
            tableDir = -chairForward;
            
            Vector3 chairRight = chairObject.transform.right;
            
            if (chairApproachSide == ChairSide.Right || (shouldAutoDetectApproachSide && !Application.isPlaying))
            {
                sidePos = finalPos + (chairRight * -gridSize);
                approachPos = sidePos + (chairForward * gridSize);
            }
            else
            {
                sidePos = finalPos + (-chairRight * -gridSize);
                approachPos = sidePos + (chairForward * -gridSize);
            }
        }
        
        // Draw behavior points
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(spawnPosition, 0.3f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(waitLocation, 0.3f);
        
        // Draw chair and directions
        if (chairObject != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(chairPos, new Vector3(0.8f, 1f, 0.8f));
            
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(chairPos, chairObject.transform.forward * 2f);
            
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawCube(approachPos, new Vector3(0.5f, 0.1f, 0.5f));
            
            Gizmos.color = Color.green;
            Gizmos.DrawRay(finalPos, tableDir * 1.5f);
        }
        
        // Draw approach points
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(approachPos, 0.4f);
        Gizmos.DrawWireSphere(approachPos, 0.6f);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(sidePos, 0.3f);
        
        // Draw sitting position
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(finalPos, 0.3f);
        Gizmos.DrawWireSphere(finalPos, 0.5f);
        
        // Draw exit points
        Gizmos.color = Color.gray;
        Gizmos.DrawSphere(beforeExitPosition, 0.3f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(exitPosition, 0.3f);
        
        // NEW: Draw money button spawn position
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f); // Orange
        Vector3 moneySpawnPos;
        if (customMoneySpawnPosition != null)
        {
            moneySpawnPos = customMoneySpawnPosition.position;
        }
        else
        {
            moneySpawnPos = transform.position + moneyButtonOffset;
        }
        Gizmos.DrawSphere(moneySpawnPos, 0.3f);
        Gizmos.DrawWireSphere(moneySpawnPos, 0.4f);
        
        // Draw additional image position
        if (additionalImage != null)
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
            Vector3 additionalImagePos = transform.position + additionalImageLocalOffset;
            Gizmos.DrawSphere(additionalImagePos, 0.25f);
            Gizmos.DrawWireSphere(additionalImagePos, 0.35f);
        }
        
        // Draw object spawn position
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Vector3 spawnObjectPos = transform.position + 
                                 (transform.right * spawnObjectLocalOffset.x) +
                                 (transform.up * spawnObjectLocalOffset.y) +
                                 (transform.forward * spawnObjectLocalOffset.z);
        Gizmos.DrawCube(spawnObjectPos, new Vector3(0.3f, 0.3f, 0.3f));
        Gizmos.DrawWireCube(spawnObjectPos, new Vector3(0.4f, 0.4f, 0.4f));
        
        // Draw approach path
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(waitLocation, approachPos);
        Gizmos.DrawLine(approachPos, sidePos);
        Gizmos.DrawLine(sidePos, finalPos);
        Gizmos.DrawLine(finalPos, beforeExitPosition);
        Gizmos.DrawLine(beforeExitPosition, exitPosition);
        
        // Draw arrows showing direction
        DrawArrow(approachPos, sidePos, Color.yellow);
        DrawArrow(sidePos, finalPos, Color.yellow);
        
        // Draw labels
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(approachPos + Vector3.up * 0.3f, "BACK\nApproach");
        UnityEditor.Handles.Label(sidePos + Vector3.up * 0.3f, $"SIDE\n{chairApproachSide}");
        UnityEditor.Handles.Label(finalPos + Vector3.up * 0.3f, "SIT");
        
        UnityEditor.Handles.Label(spawnPosition + Vector3.up * 0.5f, "Spawn");
        UnityEditor.Handles.Label(waitLocation + Vector3.up * 0.5f, $"Wait\n{waitAtLocationTime}s");
        
        if (chairObject != null)
        {
            UnityEditor.Handles.Label(chairPos + Vector3.up * 1.5f, "CHAIR");
            UnityEditor.Handles.Label(chairPos + chairObject.transform.forward * 0.5f, "Table Direction");
        }
        
        UnityEditor.Handles.Label(beforeExitPosition + Vector3.up * 0.5f, $"Before Exit\n{waitBeforeExitTime}s");
        UnityEditor.Handles.Label(exitPosition + Vector3.up * 0.5f, "Exit");
        
        // NEW: Draw money button spawn info
        UnityEditor.Handles.Label(moneySpawnPos + Vector3.up * 0.3f, 
            $"Money Button Spawn\n{(customMoneySpawnPosition != null ? "Custom Position" : "Above Character")}");
        
        // Draw camera switch info
        if (waitForCameraSwitch)
        {
            Gizmos.color = new Color(0.5f, 0f, 1f, 0.5f);
            Gizmos.DrawSphere(transform.position, 0.5f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, 
                $"Waiting for Camera Switch\n+ {delayAfterCameraSwitch}s delay");
        }
        
        // Draw satisfaction image position
        if (satisfactionImage != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            Gizmos.DrawSphere(satisfactionImage.transform.position, 0.2f);
            UnityEditor.Handles.Label(satisfactionImage.transform.position + Vector3.up * 0.3f, 
                $"Satisfaction Image\n{gameObject.name}");
        }
        
        // Draw additional image info
        if (additionalImage != null)
        {
            Vector3 additionalImagePos = transform.position + additionalImageLocalOffset;
            UnityEditor.Handles.Label(additionalImagePos + Vector3.up * 0.3f, 
                $"Additional Image\nMode: {additionalImageDisplayMode}\nTime: {additionalImageDisplayTime}s");
        }
        
        // Draw object spawn info
        UnityEditor.Handles.Label(spawnObjectPos + Vector3.up * 0.3f, 
            $"Object Spawn\nOffset: {spawnObjectLocalOffset}\nScale: {spawnObjectScale}\nDestroy on Exit: {destroySpawnedObjectOnExit}");
        #endif
    }
    
    void DrawArrow(Vector3 start, Vector3 end, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(start, end);
        
        // Arrow head
        Vector3 direction = (end - start).normalized;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 45, 0) * Vector3.forward * 0.3f;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 45, 0) * Vector3.forward * 0.3f;
        
        Gizmos.DrawLine(end, end + right);
        Gizmos.DrawLine(end, end + left);
    }
    
    // Debug method to manually trigger behavior
    [ContextMenu("Force Start Behavior")]
    public void ForceStartBehavior()
    {
        if (!isActive)
        {
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Force starting behavior (bypassing camera switch)");
            StartBehavior();
        }
    }
    
    // Test method to show/hide satisfaction image
    [ContextMenu("Test Show Satisfaction")]
    public void TestShowSatisfaction()
    {
        StartCoroutine(TestSatisfactionRoutine());
    }
    
    IEnumerator TestSatisfactionRoutine()
    {
        ShowSatisfactionImage();
        yield return new WaitForSeconds(2f);
        HideSatisfactionImage();
    }
    
    // Test method to show additional image
    [ContextMenu("Test Show Additional Image")]
    public void TestShowAdditionalImage()
    {
        StartCoroutine(TestAdditionalImageRoutine());
    }
    
    IEnumerator TestAdditionalImageRoutine()
    {
        ShowAdditionalImage();
        yield return new WaitForSeconds(2f);
        HideAdditionalImage();
    }
    
    // Test method to spawn object
    [ContextMenu("Test Spawn Object")]
    public void TestSpawnObject()
    {
        SpawnObject();
    }
    
    // NEW: Test method to spawn money button
    [ContextMenu("Test Spawn Money Button")]
    public void TestSpawnMoneyButton()
    {
        SpawnMoneyButton();
    }
    
    // Test method to destroy spawned object
    [ContextMenu("Test Destroy Spawned Object")]
    public void TestDestroySpawnedObject()
    {
        DestroySpawnedObject();
    }
    
    // NEW: Test method to destroy money button
    [ContextMenu("Test Destroy Money Button")]
    public void TestDestroyMoneyButton()
    {
        if (spawnedMoneyButton != null)
        {
            Destroy(spawnedMoneyButton);
            spawnedMoneyButton = null;
            Debug.Log("Money button destroyed.");
        }
    }
}