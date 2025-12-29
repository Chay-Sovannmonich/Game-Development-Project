using UnityEngine;
using System.Collections;

public class CarSpawner : MonoBehaviour
{
    [Header("Car Prefab")]
    public GameObject carPrefab;

    [Header("Spawn & Target Positions")]
    public Vector3 spawnPosition = new Vector3(54.89f, 23.40632f, 23.15332f);
    public Vector3 targetPosition = new Vector3(-17.72f, 23.40632f, 23.15332f);

    [Header("Car Settings")]
    public float carSpeed = 5f;
    
    [Header("Spawn Settings")]
    public bool spawnOnStart = false;
    public float spawnDelay = 0f; // Delay before first spawn
    public bool spawnRepeatedly = false; // If true, keeps spawning
    public float repeatInterval = 5f; // Time between spawns if repeating
    public int maxCarsAtOnce = 1; // Maximum cars this spawner can have active
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private int currentCars = 0;
    private Coroutine spawnCoroutine;

    void Start()
    {
        if (spawnOnStart)
        {
            StartSpawning();
        }
    }
    
    // Start spawning cars
    public void StartSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }
    
    // Stop spawning cars
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    // Spawning routine with delay
    IEnumerator SpawnRoutine()
    {
        // Initial delay - use unscaled time to work even when menu is open
        if (spawnDelay > 0)
        {
            if (showDebugInfo) Debug.Log($"{gameObject.name}: Waiting {spawnDelay} seconds before first spawn...");
            yield return new WaitForSecondsRealtime(spawnDelay); // Use realtime for delays
        }
        
        do
        {
            // Check if we can spawn more cars
            if (currentCars < maxCarsAtOnce)
            {
                SpawnCar();
                
                // If not repeating, break after one spawn
                if (!spawnRepeatedly)
                {
                    yield break;
                }
            }
            else
            {
                if (showDebugInfo) Debug.Log($"{gameObject.name}: Max cars ({maxCarsAtOnce}) reached, waiting...");
            }
            
            // Wait for the next spawn interval - use realtime for delays
            yield return new WaitForSecondsRealtime(repeatInterval);
            
        } while (spawnRepeatedly);
    }
    
    // Spawns one car
    public void SpawnCar()
    {
        if (carPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: No car prefab assigned!");
            return;
        }

        if (currentCars >= maxCarsAtOnce)
        {
            if (showDebugInfo) Debug.Log($"{gameObject.name}: Cannot spawn - already at max cars ({maxCarsAtOnce})");
            return;
        }

        GameObject car = Instantiate(carPrefab, spawnPosition, Quaternion.identity);
        
        // Ensure the car is active
        car.SetActive(true);
        
        // Add mover script dynamically and enable it immediately
        CarMover mover = car.AddComponent<CarMover>();
        mover.targetPosition = targetPosition;
        mover.speed = carSpeed;
        mover.spawner = this; // Reference back to this spawner
        mover.enabled = true;
        
        // Register the car with TimeManager if it exists
        if (TimeManager.Instance != null && car.TryGetComponent<CarTimeHandler>(out var timeHandler))
        {
            TimeManager.Instance.RegisterPausable(timeHandler);
        }
        
        // Optional: Name the car for easier debugging
        car.name = $"{gameObject.name}_Car_{currentCars + 1}";
        
        currentCars++;
        
        if (showDebugInfo) 
        {
            Debug.Log($"{gameObject.name}: Spawned {car.name} at {spawnPosition}, moving to {targetPosition} at speed {carSpeed}");
            Debug.Log($"{gameObject.name}: Current cars: {currentCars}/{maxCarsAtOnce}");
        }
    }
    
    // Called by CarMover when a car is destroyed
    public void OnCarDestroyed(GameObject car)
    {
        currentCars--;
        currentCars = Mathf.Max(0, currentCars); // Ensure not negative
        
        // Unregister from TimeManager if needed
        if (TimeManager.Instance != null && car.TryGetComponent<CarTimeHandler>(out var timeHandler))
        {
            TimeManager.Instance.UnregisterPausable(timeHandler);
        }
        
        if (showDebugInfo) 
        {
            Debug.Log($"{gameObject.name}: Car destroyed. Current cars: {currentCars}/{maxCarsAtOnce}");
        }
        
        // If we're in repeat mode and below max, trigger a spawn
        if (spawnRepeatedly && currentCars < maxCarsAtOnce && spawnCoroutine == null)
        {
            StartSpawning();
        }
    }
    
    // Manually trigger a single car spawn (with delay if needed)
    public void SpawnCarWithDelay(float delay = 0f)
    {
        StartCoroutine(SpawnSingleWithDelay(delay));
    }
    
    IEnumerator SpawnSingleWithDelay(float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSecondsRealtime(delay); // Use realtime for delays
        }
        
        SpawnCar();
    }
    
    // Reset and clear all cars from this spawner
    public void ResetSpawner()
    {
        StopSpawning();
        currentCars = 0;
        
        // Find and destroy all cars from this spawner
        CarMover[] allMovers = FindObjectsOfType<CarMover>();
        foreach (CarMover mover in allMovers)
        {
            if (mover.spawner == this)
            {
                Destroy(mover.gameObject);
            }
        }
        
        if (showDebugInfo) Debug.Log($"{gameObject.name}: Spawner reset");
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw spawn point
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(spawnPosition, 0.5f);
        Gizmos.DrawWireSphere(spawnPosition, 0.7f);
        
        // Draw target point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, 0.5f);
        Gizmos.DrawWireSphere(targetPosition, 0.7f);
        
        // Draw path line
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(spawnPosition, targetPosition);
        
        // Draw label
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(spawnPosition + Vector3.up * 2, $"{gameObject.name}\nCars: {currentCars}/{maxCarsAtOnce}");
        #endif
    }
}

