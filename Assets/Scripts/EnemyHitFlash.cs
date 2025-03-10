using UnityEngine;
using System.Collections;

public class EnemyHitFlash : MonoBehaviour {
    [Header("Health Settings")]
    public int health = 30;  // Initial health

    [Header("Flash Settings")]
    public Color flashColor = Color.red;    // The color to flash when hit.
    public float flashDuration = 0.1f;        // Duration (in seconds) the enemy remains flashed.

    private Color originalColor;
    private Renderer enemyRenderer;
    private bool isFlashing = false;
    private bool isDying = false;  // To prevent further damage during death sequence

    void Awake() {
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null) {
            // Using .material ensures you get an instance of the material
            originalColor = enemyRenderer.material.color;
        }
    }

    /// <summary>
    /// Call this method when the enemy is hit by the player's raycast.
    /// It will flash the enemy and reduce its health by 1.
    /// </summary>
    public void FlashAndTakeDamage() {
        if (isDying) return;  // Ignore hits if already dying
        
        if (!isFlashing && enemyRenderer != null) {
            StartCoroutine(FlashCoroutine());
        }
        health--;
        if (health <= 0 && !isDying) {
            Die();
        }
    }

    IEnumerator FlashCoroutine() {
        isFlashing = true;
        // Change the material color to the flash color.
        enemyRenderer.material.color = flashColor;
        // Wait for the flash duration.
        yield return new WaitForSeconds(flashDuration);
        // Revert to the original color.
        enemyRenderer.material.color = originalColor;
        isFlashing = false;
    }

    void Die() {
        isDying = true;
        
        // Disable the enemy AI to prevent further attacks.
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) {
            ai.enabled = false;
        }
        
        // Optionally, disable other enemy components (e.g., collider) here.
        
        // Start the death sequence (elevate enemy and destroy after a delay).
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence() {
        float duration = 25f;
        float elapsed = 0f;
        while (elapsed < duration) {
            // Gradually elevate the enemy (1 unit per second upward).
            transform.position += new Vector3(0, 1f * Time.deltaTime, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
