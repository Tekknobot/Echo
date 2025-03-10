using UnityEngine;
using System.Collections;

public class EnemyHitFlash : MonoBehaviour {
    // The color to flash when hit.
    public Color flashColor = Color.red;
    // Duration (in seconds) the enemy remains flashed.
    public float flashDuration = 0.1f;
    
    // Store the original color so it can revert.
    private Color originalColor;
    private Renderer enemyRenderer;
    private bool isFlashing = false;

    void Awake() {
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null) {
            // Using .material ensures you get an instance of the material
            originalColor = enemyRenderer.material.color;
        }
    }

    // Call this method when the enemy is hit by the player's raycast.
    public void Flash() {
        if (!isFlashing && enemyRenderer != null) {
            StartCoroutine(FlashCoroutine());
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
}
