using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Tracking Targets")]
    [Tooltip("Drag your Player object here in the Inspector.")]
    public Transform target;
    
    // Changed from public to private to hide it from the Inspector!
    private Transform hoop;

    [Header("Positioning Settings")]
    [Tooltip("How fast the camera catches up to the player. Higher is snappier.")]
    public float smoothSpeed = 10f;

    [Tooltip("How far behind the player the camera sits.")]
    public float distanceBehind = 4.5f;

    [Tooltip("How high above the player the camera sits.")]
    public float heightOffset = 2.5f;

    void Start()
    {
        // Search the scene for the hoop!
        if (hoop == null)
        {
            GameObject foundHoop = GameObject.Find("Hoop_North"); 
            
            if (foundHoop != null)
            {
                hoop = foundHoop.transform;
            }
            else
            {
                Debug.LogWarning("CameraFollow couldn't find 'Hoop_North' in the scene!");
            }
        }
    }

    void LateUpdate()
    {
        // Don't do anything if we haven't assigned our targets yet
        if (target == null || hoop == null) return;

        // 1. THE VECTOR MATH (Calculate the Hoop-to-Player axis)
        Vector3 hoopToPlayer = target.position - hoop.position;
        hoopToPlayer.y = 0; // Flatten it to the XZ plane so the camera doesn't dive into the floor
        
        Vector3 backwardDir = hoopToPlayer.normalized;

        // 2. CALCULATE DESIRED POSITION
        Vector3 desiredPosition = target.position + (backwardDir * distanceBehind);
        desiredPosition.y = target.position.y + heightOffset;

        // 3. SMOOTH MOVEMENT
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 4. THE 2K BROADCAST ANGLE
        Vector3 lookTarget = hoop.position;
        lookTarget.y = target.position.y + 1.5f; 
        
        transform.LookAt(lookTarget);
    }
}