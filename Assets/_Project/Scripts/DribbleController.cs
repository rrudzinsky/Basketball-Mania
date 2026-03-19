using UnityEngine;

public class DribbleController : MonoBehaviour
{
    [Header("Dribble Settings")]
    [Tooltip("How far down the ball bounces. 0.5 usually hits the floor.")]
    public float dribbleHeight = 0.5f;
    
    [Tooltip("How fast the ball bounces.")]
    public float dribbleSpeed = 10f;

    // This stores the ball's original position (the player's hand)
    private Vector3 handPosition;

    void Start()
    {
        // Remember where the ball started relative to the player
        handPosition = transform.localPosition;
    }

    void Update()
    {
        // We use an absolute Sine wave. It generates a perfect bouncing curve 
        // that goes from 0 (at the hand) to 1 (at the floor) and back over time.
        float bounce = Mathf.Abs(Mathf.Sin(Time.time * dribbleSpeed)) * dribbleHeight;

        // Apply the bounce by subtracting it from the starting Y position
        transform.localPosition = new Vector3(
            handPosition.x, 
            handPosition.y - bounce, 
            handPosition.z
        );
    }
}