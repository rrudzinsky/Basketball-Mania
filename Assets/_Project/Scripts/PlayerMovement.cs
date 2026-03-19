using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6.0f;
    
    [Header("Targeting")]
    [Tooltip("The hoop the player should always face.")]
    public Transform hoopTransform; 
    
    private Rigidbody rb;
    private Vector2 rawInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Prevents the physics engine from tipping the player over
        rb.freezeRotation = true; 
    }

    // Unity's New Input System calls this automatically
    void OnMove(InputValue value)
    {
        rawInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        // FAILSAFE: If the hoop isn't assigned, warn the console but STILL let the player move!
        if (hoopTransform == null)
        {
            Debug.LogWarning("⚠️ Hoop Transform is missing! Please drag the Hoop into the PlayerMovement script.");
            
            // Basic fallback movement so WASD doesn't freeze
            Vector3 fallbackMove = new Vector3(rawInput.x, 0, rawInput.y).normalized * moveSpeed;
            fallbackMove.y = rb.linearVelocity.y;
            rb.linearVelocity = fallbackMove;
            return;
        }

        // 1. ROTATE TO FACE THE HOOP (The 2K Stance)
        Vector3 directionToHoop = hoopTransform.position - rb.position;
        directionToHoop.y = 0; // Keep the vector perfectly flat on the floor

        if (directionToHoop.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToHoop);
            rb.MoveRotation(targetRotation);
        }

        // 2. CALCULATE RELATIVE MOVEMENT (Strafing)
        // transform.forward is now "Toward the Hoop"
        // transform.right is now "Circle around the Hoop"
        Vector3 moveDirection = (transform.forward * rawInput.y) + (transform.right * rawInput.x);
        
        // Normalize to prevent faster diagonal movement
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // 3. APPLY VELOCITY
        // We calculate the flat movement, but preserve whatever Y velocity gravity is applying
        Vector3 finalVelocity = moveDirection * moveSpeed;
        
        // Changed from linearVelocity to velocity for maximum compatibility
        finalVelocity.y = rb.linearVelocity.y; 

        rb.linearVelocity = finalVelocity;
    }
}