public class CarMover : MonoBehaviour
{
    public Vector3 targetPosition;
    public float speed = 5f;
    public CarSpawner spawner; // Reference to the spawner that created this car
    
    [Header("Rotation Settings")]
    public bool rotateToFaceTarget = true;
    public float rotationSpeed = 5f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    private Vector3 initialPosition;
    
    // Time handler component
    private CarTimeHandler timeHandler;

    void Start()
    {
        initialPosition = transform.position;
        
        // Add and setup time handler
        timeHandler = gameObject.AddComponent<CarTimeHandler>();
        
        if (showDebugInfo)
        {
            Debug.Log($"[CarMover] Started on {gameObject.name}");
            Debug.Log($"[CarMover] Initial position: {initialPosition}");
            Debug.Log($"[CarMover] Target position: {targetPosition}");
            Debug.Log($"[CarMover] Speed: {speed}");
            Debug.Log($"[CarMover] Distance to target: {Vector3.Distance(transform.position, targetPosition)}");
        }
        
        // Ensure we have a valid target
        if (targetPosition == transform.position)
        {
            Debug.LogWarning($"[CarMover] {gameObject.name}: Target position is same as spawn position!");
        }
    }

    void Update()
    {
        // Use TimeManager for delta time if available
        float deltaTime = TimeManager.Instance != null 
            ? TimeManager.Instance.GetDeltaTime(true) // true = is gameplay object
            : Time.deltaTime;
        
        // Calculate movement
        Vector3 moveDirection = targetPosition - transform.position;
        float distanceToTarget = moveDirection.magnitude;
        
        // If very close to target, destroy the car
        if (distanceToTarget < 0.1f)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[CarMover] {gameObject.name} reached target. Destroying.");
            }
            
            // Notify spawner before destroying
            if (spawner != null)
            {
                spawner.OnCarDestroyed(gameObject);
            }
            
            Destroy(gameObject);
            return;
        }
        
        // Normalize direction for movement
        Vector3 direction = moveDirection.normalized;
        
        // Move the car using appropriate delta time
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            speed * deltaTime
        );
        
        // Rotate to face movement direction (optional)
        if (rotateToFaceTarget && moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * deltaTime
            );
        }
        
        // Debug visualization in editor
        if (showDebugInfo)
        {
            Debug.DrawLine(transform.position, targetPosition, Color.red);
        }
    }
    
    void OnDestroy()
    {
        // Safety: Notify spawner if we're destroyed unexpectedly
        if (spawner != null && !Application.isPlaying)
        {
            spawner.OnCarDestroyed(gameObject);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            // Draw spawn and target positions in editor
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetPosition, 0.5f);
            Gizmos.DrawWireSphere(targetPosition, 0.7f);
            
            // Draw path line
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}