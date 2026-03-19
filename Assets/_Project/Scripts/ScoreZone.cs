using UnityEngine;

public class ScoreZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // 1. Check if the object is the basketball
        if (other.gameObject.name == "Basketball" || other.CompareTag("Player"))
        {
            Rigidbody ballRb = other.GetComponent<Rigidbody>();

            // 2. Only count the score if the ball is moving DOWN 
            // (A negative Y velocity means it's falling)
            if (ballRb != null && ballRb.linearVelocity.y < 0)
            {
                Debug.Log("SWISH! Point Scored!");
            }
        }
    }
}
