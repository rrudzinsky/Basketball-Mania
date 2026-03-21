using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Drag your TextMeshPro Score Text object here.")]
    public TMP_Text scoreText;

    [Header("Score Tracking")]
    public int currentScore = 0;

    [Header("Logic Settings")]
    [Tooltip("How long the 'Top Gate' stays primed before resetting (in seconds).")]
    public float swishWindow = 1.5f;

    private bool ballHasPassedTop = false;

    private void Start()
    {
        UpdateScoreUI();
        SetupSensors();
    }

    // 1. AUTOMATIC SETUP
    // This finds the children created by your HoopSensorGenerator and 
    // attaches a tiny "proxy" so this script can hear their triggers.
    private void SetupSensors()
    {
        Transform top = transform.Find("Sensor_Top");
        Transform bottom = transform.Find("Sensor_Bottom");

        if (top != null) 
            top.gameObject.AddComponent<SensorTriggerProxy>().Initialize(this, true);
        
        if (bottom != null) 
            bottom.gameObject.AddComponent<SensorTriggerProxy>().Initialize(this, false);
    }

    // 2. THE CORE LOGIC
    // This is called whenever the ball hits either the top or bottom gate.
    public void OnSensorTriggered(bool isTopGate, Collider other)
    {
        // Verify the object is tagged as "Ball"
        if (!other.CompareTag("Ball")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        // REQUIREMENT: The ball must be moving DOWN (negative Y velocity)
        bool isMovingDown = rb.linearVelocity.y < -0.1f;

        if (isTopGate)
        {
            if (isMovingDown)
            {
                ballHasPassedTop = true;
                CancelInvoke(nameof(ResetHoopState));
                Invoke(nameof(ResetHoopState), swishWindow);
            }
        }
        else // Bottom Gate
        {
            if (ballHasPassedTop && isMovingDown)
            {
                currentScore++;
                UpdateScoreUI();
                ResetHoopState(); // Success, reset the sequence!
                Debug.Log($"<color=green>SWISH!</color> Total Score: {currentScore}");
            }
        }
    }

    private void ResetHoopState()
    {
        ballHasPassedTop = false;
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + currentScore.ToString();
        }
    }
}

// This small "Helper" class lives in the same file to keep things tidy.
// It simply passes trigger events from the child sensors back to the ScoreManager.
public class SensorTriggerProxy : MonoBehaviour
{
    private ScoreManager manager;
    private bool isTop;

    public void Initialize(ScoreManager managerScript, bool topGate)
    {
        manager = managerScript;
        isTop = topGate;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (manager != null)
        {
            manager.OnSensorTriggered(isTop, other);
        }
    }
}