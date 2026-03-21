using UnityEngine;

public class ArenaBoundaryGenerator : MonoBehaviour
{
    [Header("Visual Settings")]
    [Tooltip("Make sure your Material is set to Render Face: Front!")]
    public Material wallMaterial;
    
    [Header("Effects")]
    [Tooltip("Drag your RedRipple_Prefab here from your Project folder!")]
    public GameObject impactPrefab; 
    
    [Header("Regulation Size (Playable Area)")]
    public float regulationWidth = 15.24f;
    public float regulationLength = 28.65f;

    [Header("Wall Settings")]
    public float wallHeight = 10f;
    public float wallThickness = 0.5f;

    [ContextMenu("Generate Boundaries")]
    public void GenerateBoundaries()
    {
        Transform oldContainer = transform.Find("Invisible_Boundaries");
        if (oldContainer != null) DestroyImmediate(oldContainer.gameObject);

        GameObject container = new GameObject("Invisible_Boundaries");
        container.transform.SetParent(this.transform);
        container.transform.localPosition = Vector3.zero;

        float w = regulationWidth / 2f;
        float l = regulationLength / 2f;
        float y = wallHeight / 2f; 

        // FIX 1: Corrected Rotations!
        // Unity Quads face Negative-Z by default. We must rotate them to face the (0,0,0) center.
        
        // North Wall (+Z). Needs to face -Z (inward). Rotation is 0.
        CreateWall("Wall_North", new Vector3(0, y, l), new Vector2(regulationWidth, wallHeight), wallThickness, new Vector3(0, 0, 0), container.transform);
        
        // South Wall (-Z). Needs to face +Z (inward). Rotation is 180.
        CreateWall("Wall_South", new Vector3(0, y, -l), new Vector2(regulationWidth, wallHeight), wallThickness, new Vector3(0, 180, 0), container.transform);
        
        // East Wall (+X). Needs to face -X (inward). Rotation is 90.
        CreateWall("Wall_East", new Vector3(w, y, 0), new Vector2(regulationLength, wallHeight), wallThickness, new Vector3(0, 90, 0), container.transform);
        
        // West Wall (-X). Needs to face +X (inward). Rotation is -90.
        CreateWall("Wall_West", new Vector3(-w, y, 0), new Vector2(regulationLength, wallHeight), wallThickness, new Vector3(0, -90, 0), container.transform);

        Debug.Log("Interactive, Inward-Facing Boundaries Generated!");
    }

    void CreateWall(string name, Vector3 pos, Vector2 size, float thickness, Vector3 rotation, Transform parent)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Quad);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.localPosition = pos;
        
        // This scale stretches the Quad to the exact dimensions of the wall
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);
        wall.transform.localEulerAngles = rotation;

        // Physics
        DestroyImmediate(wall.GetComponent<MeshCollider>());
        BoxCollider col = wall.AddComponent<BoxCollider>();
        col.size = new Vector3(1f, 1f, thickness);

        // FIX 2: Removed Tiling Logic
        // By just applying the material, the image stretches to fit the 15m or 28m wall perfectly.
        MeshRenderer renderer = wall.GetComponent<MeshRenderer>();
        if (wallMaterial != null) 
        {
            renderer.material = wallMaterial;
        }
        else 
        {
            renderer.enabled = false;
        }

        // --- NEW: IMPACT SCRIPT ASSIGNMENT ---
        // Automatically add the WallImpact script to the wall and link the prefab
        WallImpact impactScript = wall.AddComponent<WallImpact>();
        impactScript.effectPrefab = impactPrefab;
    }
}