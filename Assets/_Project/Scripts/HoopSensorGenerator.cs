using UnityEngine;

public class HoopSensorGenerator : MonoBehaviour
{
    [Header("Hoop Dimensions (In Meters)")]
    [Tooltip("Keep this the same as your RimColliderGenerator radius!")]
    public float rimRadius = 0.235f; 
    public float tubeRadius = 0.01f; 

    [Header("Sensor Settings")]
    [Tooltip("How far down the net hangs (determines Bottom Gate position).")]
    public float netLength = 0.4f;
    
    [Tooltip("Multiplier for the bottom sensor. 1.3x makes it wider to catch angled swishes.")]
    public float bottomSensorScale = 1.3f;

    [ContextMenu("Generate Score Sensors")]
    public void GenerateSensors()
    {
        // 1. SAFE CLEANUP
        Transform oldContainer = transform.Find("Score_Sensors_Container");
        if (oldContainer != null)
        {
            DestroyImmediate(oldContainer.gameObject);
        }

        // 2. CREATE CONTAINER
        GameObject container = new GameObject("Score_Sensors_Container");
        container.transform.SetParent(transform, false);

        // 3. SENSOR MATH (Decoupled!)
        float innerDiameter = (rimRadius - tubeRadius) * 2f;
        
        // Top sensor is constrained to 0.65f so its corners fit perfectly inside the metal tube.
        float topSensorWidth = innerDiameter * 0.65f;
        
        // Bottom sensor uses your original 0.85f math so it remains its original, wider size.
        float bottomSensorWidth = (innerDiameter * 0.85f) * bottomSensorScale;

        // 4. GENERATE TOP GATE
        GameObject topGate = new GameObject("Sensor_Top");
        topGate.transform.SetParent(container.transform, false);
        topGate.transform.localPosition = new Vector3(0f, -tubeRadius, 0f); 
        
        BoxCollider topCol = topGate.AddComponent<BoxCollider>();
        topCol.isTrigger = true;
        topCol.size = new Vector3(topSensorWidth, 0.02f, topSensorWidth);

        // 5. GENERATE BOTTOM GATE
        GameObject bottomGate = new GameObject("Sensor_Bottom");
        bottomGate.transform.SetParent(container.transform, false);
        bottomGate.transform.localPosition = new Vector3(0f, -netLength, 0f);
        
        BoxCollider bottomCol = bottomGate.AddComponent<BoxCollider>();
        bottomCol.isTrigger = true;
        bottomCol.size = new Vector3(bottomSensorWidth, 0.02f, bottomSensorWidth);

        Debug.Log($"Success! Top sensor safely tucked inside the rim. Bottom sensor restored to original width.");
    }
}