using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Tracking")]
    public int currentScore = 0;

    // This function automatically fires when something enters the Trigger Box
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that passed through is the basketball
        if (other.CompareTag("Ball"))
        {
            currentScore++;
            Debug.Log($"SWISH! Excellent shot. Current Score: {currentScore}");
            
            // We will add the audio and UI code here next!
        }
    }
}