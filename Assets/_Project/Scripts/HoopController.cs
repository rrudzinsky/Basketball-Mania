using UnityEngine;

public class HoopController : MonoBehaviour
{
    [Header("Backboard Splitting")]
    [Tooltip("Drag the parent 'Backboard' object here")]
    public Transform backboardParent; 
    [Tooltip("Drag the 'BoardVisual' object here")]
    public MeshRenderer boardVisualRenderer; 
    [Tooltip("Drag the object with the Backboard's BoxCollider here")]
    public Collider backboardCollider;

    [Header("Rim Splitting")]
    [Tooltip("Drag the 'CircularRim' parent object here")]
    public Transform circularRimParent; 
    [Tooltip("Drag the object with the Rim's Collider here")]
    public Collider rimCollider;

    public void ApplyStandardSettings(WorldProfile profile)
    {
        // 1. Scale the Backboard (X is width, Y is height)
        // Leaving Z at 1f prevents us from squashing your colliders!
        if (backboardParent != null)
            backboardParent.localScale = new Vector3(profile.backboardSize.x, profile.backboardSize.y, 1f);

        // 2. Apply Visual Material (Using the new 'Appearance' name)
        if (boardVisualRenderer != null && profile.backboardAppearance != null)
            boardVisualRenderer.material = profile.backboardAppearance;

        // 3. Apply Physics Material (Using the new 'Physics' name)
        if (backboardCollider != null && profile.backboardPhysics != null)
            backboardCollider.material = profile.backboardPhysics;

        // 4. Scale the Rim
        if (circularRimParent != null)
        {
            float rimDiameter = profile.rimRadius * 2f; 
            // We only scale X and Z. Leaving Y at 1f protects your ScoreSensors!
            circularRimParent.localScale = new Vector3(rimDiameter, 1f, rimDiameter);
        }

        // 5. Apply Rim Physics Material (Using the new 'Physics' name)
        if (rimCollider != null && profile.rimPhysics != null)
            rimCollider.material = profile.rimPhysics;
    }
}