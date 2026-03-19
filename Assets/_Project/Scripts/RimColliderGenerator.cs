using UnityEngine;

public class RimColliderGenerator : MonoBehaviour
{
    [Header("Dimensions (In Meters)")]
    [Tooltip("Distance from the exact center of the hoop to the middle of the iron.")]
    public float rimRadius = 0.235f; 
    
    [Tooltip("The thickness of the iron itself.")]
    public float tubeRadius = 0.01f; 
    
    [Header("Collider Settings")]
    [Range(16, 64)]
    [Tooltip("32 is the sweet spot for a perfectly smooth bounce.")]
    public int colliderCount = 32;
    
    public PhysicsMaterial rimPhysicsMaterial; 

    [ContextMenu("Generate Rim Colliders")]
    public void GenerateColliders()
    {
        // 1. SAFE CLEANUP: Only destroy the old rim container, leave the sensors alone!
        Transform oldContainer = transform.Find("Rim_Colliders_Container");
        if (oldContainer != null)
        {
            DestroyImmediate(oldContainer.gameObject);
        }

        // 2. CREATE THE CONTAINER
        // This keeps the hierarchy clean and protects other child objects
        GameObject container = new GameObject("Rim_Colliders_Container");
        container.transform.SetParent(transform, false);

        // 3. FLAWLESS DYNAMIC MATH
        float angleStep = 360f / colliderCount;
        float angleStepRad = angleStep * Mathf.Deg2Rad;
        
        // Calculate the exact straight-line distance (chord length) between two capsule centers
        float chordLength = 2f * rimRadius * Mathf.Sin(angleStepRad / 2f);
        
        // Unity's CapsuleHeight includes the two spherical caps (which are 2 * tubeRadius).
        // By adding the caps to the chord length, the spheres overlap perfectly at the joints!
        float capsuleHeight = chordLength + (tubeRadius * 2f);

        // 4. GENERATE THE RING
        for (int i = 0; i < colliderCount; i++)
        {
            float currentAngle = i * angleStep;
            float currentAngleRad = currentAngle * Mathf.Deg2Rad;

            // Find the exact X/Z coordinates along the perimeter of the circle
            float xPos = Mathf.Cos(currentAngleRad) * rimRadius;
            float zPos = Mathf.Sin(currentAngleRad) * rimRadius;
            Vector3 spawnPos = new Vector3(xPos, 0f, zPos);

            GameObject colliderObj = new GameObject($"RimCapsule_{i}");
            
            // PUT INSIDE THE CONTAINER (Instead of directly on the transform)
            colliderObj.transform.SetParent(container.transform, false); 
            colliderObj.transform.localPosition = spawnPos;

            // Point the capsule at the dead-center of the hoop. 
            // This perfectly aligns its local X-Axis to the curve of the ring.
            Vector3 directionToCenter = -spawnPos.normalized;
            if (directionToCenter != Vector3.zero)
            {
                colliderObj.transform.rotation = Quaternion.LookRotation(directionToCenter);
            }

            // Apply the math to the collider
            CapsuleCollider cap = colliderObj.AddComponent<CapsuleCollider>();
            cap.radius = tubeRadius;
            cap.height = capsuleHeight;
            
            // 0 = X-Axis. This lays the pill perfectly flat along the tangent line.
            cap.direction = 0; 
            
            cap.sharedMaterial = rimPhysicsMaterial;
        }

        Debug.Log($"Success! Generated {colliderCount} dynamic colliders inside 'Rim_Colliders_Container' for Rim Radius: {rimRadius}, Tube Radius: {tubeRadius}");
    }
}