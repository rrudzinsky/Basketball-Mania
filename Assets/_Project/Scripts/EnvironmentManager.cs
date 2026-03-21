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
    
    // Hidden variables to keep track of the spawned hoops so we can delete them when changing planets
    private GameObject spawnedNorthHoop;
    private GameObject spawnedSouthHoop;

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
            floorRenderer.material = currentWorld.courtFloorAppearance; // Fixed naming error here
        
        if (floorTransform != null)
        {
            // Note: This assumes your floor is a standard 1x1 primitive scaled up. 
            // If it's a 10x10 Unity Plane, the math here would be divided by 10.
            floorTransform.localScale = new Vector3(currentWorld.courtWidth, 1f, currentWorld.courtLength);
            
            // NEW: Apply the Floor Physics!
            Collider floorCol = floorTransform.GetComponent<Collider>();
            if (floorCol != null && currentWorld.courtFloorPhysics != null)
            {
                floorCol.material = currentWorld.courtFloorPhysics;
            }
        }

        // 2. Update Boundaries & Size
        if (boundaryGenerator != null)
        {
            boundaryGenerator.wallMaterial = currentWorld.boundaryAppearance; // Fixed naming error here
            
            // Pass the new dimensions to your generator script
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
        // First, destroy old hoops if they exist so we don't get duplicates stacked on top of each other
        if (spawnedNorthHoop != null) DestroyImmediate(spawnedNorthHoop);
        if (spawnedSouthHoop != null) DestroyImmediate(spawnedSouthHoop);

        // Decide which blueprint to use: The Custom Alien one, or the Standard one?
        GameObject blueprintToSpawn = currentWorld.customHoopPrefab != null ? currentWorld.customHoopPrefab : standardHoopPrefab;

        if (blueprintToSpawn != null)
        {
            // Calculate where the edges of the court are
            float halfCourtLength = currentWorld.courtLength / 2f;

            // SPAWN NORTH: Place at +Z, rotate 180 degrees to face inward
            spawnedNorthHoop = Instantiate(blueprintToSpawn, new Vector3(0, 0, halfCourtLength), Quaternion.Euler(0, 180, 0), this.transform);
            spawnedNorthHoop.name = "Hoop_North";

            // SPAWN SOUTH: Place at -Z, rotate 0 degrees to face inward
            spawnedSouthHoop = Instantiate(blueprintToSpawn, new Vector3(0, 0, -halfCourtLength), Quaternion.Euler(0, 0, 0), this.transform);
            spawnedSouthHoop.name = "Hoop_South";

            // If we used the standard hoop (not a custom override), tell the HoopController to apply the math!
            if (currentWorld.customHoopPrefab == null)
            {
                spawnedNorthHoop.GetComponent<HoopController>()?.ApplyStandardSettings(currentWorld);
                spawnedSouthHoop.GetComponent<HoopController>()?.ApplyStandardSettings(currentWorld);
            }
        }
    }
}