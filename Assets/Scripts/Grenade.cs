using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour
{
    [Header("Grenade Settings")]
    public float fuseTime = 3f;             // Delay before the grenade explodes.
    public GameObject shrapnelPrefab;       // Prefab for the shrapnel piece.
    public int shrapnelCount = 20;          // How many shrapnel pieces to spawn.
    public float shrapnelForce = 10f;       // Force applied to each shrapnel piece.

    [Header("Explosion Settings")]
    public float blastRadius = 5f;          // Radius of the explosion effect.
    public int explosionDamage = 14;     // Damage applied to each enemy in the blast radius.

    [Header("Flash Settings")]
    public Color flashColor = Color.red;    // Color to flash.
    
    [Header("Audio Settings")]
    public AudioClip tossSFX;               // Sound effect played when the grenade is tossed.
    public AudioClip explosionSFX;          // Sound effect played when the grenade explodes.

    private Renderer rend;
    private Color originalColor;
    private float startTime;

    void Start()
    {
        startTime = Time.time;
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }

        // Play the toss sound effect at the grenade's position.
        if (tossSFX != null)
        {
            AudioSource.PlayClipAtPoint(tossSFX, transform.position);
        }

        // Start the flash and explosion countdown coroutines.
        StartCoroutine(FlashRoutine());
        StartCoroutine(ExplosionCountdown());
    }

    IEnumerator FlashRoutine()
    {
        // Loop until the grenade explodes.
        while (Time.time - startTime < fuseTime)
        {
            // Calculate time left and map it to a flash interval.
            float timeLeft = fuseTime - (Time.time - startTime);
            // The flash interval linearly decreases as time runs out.
            float flashInterval = Mathf.Lerp(0.05f, 0.5f, timeLeft / fuseTime);

            // Flash to the flash color.
            if (rend != null)
                rend.material.color = flashColor;
            yield return new WaitForSeconds(flashInterval);

            // Revert to the original color.
            if (rend != null)
                rend.material.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
        }
    }

    IEnumerator ExplosionCountdown()
    {
        // Wait for the fuse time.
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    void Explode()
    {
        Debug.Log("Grenade exploded!");

        // Play the explosion sound effect.
        if (explosionSFX != null)
        {
            AudioSource.PlayClipAtPoint(explosionSFX, transform.position);
        }

        // Apply explosion damage to all enemies within the blast radius.
        Collider[] colliders = Physics.OverlapSphere(transform.position, blastRadius);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                // Assumes the enemy has a script with a TakeDamage(float damage) method.
                var enemy = col.GetComponent<EnemyHitFlash>();
                if (enemy != null)
                {
                    enemy.TakeDamage(explosionDamage);
                }
            }
        }

        // Optionally, add explosion visual effects here.

        // Spawn shrapnel pieces.
        for (int i = 0; i < shrapnelCount; i++)
        {
            GameObject shrapnel = Instantiate(shrapnelPrefab, transform.position, Quaternion.identity);
            Vector3 randomDir = Random.onUnitSphere;
            Rigidbody rb = shrapnel.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Vector3.up * shrapnelForce, ForceMode.Impulse);
            }
        }
        // Destroy the grenade after explosion.
        Destroy(gameObject);
    }
}
