using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Tracking Targets")]
    [Tooltip("Drag your Player object here in the Inspector.")]
    public Transform target;
    
    [Tooltip("Drag the Hoop/Backboard object here so the camera knows what to look at.")]
    public Transform hoop;

    [Header("Positioning Settings")]
    [Tooltip("How fast the camera catches up to the player. Higher is snappier.")]
    public float smoothSpeed = 10f;

    [Tooltip("How far behind the player the camera sits.")]
    public float distanceBehind = 4.5f;

    [Tooltip("How high above the player the camera sits.")]
    public float heightOffset = 2.5f;

    void LateUpdate()
    {
        // Don't do anything if we haven't assigned our targets yet
        if (target == null || hoop == null) return;

        // 1. THE VECTOR MATH (Calculate the Hoop-to-Player axis)
        // This gives us a vector pointing exactly away from the basket
        Vector3 hoopToPlayer = target.position - hoop.position;
        hoopToPlayer.y = 0; // Flatten it to the XZ plane so the camera doesn't dive into the floor
        
        Vector3 backwardDir = hoopToPlayer.normalized;

        // 2. CALCULATE DESIRED POSITION
        // Start at the player, push backward along our vector, then move up
        Vector3 desiredPosition = target.position + (backwardDir * distanceBehind);
        desiredPosition.y = target.position.y + heightOffset;

        // 3. SMOOTH MOVEMENT
        // Glide smoothly to the calculated position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 4. THE 2K BROADCAST ANGLE
        // Look at the hoop, but offset the target height slightly so we look over the player's shoulders
        Vector3 lookTarget = hoop.position;
        lookTarget.y = target.position.y + 1.5f; 
        
        transform.LookAt(lookTarget);
    }
}
