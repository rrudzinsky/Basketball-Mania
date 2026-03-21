using UnityEngine;

public class WallImpact : MonoBehaviour
{
    [Tooltip("The particle system prefab to spawn on impact.")]
    public GameObject effectPrefab;

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Check if the object hitting the wall is the basketball
        if (collision.gameObject.CompareTag("Ball"))
        {
            if (effectPrefab != null)
            {
                // 2. Get the exact mathematical point of impact
                ContactPoint contact = collision.contacts[0];

                // 3. Push the spawn position slightly off the wall (0.05m) 
                // This prevents "Z-fighting" where the particle clips into the wall geometry
                Vector3 spawnPos = contact.point + (contact.normal * 0.05f);
                
                // 4. Rotate the particle effect so it faces perfectly outward from the wall
                Quaternion rotation = Quaternion.LookRotation(contact.normal);

                // 5. Spawn the visual effect
                GameObject splash = Instantiate(effectPrefab, spawnPos, rotation);

                // 6. Destroy the particle object after 1 second so your game doesn't lag over time
                Destroy(splash, 1f);
            }
        }
    }
}