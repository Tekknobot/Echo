using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour
{
    [Header("Grenade Settings")]
    public float fuseTime = 3f;             // Delay before the grenade explodes.
    public GameObject shrapnelPrefab;       // Prefab for each fragment piece.
    public int shrapnelCount = 20;          // Number of fragments to spawn on explosion.
    public float shrapnelForce = 10f;       // Force applied to each fragment.

    [Header("Explosion Settings")]
    public float blastRadius = 5f;          // Radius of the explosion effect.
    public int explosionDamage = 14;        // Damage applied to each enemy in the blast radius.
    public float knockbackForce = 5f;       // Knockback force applied to the player if in range.

    [Header("Flash Settings")]
    public Color flashColor = Color.red;    // Color to flash.

    [Header("Audio Settings")]
    public AudioClip tossSFX;               // Sound effect played when the grenade is tossed.
    public AudioClip explosionSFX;          // Sound effect played when the grenade explodes.

    [Header("Visual Explosion Settings")]
    public Material blastEffectMaterial;    // Predefined material for the blast effect.

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
            float timeLeft = fuseTime - (Time.time - startTime);
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

        // Instantiate the blast effect (if desired).
        GameObject blastEffectGO = new GameObject("BlastEffect");
        blastEffectGO.transform.position = transform.position;
        BlastEffect blastEffect = blastEffectGO.AddComponent<BlastEffect>();
        blastEffect.blastRadius = blastRadius;
        blastEffect.blastColor = flashColor;
        blastEffect.blastMaterial = blastEffectMaterial != null ? blastEffectMaterial : new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));

        // Process explosion effects: apply damage and knockback.
        Collider[] colliders = Physics.OverlapSphere(transform.position, blastRadius);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                var enemy = col.GetComponent<EnemyHitFlash>();
                if (enemy != null)
                {
                    enemy.TakeDamage(explosionDamage);
                }
            }
            else if (col.CompareTag("Player"))
            {
                ScreenFlash screenFlash = FindAnyObjectByType<ScreenFlash>();
                if (screenFlash != null)
                {
                    screenFlash.Flash();
                }
                
                PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(explosionDamage);
                }
                
                Vector3 knockbackDir = (col.transform.position - transform.position).normalized;
                Rigidbody playerRb = col.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    playerRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
                }
                else
                {
                    PlayerKnockback knockback = col.GetComponent<PlayerKnockback>();
                    if (knockback != null)
                    {
                        knockback.ApplyKnockback(knockbackDir * knockbackForce);
                    }
                }
            }
        }

        // Fragment the grenade into pieces.
        // Adjust fragmentRadius as needed to control spawn area.
        float fragmentRadius = 0.5f;
        if (shrapnelPrefab != null)
        {
            for (int i = 0; i < shrapnelCount; i++)
            {
                // Randomly offset each fragment from the grenade's center.
                Vector3 offset = Random.insideUnitSphere * fragmentRadius;
                Vector3 spawnPos = transform.position + offset;
                GameObject fragment = Instantiate(shrapnelPrefab, spawnPos, Random.rotation);

                // Ensure the fragment has a Rigidbody.
                Rigidbody rb = fragment.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = fragment.AddComponent<Rigidbody>();
                }
                // Ensure the fragment has a Collider.
                Collider col = fragment.GetComponent<Collider>();
                if (col == null)
                {
                    fragment.AddComponent<BoxCollider>();
                }
                // Apply force away from the grenade's center.
                Vector3 forceDir = offset.normalized;
                if (forceDir == Vector3.zero)
                {
                    forceDir = Random.onUnitSphere;
                }
                rb.AddForce(forceDir * shrapnelForce, ForceMode.Impulse);
            }
        }
        
        // Destroy the grenade after explosion.
        Destroy(gameObject);
    }
}
