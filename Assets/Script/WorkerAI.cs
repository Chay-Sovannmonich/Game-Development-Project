using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorkerAI : MonoBehaviour
{
    [Header("Spawn & Positions")]
    public Vector3 spawnPosition = Vector3.zero;
    public Vector3 cashierPosition = Vector3.zero;
    public Vector3 kitchenPosition = new Vector3(5f, 0f, 5f);
    public GameObject kitchenPickupPoint;
    
    [Header("Worker Settings")]
    public float movementSpeed = 4f;
    public float rotationSpeed = 8f;
    public float interactionDistance = 1.5f;
    public bool showDebugInfo = true;
    
    [Header("Camera Switch Control")]
    public CameraSwitchButton cameraSwitchButton;
    public bool waitForCameraSwitch = true;
    public float spawnDelayAfterCameraSwitch = 1f;
    
    [Header("Customer Assignment")]
    public CharacterAI assignedCustomer;
    public float customerDetectionRadius = 10f;
    
    [Header("Food Settings")]
    public GameObject foodPrefab;
    public float foodCarryHeight = 1f;
    public float foodPreparationTime = 3f;
    
    [Header("Food Spawning")]
    public bool spawnFoodAtTable = true;
    public float foodSpawnDistance = 2.5f;
    public float foodSpawnHeight = 0.8f;
    
    [Header("Manual Food Position")]
    public bool useManualFoodPosition = false;
    public Vector3 manualFoodPosition = Vector3.zero;
    
    [Header("Emergency Food Pickup")]
    public GameObject emergencyFoodTarget;
    public float emergencyPickupSpeed = 6f;
    
    [Header("Animation")]
    public Animator workerAnimator;
    public string walkAnimationBool = "IsWalking";
    public string carryAnimationBool = "IsCarrying";
    public string idleAnimationTrigger = "Idle";
    public string deliverAnimationTrigger = "DeliverFood";
    
    [Header("State")]
    public WorkerState currentState = WorkerState.WaitingForCameraSwitch;
    private GameObject carriedFood = null;
    
    private Coroutine behaviorCoroutine;
    private bool isActive = false;
    private Vector3 targetPosition;
    private CharacterAI targetCustomer = null;
    
    private bool isEmergencyMode = false;
    private GameObject emergencyFood = null;
    
    public enum WorkerState
    {
        WaitingForCameraSwitch,
        Spawning,
        Idle,
        MovingToCashier,
        WaitingAtCashier,
        MovingToCustomer,
        InteractingWithCustomer,
        MovingToKitchen,
        PreparingFood,
        MovingToTable,
        DeliveringFood,
        ReturningToCashier,
        EmergencyPickup
    }
    
    // ✅ Subscribe in OnEnable
    void OnEnable()
    {
        Debug.Log($"{gameObject.name}: OnEnable called - Worker active: {gameObject.activeSelf}");
        Debug.Log($"{gameObject.name}: Worker position: {transform.position}");
        
        if (cameraSwitchButton != null)
        {
            Debug.Log($"{gameObject.name}: CameraSwitchButton assigned: {cameraSwitchButton.name}");
            cameraSwitchButton.onSwitchedToCamera2 += OnCameraSwitched;
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Subscribed to camera switch event");
        }
        else
        {
            Debug.LogError($"{gameObject.name}: CameraSwitchButton is NULL!");
        }
    }
    
    void OnDisable()
    {
        if (cameraSwitchButton != null)
        {
            cameraSwitchButton.onSwitchedToCamera2 -= OnCameraSwitched;
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Unsubscribed from camera switch event");
        }
    }
    
    void Start()
    {
        if (showDebugInfo) 
            Debug.Log($"{gameObject.name}: WorkerAI Start() called. Current state: {currentState}");
        
        transform.position = spawnPosition;
        Debug.Log($"{gameObject.name}: Set to spawn position: {spawnPosition}");
        
        if (waitForCameraSwitch)
        {
            if (cameraSwitchButton != null)
            {
                currentState = WorkerState.WaitingForCameraSwitch;
                
                // Check if camera is already switched
                if (cameraSwitchButton.IsCamera2Active())
                {
                    if (showDebugInfo)
                        Debug.Log($"{gameObject.name}: Camera already switched, spawning immediately");
                    
                    StartCoroutine(DelayedSpawnStart());
                }
                else
                {
                    if (showDebugInfo) 
                        Debug.Log($"{gameObject.name}: Waiting for camera switch before spawning...");
                }
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Wait for camera switch enabled but no CameraSwitchButton assigned!");
                currentState = WorkerState.Spawning;
                StartCoroutine(WorkerBehaviorLoop());
            }
        }
        else
        {
            currentState = WorkerState.Spawning;
            StartCoroutine(WorkerBehaviorLoop());
        }
    }
    
    // Camera switch event handler
    void OnCameraSwitched()
    {
        Debug.Log($"[Camera Event RECEIVED] Worker State = {currentState}");

        if (currentState == WorkerState.WaitingForCameraSwitch)
        {
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Camera switched! Starting spawn in {spawnDelayAfterCameraSwitch}s...");
            
            StartCoroutine(DelayedSpawnStart());
        }
        else
        {
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Camera switched but current state is {currentState}, not waiting for camera.");
        }
    }
    
    IEnumerator DelayedSpawnStart()
    {
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Waiting {spawnDelayAfterCameraSwitch}s before spawning...");
        
        yield return new WaitForSeconds(spawnDelayAfterCameraSwitch);
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Spawn delay complete. Starting worker behavior...");
        
        currentState = WorkerState.Spawning;
        StartCoroutine(WorkerBehaviorLoop());
    }
    
    IEnumerator WorkerBehaviorLoop()
    {
        isActive = true;
        
        if (showDebugInfo) 
            Debug.Log($"{gameObject.name}: Starting worker behavior loop...");
        
        while (isActive)
        {
            switch (currentState)
            {
                case WorkerState.Spawning:
                    yield return StartCoroutine(SpawnBehavior());
                    break;
                    
                case WorkerState.Idle:
                    yield return StartCoroutine(IdleBehavior());
                    break;
                    
                case WorkerState.MovingToCashier:
                    yield return StartCoroutine(MoveToPosition(cashierPosition, "Cashier Position", isEmergencyMode));
                    currentState = WorkerState.WaitingAtCashier;
                    break;
                    
                case WorkerState.WaitingAtCashier:
                    yield return StartCoroutine(WaitAtCashier());
                    break;
                    
                case WorkerState.MovingToCustomer:
                    if (targetCustomer != null)
                    {
                        Vector3 approachPos = targetCustomer.waitLocation + Vector3.right * 1f;
                        yield return StartCoroutine(MoveToPosition(approachPos, "Customer Approach", isEmergencyMode));
                        currentState = WorkerState.InteractingWithCustomer;
                    }
                    else
                    {
                        currentState = WorkerState.Idle;
                    }
                    break;
                    
                case WorkerState.InteractingWithCustomer:
                    yield return StartCoroutine(InteractWithCustomer());
                    break;
                    
                case WorkerState.MovingToKitchen:
                    yield return StartCoroutine(MoveToPosition(kitchenPosition, "Kitchen", isEmergencyMode));
                    currentState = WorkerState.PreparingFood;
                    break;
                    
                case WorkerState.PreparingFood:
                    yield return StartCoroutine(PrepareFood());
                    break;
                    
                case WorkerState.MovingToTable:
                    yield return StartCoroutine(MoveToTable());
                    break;
                    
                case WorkerState.DeliveringFood:
                    yield return StartCoroutine(DeliverFood());
                    break;
                    
                case WorkerState.ReturningToCashier:
                    yield return StartCoroutine(MoveToPosition(cashierPosition, "Cashier Position", isEmergencyMode));
                    currentState = WorkerState.WaitingAtCashier;
                    isEmergencyMode = false;
                    break;
                    
                case WorkerState.EmergencyPickup:
                    yield return StartCoroutine(EmergencyPickupBehavior());
                    break;
                    
                case WorkerState.WaitingForCameraSwitch:
                    if (showDebugInfo)
                        Debug.Log($"{gameObject.name}: Still waiting for camera switch...");
                    yield return new WaitForSeconds(0.5f);
                    break;
            }
            
            yield return null;
        }
    }
    
    IEnumerator SpawnBehavior()
    {
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Starting spawn behavior...");
        
        float distanceToCashier = Vector3.Distance(transform.position, cashierPosition);
        if (distanceToCashier < 0.1f)
        {
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Already at cashier position");
            currentState = WorkerState.WaitingAtCashier;
            yield break;
        }
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Moving from spawn to cashier...");
        
        yield return StartCoroutine(MoveToPosition(cashierPosition, "Cashier Position", false));
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Facing forward at cashier...");
        
        yield return StartCoroutine(TurnToDirection(Vector3.forward, "Cashier Forward"));
        
        currentState = WorkerState.WaitingAtCashier;
        
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Spawn complete, ready at cashier!");
    }
    
    IEnumerator IdleBehavior()
    {
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Idle - Looking for customers...");
        
        SetAnimation(false, false);
        
        float distanceToCashier = Vector3.Distance(transform.position, cashierPosition);
        if (distanceToCashier > 1f)
        {
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Not at cashier, moving to cashier...");
            currentState = WorkerState.MovingToCashier;
            yield break;
        }
        
        float idleTimer = 0f;
        while (currentState == WorkerState.Idle && idleTimer < 5f)
        {
            idleTimer += Time.deltaTime;
            
            CharacterAI[] allCustomers = FindObjectsOfType<CharacterAI>();
            
            foreach (CharacterAI customer in allCustomers)
            {
                float distanceToCustomer = Vector3.Distance(cashierPosition, customer.waitLocation);
                
                if (customer.currentState == CharacterAI.CharacterState.WaitingAtLocation && 
                    distanceToCustomer < customerDetectionRadius &&
                    !customer.isReadyForTable)
                {
                    targetCustomer = customer;
                    customer.assignedWorker = this;
                    currentState = WorkerState.MovingToCustomer;
                    
                    if (showDebugInfo) 
                        Debug.Log($"{gameObject.name}: Found customer at wait location, approaching...");
                    
                    yield break;
                }
            }
            
            yield return null;
        }
        
        if (currentState == WorkerState.Idle)
        {
            if (distanceToCashier > 1f)
            {
                currentState = WorkerState.MovingToCashier;
            }
            else
            {
                currentState = WorkerState.WaitingAtCashier;
            }
        }
    }
    
    IEnumerator WaitAtCashier()
    {
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Waiting at cashier...");
        SetAnimation(false, false);
        
        yield return StartCoroutine(TurnToDirection(Vector3.forward, "Cashier Forward"));
        
        float waitTimer = 0f;
        while (currentState == WorkerState.WaitingAtCashier && waitTimer < 2f)
        {
            waitTimer += Time.deltaTime;
            
            CharacterAI[] allCustomers = FindObjectsOfType<CharacterAI>();
            
            foreach (CharacterAI customer in allCustomers)
            {
                if (customer.currentState == CharacterAI.CharacterState.WaitingAtLocation && 
                    !customer.isReadyForTable)
                {
                    targetCustomer = customer;
                    customer.assignedWorker = this;
                    currentState = WorkerState.MovingToCustomer;
                    
                    if (showDebugInfo) 
                        Debug.Log($"{gameObject.name}: Customer detected while at cashier, approaching...");
                    
                    yield break;
                }
            }
            
            yield return null;
        }
        
        if (currentState == WorkerState.WaitingAtCashier)
        {
            currentState = WorkerState.Idle;
        }
    }
    
    IEnumerator InteractWithCustomer()
    {
        if (targetCustomer == null)
        {
            currentState = WorkerState.Idle;
            yield break;
        }
        
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Interacting with customer...");
        SetAnimation(false, false);
        
        Vector3 directionToCustomer = (targetCustomer.transform.position - transform.position).normalized;
        yield return StartCoroutine(TurnToDirection(directionToCustomer, "Customer"));
        
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Telling customer to go to table...");
        yield return new WaitForSeconds(1.5f);
        
        targetCustomer.NotifyTableReady();
        
        yield return new WaitForSeconds(0.5f);
        
        currentState = WorkerState.MovingToKitchen;
        
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Customer notified, going to kitchen...");
    }
    
    IEnumerator PrepareFood()
    {
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Preparing food in kitchen...");
        SetAnimation(false, false);
        
        yield return new WaitForSeconds(foodPreparationTime);
        
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Food prepared! Waiting for customer to get to table...");
        
        // Wait for customer to reach table
        float waitTimer = 0f;
        while (targetCustomer != null && 
               targetCustomer.currentState != CharacterAI.CharacterState.SittingAtChair &&
               targetCustomer.currentState != CharacterAI.CharacterState.WaitingForFood &&
               waitTimer < 30f)
        {
            waitTimer += Time.deltaTime;
            
            if (targetCustomer.currentState == CharacterAI.CharacterState.SittingAtChair ||
                targetCustomer.currentState == CharacterAI.CharacterState.WaitingForFood)
            {
                break;
            }
            
            yield return null;
        }
        
        if (targetCustomer != null)
        {
            // Don't carry food, just go to deliver it
            SetAnimation(false, false);
            currentState = WorkerState.MovingToTable;
        }
        else
        {
            currentState = WorkerState.ReturningToCashier;
        }
    }
    
    IEnumerator MoveToTable()
    {
        if (targetCustomer == null)
        {
            currentState = WorkerState.Idle;
            yield break;
        }
        
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Going to customer table...");
        
        // Determine which position to use for delivery
        Vector3 targetDeliveryPosition = useManualFoodPosition ? manualFoodPosition : targetCustomer.GetTableFoodPosition();
        
        // Approach position - adjust this based on your scene layout
        Vector3 deliveryPosition;
        
        if (useManualFoodPosition)
        {
            // For manual position, approach from a slight offset
            deliveryPosition = targetDeliveryPosition + Vector3.back * 1f;
        }
        else
        {
            // For table position, approach from behind the table
            deliveryPosition = targetDeliveryPosition + Vector3.back * 1f;
        }
        
        deliveryPosition.y = transform.position.y; // Keep same height
        
        yield return StartCoroutine(MoveToPosition(deliveryPosition, "Table Delivery Position", isEmergencyMode));
        
        // Face the delivery position
        Vector3 tableDirection = (targetDeliveryPosition - transform.position).normalized;
        yield return StartCoroutine(TurnToDirection(tableDirection, "Food Delivery Point"));
        
        currentState = WorkerState.DeliveringFood;
    }
    
    IEnumerator DeliverFood()
    {
        if (targetCustomer == null)
        {
            currentState = WorkerState.Idle;
            yield break;
        }
        
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Delivering food...");
        
        // Determine which position to use for food spawning
        Vector3 targetFoodPosition;
        
        if (useManualFoodPosition)
        {
            targetFoodPosition = manualFoodPosition;
            if (showDebugInfo) 
                Debug.Log($"{gameObject.name}: Using MANUAL food position: {targetFoodPosition}");
        }
        else
        {
            targetFoodPosition = targetCustomer.GetTableFoodPosition();
            if (showDebugInfo) 
                Debug.Log($"{gameObject.name}: Using customer's table position");
        }
        
        // Check if we're close enough to spawn food
        float distanceToFoodPoint = Vector3.Distance(transform.position, targetFoodPosition);
        
        if (distanceToFoodPoint <= foodSpawnDistance)
        {
            // Spawn food at the target position
            if (spawnFoodAtTable && foodPrefab != null)
            {
                // Adjust position with height offset
                Vector3 spawnPosition = targetFoodPosition + Vector3.up * foodSpawnHeight;
                GameObject food = Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
                
                // Optional: Add spawn effect
                StartCoroutine(FoodSpawnEffect(food));
                
                // Notify customer that food has arrived
                targetCustomer.FoodArrived(food);
                
                if (showDebugInfo) 
                    Debug.Log($"{gameObject.name}: Food spawned at position! Distance: {distanceToFoodPoint}, Position: {spawnPosition}");
            }
            else
            {
                if (showDebugInfo) Debug.LogWarning($"{gameObject.name}: No food prefab assigned or table spawning disabled!");
            }
        }
        else
        {
            if (showDebugInfo) 
                Debug.LogWarning($"{gameObject.name}: Too far from food position! Distance: {distanceToFoodPoint}");
            
            // Move closer to the food position
            Vector3 approachPos = targetFoodPosition + (transform.position - targetFoodPosition).normalized * foodSpawnDistance;
            approachPos.y = transform.position.y;
            
            yield return StartCoroutine(MoveToPosition(approachPos, "Food Approach", false));
            
            // Try spawning again
            if (spawnFoodAtTable && foodPrefab != null)
            {
                Vector3 spawnPosition = targetFoodPosition + Vector3.up * foodSpawnHeight;
                GameObject food = Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
                StartCoroutine(FoodSpawnEffect(food));
                targetCustomer.FoodArrived(food);
                
                if (showDebugInfo) 
                    Debug.Log($"{gameObject.name}: Food spawned after moving closer!");
            }
        }
        
        // Handle any carried food (for consistency)
        if (carriedFood != null)
        {
            Destroy(carriedFood);
            carriedFood = null;
        }
        
        // Play delivery animation
        if (workerAnimator != null && !string.IsNullOrEmpty(deliverAnimationTrigger))
        {
            workerAnimator.SetTrigger(deliverAnimationTrigger);
        }
        
        SetAnimation(false, false);
        
        yield return new WaitForSeconds(1f);
        
        currentState = WorkerState.ReturningToCashier;
    }
    
    IEnumerator FoodSpawnEffect(GameObject food)
    {
        // Simple spawn effect - scale from 0 to 1
        if (food != null)
        {
            Vector3 originalScale = food.transform.localScale;
            food.transform.localScale = Vector3.zero;
            
            float duration = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                food.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
                yield return null;
            }
            
            food.transform.localScale = originalScale;
        }
    }
    
    IEnumerator MoveToPosition(Vector3 targetPos, string destinationName, bool emergencyMode = false)
    {
        float speed = emergencyMode ? emergencyPickupSpeed : movementSpeed;
        
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Moving to {destinationName} at {targetPos} (Emergency: {emergencyMode})");
        SetAnimation(true, carriedFood != null);
        
        float distance = Vector3.Distance(transform.position, targetPos);
        
        while (distance > 0.1f)
        {
            distance = Vector3.Distance(transform.position, targetPos);
            Vector3 direction = (targetPos - transform.position).normalized;
            
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            if (carriedFood != null)
            {
                carriedFood.transform.position = transform.position + Vector3.up * foodCarryHeight;
            }
            
            yield return null;
        }
        
        transform.position = targetPos;
        SetAnimation(false, carriedFood != null);
        
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
        
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Finished turning to {description}");
    }
    
    public void EmergencyFoodPickup()
    {
        if (!gameObject.activeInHierarchy || !enabled)
        {
            Debug.LogWarning($"{gameObject.name}: Worker is not active. Cannot perform emergency pickup.");
            return;
        }
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: EMERGENCY FOOD PICKUP INITIATED!");
        
        isEmergencyMode = true;
        currentState = WorkerState.EmergencyPickup;
        FindNearestFood();
    }
    
    public void OnFoodSpawned(GameObject food)
    {
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Notified about spawned food: {food.name}");
        
        emergencyFood = food;
        
        if (isEmergencyMode && currentState == WorkerState.EmergencyPickup)
        {
            emergencyFoodTarget = food;
        }
    }
    
    IEnumerator EmergencyPickupBehavior()
    {
        if (showDebugInfo) 
            Debug.Log($"{gameObject.name}: Starting emergency pickup behavior...");
        
        if (emergencyFoodTarget != null || emergencyFood != null)
        {
            GameObject targetFood = emergencyFoodTarget != null ? emergencyFoodTarget : emergencyFood;
            
            if (targetFood != null)
            {
                Vector3 foodPosition = targetFood.transform.position;
                Vector3 approachPosition = foodPosition + Vector3.back * 1f;
                
                yield return StartCoroutine(MoveToPosition(approachPosition, "Emergency Food", true));
                yield return StartCoroutine(PickupFood(targetFood));
                yield return StartCoroutine(MoveToPosition(cashierPosition, "Cashier Position", true));
                
                if (carriedFood != null)
                {
                    Destroy(carriedFood);
                    carriedFood = null;
                }
                
                if (showDebugInfo)
                    Debug.Log($"{gameObject.name}: Emergency pickup complete!");
            }
        }
        else
        {
            if (showDebugInfo)
                Debug.LogWarning($"{gameObject.name}: No emergency food target specified.");
        }
        
        isEmergencyMode = false;
        emergencyFoodTarget = null;
        emergencyFood = null;
        currentState = WorkerState.WaitingAtCashier;
    }
    
    private void FindNearestFood()
    {
        string[] foodTags = { "Board", "RiceBall", "Extra", "ServedFood" };
        GameObject nearestFood = null;
        float nearestDistance = float.MaxValue;
        
        foreach (string tag in foodTags)
        {
            try
            {
                GameObject[] foodObjects = GameObject.FindGameObjectsWithTag(tag);
                foreach (GameObject food in foodObjects)
                {
                    float distance = Vector3.Distance(transform.position, food.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestFood = food;
                    }
                }
            }
            catch (UnityException)
            {
                continue;
            }
        }
        
        if (nearestFood != null)
        {
            emergencyFoodTarget = nearestFood;
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Found nearest food: {nearestFood.name} at distance {nearestDistance}");
        }
        else
        {
            if (showDebugInfo)
                Debug.LogWarning($"{gameObject.name}: No food found for emergency pickup!");
        }
    }
    
    IEnumerator PickupFood(GameObject food)
    {
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Picking up {food.name}");
        
        Vector3 foodDirection = (food.transform.position - transform.position).normalized;
        yield return StartCoroutine(TurnToDirection(foodDirection, "Food to Pickup"));
        
        SetAnimation(false, false);
        yield return new WaitForSeconds(0.5f);
        
        carriedFood = food;
        SetAnimation(false, true);
        
        if (carriedFood != null)
        {
            carriedFood.SetActive(false);
        }
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Picked up {food.name}");
    }
    
    void SetAnimation(bool isWalking, bool isCarrying)
    {
        if (workerAnimator != null)
        {
            if (!string.IsNullOrEmpty(walkAnimationBool))
                workerAnimator.SetBool(walkAnimationBool, isWalking);
            
            if (!string.IsNullOrEmpty(carryAnimationBool))
                workerAnimator.SetBool(carryAnimationBool, isCarrying);
            
            if (!isWalking && !isCarrying && !string.IsNullOrEmpty(idleAnimationTrigger))
                workerAnimator.SetTrigger(idleAnimationTrigger);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(spawnPosition, 0.3f);
        Gizmos.DrawWireSphere(spawnPosition, 0.5f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(cashierPosition, 0.3f);
        Gizmos.DrawWireSphere(cashierPosition, 0.5f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(kitchenPosition, 0.3f);
        
        Gizmos.color = new Color(0, 1, 1, 0.2f);
        Gizmos.DrawSphere(cashierPosition, customerDetectionRadius);
        
        if (isEmergencyMode)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
        
        // Draw manual food position if enabled
        if (useManualFoodPosition)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(manualFoodPosition, 0.3f);
            Gizmos.DrawWireSphere(manualFoodPosition, 0.5f);
            
            Gizmos.color = new Color(1, 0, 1, 0.3f);
            Gizmos.DrawWireSphere(manualFoodPosition, foodSpawnDistance);
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(manualFoodPosition + Vector3.up * foodSpawnHeight, 0.2f);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(manualFoodPosition + Vector3.up * 1f, 
                $"Manual Food Position\nX: {manualFoodPosition.x:F2}\nY: {manualFoodPosition.y:F2}\nZ: {manualFoodPosition.z:F2}");
            #endif
        }
        else if (spawnFoodAtTable && targetCustomer != null)
        {
            // Draw food spawn radius at customer table
            Vector3 tablePosition = targetCustomer.GetTableFoodPosition();
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(tablePosition, foodSpawnDistance);
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(tablePosition + Vector3.up * foodSpawnHeight, 0.2f);
        }
        
        Gizmos.color = Color.white;
        if (spawnPosition != Vector3.zero)
        {
            Gizmos.DrawLine(spawnPosition, cashierPosition);
        }
        
        if (targetCustomer != null)
        {
            Gizmos.DrawLine(cashierPosition, targetCustomer.waitLocation);
            Gizmos.DrawLine(targetCustomer.waitLocation, kitchenPosition);
            
            if (targetCustomer.chairObject != null)
            {
                Vector3 tablePos = useManualFoodPosition ? manualFoodPosition : targetCustomer.GetTableFoodPosition();
                Gizmos.DrawLine(kitchenPosition, tablePos);
                Gizmos.DrawLine(tablePos, cashierPosition);
            }
        }
        
        if (emergencyFoodTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, emergencyFoodTarget.transform.position);
            Gizmos.DrawWireSphere(emergencyFoodTarget.transform.position, 0.5f);
        }
        
        if (currentState == WorkerState.WaitingForCameraSwitch)
        {
            Gizmos.color = new Color(0.5f, 0f, 1f, 0.5f);
            Gizmos.DrawSphere(transform.position, 0.8f);
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, 
                $"Waiting for Camera Switch\nDelay: {spawnDelayAfterCameraSwitch}s");
            #endif
        }
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, 
            $"Worker: {currentState}\n" +
            $"Emergency: {isEmergencyMode}\n" +
            $"Customer: {(targetCustomer != null ? targetCustomer.name : "None")}\n" +
            $"Spawn Food at Table: {spawnFoodAtTable}\n" +
            $"Manual Position: {useManualFoodPosition}");
        
        if (spawnPosition != Vector3.zero)
            UnityEditor.Handles.Label(spawnPosition + Vector3.up * 0.5f, "Spawn");
        
        UnityEditor.Handles.Label(cashierPosition + Vector3.up * 0.5f, "Cashier");
        UnityEditor.Handles.Label(kitchenPosition + Vector3.up * 0.5f, "Kitchen");
        #endif
    }
    
    // New method to set manual food position
    [ContextMenu("Set Manual Food Position to Current")]
    public void SetManualFoodPositionToCurrent()
    {
        manualFoodPosition = transform.position;
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Manual food position set to current position: {manualFoodPosition}");
    }
    
    // New method to spawn food manually (for testing)
    [ContextMenu("Spawn Food Manually Now")]
    public void SpawnFoodManuallyNow()
    {
        if (foodPrefab != null)
        {
            Vector3 spawnPosition = useManualFoodPosition ? 
                manualFoodPosition + Vector3.up * foodSpawnHeight : 
                transform.position + Vector3.up * foodSpawnHeight;
            
            GameObject food = Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
            StartCoroutine(FoodSpawnEffect(food));
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Manually spawned food at: {spawnPosition}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No food prefab assigned!");
        }
    }
    
    public void AssignCustomer(CharacterAI customer)
    {
        if (customer != null && currentState == WorkerState.WaitingAtCashier)
        {
            targetCustomer = customer;
            customer.assignedWorker = this;
            currentState = WorkerState.MovingToCustomer;
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Manually assigned to customer {customer.name}");
        }
    }
    
    [ContextMenu("Force Return to Cashier")]
    public void ForceReturnToCashier()
    {
        if (carriedFood != null)
        {
            Destroy(carriedFood);
            carriedFood = null;
        }
        
        currentState = WorkerState.ReturningToCashier;
        isEmergencyMode = false;
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Forcing return to cashier");
    }
    
    [ContextMenu("Reset to Spawn")]
    public void ResetToSpawnMenu()
    {
        ResetToSpawn();
    }
    
    public void ResetToSpawn()
    {
        if (spawnPosition != Vector3.zero)
        {
            transform.position = spawnPosition;
            currentState = WorkerState.WaitingForCameraSwitch;
            
            if (carriedFood != null)
            {
                Destroy(carriedFood);
                carriedFood = null;
            }
            
            isEmergencyMode = false;
            emergencyFoodTarget = null;
            emergencyFood = null;
            targetCustomer = null;
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Reset to spawn position (waiting for camera switch)");
        }
    }
    
    [ContextMenu("Test Emergency Pickup")]
    public void TestEmergencyPickup()
    {
        EmergencyFoodPickup();
    }
    
    [ContextMenu("Force Start Worker")]
    public void ForceStartWorker()
    {
        if (currentState == WorkerState.WaitingForCameraSwitch)
        {
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Force starting worker (bypassing camera switch)");
            
            currentState = WorkerState.Spawning;
            if (!isActive)
            {
                StartCoroutine(WorkerBehaviorLoop());
            }
        }
    }
    
    void OnDestroy()
    {
        if (cameraSwitchButton != null)
        {
            cameraSwitchButton.onSwitchedToCamera2 -= OnCameraSwitched;
        }
    }
}