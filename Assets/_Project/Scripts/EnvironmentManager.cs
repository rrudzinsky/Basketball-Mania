using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [Header("Current World Data")]
    public WorldProfile currentWorld;

    [Header("Scene References")]
    [Tooltip("Drag your Court_Floor object here so we can scale it.")]
    public Transform floorTransform; 
    public MeshRenderer floorRenderer;
    public ArenaBoundaryGenerator boundaryGenerator;

    [Header("Hoop Spawning")]
    [Tooltip("Drag your Hoop_Earth prefab from your project folder here!")]
    public GameObject standardHoopPrefab;
    
    // Hardcoded regulation distance from the baseline to the backboard (1.2 meters / ~4 feet).
    private const float BASELINE_OFFSET = 1.2f; 
    
    // Hidden variables to keep track of the spawned hoops
    private GameObject spawnedNorthHoop;
    private GameObject spawnedSouthHoop;

    // Run instantly when the game starts
    void Awake()
    {
        LoadWorld();
    }

    [ContextMenu("Load World Environment")]
    public void LoadWorld()
    {
        if (currentWorld == null)
        {
            Debug.LogWarning("No World Profile assigned!");
            return;
        }

        // 1. Update Floor Visuals & Dimensions
        if (floorRenderer != null) 
            floorRenderer.material = currentWorld.courtFloorAppearance; 
        
        if (floorTransform != null)
        {
            floorTransform.localScale = new Vector3(currentWorld.courtWidth, 1f, currentWorld.courtLength);
            
            // Apply the Floor Physics
            Collider floorCol = floorTransform.GetComponent<Collider>();
            if (floorCol != null && currentWorld.courtFloorPhysics != null)
            {
                floorCol.material = currentWorld.courtFloorPhysics;
            }
        }

        // 2. Update Boundaries & Size
        if (boundaryGenerator != null)
        {
            boundaryGenerator.wallMaterial = currentWorld.boundaryAppearance; 
            
            boundaryGenerator.regulationWidth = currentWorld.courtWidth;
            boundaryGenerator.regulationLength = currentWorld.courtLength;
            
            boundaryGenerator.GenerateBoundaries();
        }

        // 3. Spawn the Hoops
        SpawnHoops();

        // 4. Update the Physics Engine
        Physics.gravity = new Vector3(0, -currentWorld.ballGravity, 0);

        Debug.Log($"[Space League] Welcome to {currentWorld.worldName}! Court resized to {currentWorld.courtWidth}x{currentWorld.courtLength}.");
    }
    private void SpawnHoops()
    {
        // Safely destroy old hoops depending on if we are testing in the Editor or playing the game
        if (spawnedNorthHoop != null)
        {
            if (Application.isPlaying) Destroy(spawnedNorthHoop);
            else DestroyImmediate(spawnedNorthHoop);
        }

        if (spawnedSouthHoop != null)
        {
            if (Application.isPlaying) Destroy(spawnedSouthHoop);
            else DestroyImmediate(spawnedSouthHoop);
        }

        // Decide which blueprint to use: The Custom Alien one, or the Standard one?
        GameObject blueprintToSpawn = currentWorld.customHoopPrefab != null ? currentWorld.customHoopPrefab : standardHoopPrefab;

        if (blueprintToSpawn != null)
        {
            // We use the exact physical center of the floor object in World Space
            Vector3 courtCenter = floorTransform.position;
            
            // We calculate exactly half the physical length of the court
            float halfCourtLength = currentWorld.courtLength / 2f;
            
            // We calculate the exact world-space position for each baseline
            // Note: If your court spawns from 0 to -28, courtCenter is likely (0, 0, -14)
            Vector3 northBaselinePos = courtCenter + new Vector3(0, 0, halfCourtLength);
            Vector3 southBaselinePos = courtCenter + new Vector3(0, 0, -halfCourtLength);

            // Apply the 1.2m overhang inside the baselines
            Vector3 northHoopPos = northBaselinePos + new Vector3(0, 0, -1.2f);
            Vector3 southHoopPos = southBaselinePos + new Vector3(0, 0, 1.2f);

            // SPAWN: We REMOVE the parent argument. This spawns them at the root level of the scene.
            // This guarantees they use pure world coordinates and ignore any weird court scaling.
            spawnedNorthHoop = Instantiate(blueprintToSpawn, northHoopPos, Quaternion.Euler(0, 180, 0));
            spawnedNorthHoop.name = "Hoop_North";

            // spawnedSouthHoop = Instantiate(blueprintToSpawn, southHoopPos, Quaternion.Euler(0, 0, 0));
            // spawnedSouthHoop.name = "Hoop_South";

            // If we used the standard hoop (not a custom override), tell the HoopController to apply the math
            if (currentWorld.customHoopPrefab == null)
            {
                spawnedNorthHoop.GetComponent<HoopController>()?.ApplyStandardSettings(currentWorld);
                // spawnedSouthHoop.GetComponent<HoopController>()?.ApplyStandardSettings(currentWorld);
            }
        }
    }
}