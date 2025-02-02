using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class ProceduralPlatformSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    public GameObject[] platformPrefabs; // Array of platform prefabs
    public int platformCount = 10; // Number of platforms to spawn
    public Vector2Int gridSize = new Vector2Int(40, 15); // Grid size (increase width)
    public float minVerticalDistance = 3f; // Minimum vertical gap
    public float minHorizontalDistance = 2f; // Minimum horizontal gap (edges only)
    public float reachableDistance = 8f; // Max distance for reachability check
    
    public GameObject[] enemyPrefabs; // Array of enemy prefabs
    public GameObject specialPrefab; // Prefab to spawn in center & top of platform
    public float enemySpawnChance = 0.5f; // 50% chance to spawn an enemy at each point

    private List<Bounds> spawnedPlatforms = new List<Bounds>(); // Store platform bounds

    void Start()
    {
    }
    
    private void Awake()
    {
        GenerateLevel();
        SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe in Awake
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    { 
        Debug.Log("Scene Loaded: " + scene.name);
        Debug.Log("Knight Instance: " + Knight.Instance);
        Debug.Log("Knight Instance.gameObejct: " + Knight.Instance.gameObject);
        if (Knight.Instance == null)
        {
            Debug.LogWarning("Knight.Instance is NULL! Creating a new instance...");
            SpawnKnight();
        }
        else if (Knight.Instance.gameObject == null)
        {
            Debug.Log("fuck");
            Debug.LogWarning("Knight GameObject is NULL! Respawning Knight...");
            SpawnKnight();
        }
    }
    
    public void SpawnKnight()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("playerPrefab is NOT assigned in the Inspector!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("spawnPoint is NOT assigned in the Inspector! Using default position.");
            spawnPoint = new GameObject("DefaultSpawn").transform;
            spawnPoint.position = Vector3.zero;
        }

        GameObject newKnight = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        newKnight.SetActive(true); // Ensure it's active

        if (Knight.Instance == null)
        {
            Debug.LogWarning("Knight.Instance is NULL! Manually assigning...");
            Knight.Instance = newKnight.GetComponent<Knight>(); // Assign manually
        }

        Knight.Instance.SetKnightGameObject(newKnight);
        Debug.Log("Knight successfully spawned.");
    }

    void GenerateLevel()
    {
        spawnedPlatforms.Clear();

        // Place the first platform at a fixed starting position
        Vector2 startPos = new Vector2(0, 0);
        SpawnPlatform(startPos, platformPrefabs[Random.Range(0, platformPrefabs.Length)]);

        // Spawn remaining platforms
        for (int i = 1; i < platformCount; i++)
        {
            bool placed = false;
            int attempts = 0;

            while (!placed && attempts < 500) // Retry if placement fails
            {
                Vector2 randomPos = new Vector2(
                    Random.Range(-gridSize.x / 2, gridSize.x / 2), // Spread horizontally
                    Random.Range(0, gridSize.y / 2) // Reduce excessive vertical stacking
                );

                GameObject selectedPrefab = platformPrefabs[Random.Range(0, platformPrefabs.Length)];

                if (CanPlacePlatform(randomPos, selectedPrefab) && IsPlatformReachable(randomPos, selectedPrefab))
                {
                    SpawnPlatform(randomPos, selectedPrefab);
                    placed = true;
                }

                attempts++;
            }
        }
        
        SpawnSpecialOnRandomPlatform();
    }
    
    void SpawnSpecialOnRandomPlatform()
    {
        if (spawnedPlatforms.Count == 0)
        {
            Debug.LogWarning("No platforms available to spawn the special object.");
            return;
        }

        // 🔹 Select a random platform
        Bounds selectedPlatform = spawnedPlatforms[Random.Range(0, spawnedPlatforms.Count)];

        // 🔹 Calculate positions
        Vector2 centerPosition = new Vector2(selectedPlatform.center.x, selectedPlatform.center.y);
        Vector2 topPosition = new Vector2(selectedPlatform.center.x, selectedPlatform.max.y + 1f); // Slightly above top

        // 🔹 Spawn the special prefab at the center and top
        // Instantiate(specialPrefab, centerPosition, Quaternion.identity);
        Instantiate(specialPrefab, topPosition, Quaternion.identity);
    }

    void SpawnPlatform(Vector2 position, GameObject prefab)
    {
        GameObject platform = Instantiate(prefab, position, Quaternion.identity);
        Tilemap tilemap = platform.GetComponentInChildren<Tilemap>();

        if (tilemap == null)
        {
            Debug.LogError("Tilemap component missing in platform prefab: " + prefab.name);
            return;
        }

        Bounds bounds = tilemap.localBounds;
        bounds.center = position;

        spawnedPlatforms.Add(bounds);
        
        SpawnEnemiesOnPlatform(platform);
    }
    
    void SpawnEnemiesOnPlatform(GameObject platform)
    {
        // Find all empty GameObjects marked as enemy spawn points
        Transform[] spawnPoints = platform.GetComponentsInChildren<Transform>();

        foreach (Transform spawnPoint in spawnPoints)
        {
            // Ignore the platform's main transform
            if (!spawnPoint.CompareTag("SpawnPoint")) 
                continue;

            // Random chance to spawn an enemy at this point
            if (Random.value < enemySpawnChance)
            {
                GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            }
        }
    }

    bool CanPlacePlatform(Vector2 position, GameObject prefab)
    {
        Tilemap tilemap = prefab.GetComponentInChildren<Tilemap>();

        if (tilemap == null)
            return false;

        Bounds newBounds = tilemap.localBounds;
        newBounds.center = position;

        foreach (Bounds existingBounds in spawnedPlatforms)
        {
            if (newBounds.Intersects(existingBounds)) return false; // Prevent direct overlap

            // Get edges of the new platform
            Vector2 newLeftEdge = new Vector2(newBounds.min.x, newBounds.center.y);
            Vector2 newRightEdge = new Vector2(newBounds.max.x, newBounds.center.y);
            Vector2 newTopEdge = new Vector2(newBounds.center.x, newBounds.max.y);
            Vector2 newBottomEdge = new Vector2(newBounds.center.x, newBounds.min.y);

            // Get edges of the existing platform
            Vector2 existingLeftEdge = new Vector2(existingBounds.min.x, existingBounds.center.y);
            Vector2 existingRightEdge = new Vector2(existingBounds.max.x, existingBounds.center.y);
            Vector2 existingTopEdge = new Vector2(existingBounds.center.x, existingBounds.max.y);
            Vector2 existingBottomEdge = new Vector2(existingBounds.center.x, existingBounds.min.y);

            // 🚨 Condition 1: Ensure platform edges are far enough apart using Euclidean distance
            float minEdgeDistance = 5f; // Adjust based on tile size
            bool edgesFarEnough =
                Vector2.Distance(newLeftEdge, existingRightEdge) >= minEdgeDistance &&
                Vector2.Distance(newRightEdge, existingLeftEdge) >= minEdgeDistance &&
                Vector2.Distance(newTopEdge, existingBottomEdge) >= minEdgeDistance &&
                Vector2.Distance(newBottomEdge, existingTopEdge) >= minEdgeDistance;

            // 🚨 Condition 2: Ensure platform edges are NOT inside another platform's width range UNLESS Y is far enough
            float minVerticalOffset = 5f; // Minimum vertical separation if within width
            bool isHorizontallyInside =
                newLeftEdge.x >= existingLeftEdge.x && newRightEdge.x <= existingRightEdge.x; // Completely inside width

            bool verticalOffsetValid = Mathf.Abs(newBottomEdge.y - existingTopEdge.y) >= minVerticalOffset ||
                                       Mathf.Abs(newTopEdge.y - existingBottomEdge.y) >= minVerticalOffset;

            // 🚨 Final validation: Both conditions must pass
            if (!edgesFarEnough || (isHorizontallyInside && !verticalOffsetValid))
            {
                return false; // Invalid placement
            }
        }

        return true; // Valid placement
    }





    bool IsPlatformReachable(Vector2 position, GameObject prefab)
    {
        Tilemap tilemap = prefab.GetComponentInChildren<Tilemap>();

        if (tilemap == null)
            return false;

        Bounds newBounds = tilemap.localBounds;
        newBounds.center = position;

        foreach (Bounds existingBounds in spawnedPlatforms)
        {
            // Get edges of new platform
            Vector2 newLeftEdge = new Vector2(newBounds.min.x, newBounds.center.y);
            Vector2 newRightEdge = new Vector2(newBounds.max.x, newBounds.center.y);
            Vector2 newTopEdge = new Vector2(newBounds.center.x, newBounds.max.y);
            Vector2 newBottomEdge = new Vector2(newBounds.center.x, newBounds.min.y);

            // Get edges of existing platform
            Vector2 existingLeftEdge = new Vector2(existingBounds.min.x, existingBounds.center.y);
            Vector2 existingRightEdge = new Vector2(existingBounds.max.x, existingBounds.center.y);
            Vector2 existingTopEdge = new Vector2(existingBounds.center.x, existingBounds.max.y);
            Vector2 existingBottomEdge = new Vector2(existingBounds.center.x, existingBounds.min.y);

            // ✅ Check if any edges are within `reachableDistance` using Euclidean distance
            if (Vector2.Distance(newLeftEdge, existingRightEdge) <= reachableDistance ||
                Vector2.Distance(newRightEdge, existingLeftEdge) <= reachableDistance ||
                Vector2.Distance(newTopEdge, existingBottomEdge) <= reachableDistance ||
                Vector2.Distance(newBottomEdge, existingTopEdge) <= reachableDistance)
            {
                return true; // Platform is reachable
            }
        }

        return false; // No reachable edges found
    }
}
