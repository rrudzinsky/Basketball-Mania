using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerShooting : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject meterParent; 
    public Image fillImage;        
    public RectTransform targetZone; 
    public TMP_Text feedbackText; 

    [Header("Timing Logic")]
    public float perfectTime = 0.75f; 
    public float totalBarTime = 1.0f; 
    public float hideMeterDelay = 2.5f; 
    
    [Header("Dynamic Difficulty")]
    public float closeDistance = 4.0f;  
    public float farDistance = 14.0f;   
    public float maxGreenWindow = 0.4f; 
    public float minGreenWindow = 0.1f; 
    private float currentGreenWindow; 

    [Header("Physics & Targeting")]
    public GameObject basketball;
    public Vector3 targetHoopPos = new Vector3(0, 3.75f, 12f);
    
    [Header("Pickup Mechanics")]
    public float pickupDistance = 2.0f; 
    public float pickupCooldown = 1.0f;

    private float chargeTime = 0f;
    private float timeSinceShot = 0f;
    private bool isCharging = false;
    private bool hasBall = true;
    
    private Rigidbody playerRb;
    private Rigidbody ballRb;
    private SphereCollider ballCollider; // Added: Reference to the ball's collider
    private DribbleController dribbleScript;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        ballRb = basketball.GetComponent<Rigidbody>();
        ballCollider = basketball.GetComponent<SphereCollider>(); // Added: Grabbing the collider
        dribbleScript = basketball.GetComponent<DribbleController>();
        
        if (meterParent != null) meterParent.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);

        // Optional safety check: Ensure the ball starts with physics disabled if the player starts with it
        if (hasBall && ballCollider != null) 
        {
            ballCollider.enabled = false;
        }
    }

    void Update()
    {
        if (!hasBall) timeSinceShot += Time.deltaTime;

        if (!hasBall && !isCharging && timeSinceShot > pickupCooldown)
        {
            float distToBall = Vector3.Distance(transform.position, basketball.transform.position);
            if (distToBall < pickupDistance) PickupBall();
        }

        if (hasBall && !isCharging) UpdateDynamicMeter();

        if (Keyboard.current.spaceKey.wasPressedThisFrame && hasBall && !isCharging)
            StartCharge();

        if (Keyboard.current.spaceKey.wasReleasedThisFrame && isCharging)
            ReleaseBall();

        if (isCharging)
        {
            chargeTime += Time.deltaTime;
            
            if (fillImage != null)
            {
                fillImage.fillAmount = Mathf.Min(chargeTime / totalBarTime, 1f);
            }

            if (chargeTime > totalBarTime + 0.2f) ReleaseBall();
        }
    }

    void UpdateDynamicMeter()
    {
        Vector3 flatPlayer = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatHoop = new Vector3(targetHoopPos.x, 0, targetHoopPos.z);
        float distToHoop = Vector3.Distance(flatPlayer, flatHoop);

        float distanceT = Mathf.Clamp01((distToHoop - closeDistance) / (farDistance - closeDistance));
        float safeMaxWindow = Mathf.Min(maxGreenWindow, totalBarTime * 0.9f);
        currentGreenWindow = Mathf.Lerp(safeMaxWindow, minGreenWindow, distanceT);

        if (targetZone != null)
        {
            float centerPct = perfectTime / totalBarTime;
            float windowPct = currentGreenWindow / totalBarTime;

            float startPct = Mathf.Clamp(centerPct - (windowPct / 2f), 0f, 1f);
            float endPct = Mathf.Clamp(centerPct + (windowPct / 2f), 0f, 1f);

            targetZone.anchorMin = new Vector2(0f, startPct);
            targetZone.anchorMax = new Vector2(1f, endPct);
            targetZone.offsetMin = Vector2.zero;
            targetZone.offsetMax = Vector2.zero;
        }
    }

    void StartCharge()
    {
        isCharging = true;
        chargeTime = 0f;
        
        StopAllCoroutines(); 
        
        if (meterParent != null) meterParent.SetActive(true);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false); 
        playerRb.AddForce(Vector3.up * 6f, ForceMode.Impulse); 
    }

    void ReleaseBall()
    {
        if (!isCharging) return;
        isCharging = false; 
        hasBall = false;
        timeSinceShot = 0f;

        float timeDiff = Mathf.Abs(chargeTime - perfectTime);
        float halfWindow = currentGreenWindow / 2f;
        
        bool isPerfectRelease = timeDiff <= halfWindow;

        float accuracy = 1f;
        if (!isPerfectRelease)
        {
            float missAmount = timeDiff - halfWindow; 
            accuracy = Mathf.Clamp01(1.0f - (missAmount / 0.5f)); 
        }

        EvaluateShotFeedback(isPerfectRelease, chargeTime, perfectTime, accuracy);

        dribbleScript.enabled = false;
        
        // --- PHYSICS WAKE-UP LOGIC ---
        basketball.transform.SetParent(null); // Best practice unparenting
        ballRb.isKinematic = false; // Turn on gravity/forces
        if (ballCollider != null) ballCollider.enabled = true; // Turn on collisions!

        float finalAngle;
        Vector3 finalTarget = CalculateSmartTarget(out finalAngle);
        
        finalTarget += new Vector3(0, 0.25f, 0); 

        if (!isPerfectRelease)
        {
            float missSeverity = 1.0f - accuracy; 
            float spreadRadius = 0.4f * missSeverity; 
            
            float randomX = Random.Range(-spreadRadius, spreadRadius);
            float randomZ = Random.Range(-spreadRadius, spreadRadius);

            finalTarget += new Vector3(randomX, 0f, randomZ);
        }

        Vector3 perfectVelocity = CalculateKinematicTrajectory(basketball.transform.position, finalTarget, finalAngle);
        ballRb.AddForce(perfectVelocity, ForceMode.VelocityChange);

        StartCoroutine(HideMeterRoutine());
    }

    System.Collections.IEnumerator HideMeterRoutine()
    {
        yield return new WaitForSeconds(hideMeterDelay);
        if (meterParent != null) meterParent.SetActive(false);
    }

    Vector3 CalculateSmartTarget(out float launchAngle)
    {
        Vector3 flatPlayer = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatHoop = new Vector3(targetHoopPos.x, 0, targetHoopPos.z);
        float distToHoop = Vector3.Distance(flatPlayer, flatHoop);

        Vector3 dirToHoop = (flatHoop - flatPlayer).normalized;
        float angleFromCenter = Vector3.SignedAngle(Vector3.forward, dirToHoop, Vector3.up);

        Vector3 smartTarget = targetHoopPos;

        if (distToHoop <= closeDistance)
        {
            if (Mathf.Abs(angleFromCenter) < 25f)
            {
                launchAngle = 65f; 
                smartTarget = targetHoopPos; 
            }
            else
            {
                launchAngle = 60f;
                float sideOffset = (angleFromCenter > 0) ? -0.25f : 0.25f; 
                smartTarget = targetHoopPos + new Vector3(sideOffset, 0.45f, 0.3f); 
            }
        }
        else
        {
            float t = Mathf.Clamp01((distToHoop - closeDistance) / (farDistance - closeDistance));
            launchAngle = Mathf.Lerp(55f, 45f, t);
            smartTarget = targetHoopPos;
        }

        return smartTarget;
    }

    void PickupBall()
    {
        hasBall = true;
        
        // --- PHYSICS SLEEP LOGIC ---
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.isKinematic = true; // Turn off gravity/forces
        if (ballCollider != null) ballCollider.enabled = false; // Turn off collisions to prevent jitter!

        basketball.transform.parent = transform;
        basketball.transform.localPosition = new Vector3(0.5f, 1f, 0.5f); 
        dribbleScript.enabled = true;
        
        if (meterParent != null) meterParent.SetActive(false);
    }

    void EvaluateShotFeedback(bool isPerfect, float heldTime, float target, float acc)
    {
        if (feedbackText == null) return;

        string message;
        Color textColor;

        if (isPerfect) 
        { 
            message = "EXCELLENT!"; 
            textColor = Color.green; 
        } 
        else if (heldTime < target) 
        { 
            message = (acc > 0.5f) ? "SLIGHTLY EARLY" : "EARLY"; 
            textColor = (acc > 0.5f) ? Color.yellow : Color.red; 
        } 
        else 
        { 
            message = (acc > 0.5f) ? "SLIGHTLY LATE" : "LATE"; 
            textColor = (acc > 0.5f) ? Color.yellow : Color.red; 
        }

        StartCoroutine(DisplayFeedbackRoutine(message, textColor));
    }

    System.Collections.IEnumerator DisplayFeedbackRoutine(string msg, Color col)
    {
        feedbackText.text = msg;
        feedbackText.color = col;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(hideMeterDelay); 
        feedbackText.gameObject.SetActive(false);
    }

    Vector3 CalculateKinematicTrajectory(Vector3 start, Vector3 target, float angle)
    {
        Vector3 displacement = target - start;
        float h = displacement.y;
        displacement.y = 0;
        float r = displacement.magnitude;
        
        float a = angle * Mathf.Deg2Rad;
        float g = Mathf.Abs(Physics.gravity.y);
        
        float denominator = 2 * Mathf.Pow(Mathf.Cos(a), 2) * (r * Mathf.Tan(a) - h);
        if (denominator <= 0) return Vector3.up * 5f; 
        
        float v = Mathf.Sqrt((g * r * r) / denominator);
        
        Vector3 velocity = displacement.normalized * Mathf.Cos(a);
        velocity.y = Mathf.Sin(a);
        
        return velocity * v;
    }
}