using UnityEngine;
using System.Collections;

public class Shrapnel : MonoBehaviour
{
    [Header("Flash Settings")]
    public Color flashColor = Color.yellow;   // Color to flash.
    public float flashDuration = 3f;          // Total duration to flash (in seconds).
    public float flashInterval = 0.05f;       // Time between color toggles.

    [Header("Fade Out Settings")]
    public float fadeOutDuration = 1f;        // Duration over which the bloom fades to nothing.

    private Renderer rend;
    private Color originalColor;
    private Color originalEmission;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
            // Cache the original emission color.
            originalEmission = rend.material.GetColor("_EmissionColor");
            StartCoroutine(FlashCoroutine());
        }
    }

    IEnumerator FlashCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            if (rend != null)
                rend.material.color = flashColor;
            yield return new WaitForSeconds(flashInterval);

            if (rend != null)
                rend.material.color = originalColor;
            yield return new WaitForSeconds(flashInterval);

            elapsed += 2 * flashInterval;
        }
        // After flashing, gradually reduce the bloom.
        yield return StartCoroutine(FadeOutBloom());
    }

    IEnumerator FadeOutBloom()
    {
        float elapsed = 0f;
        // Get the current emission color (should be near the original or flash color).
        Color startEmission = rend.material.GetColor("_EmissionColor");

        while (elapsed < fadeOutDuration)
        {
            float t = elapsed / fadeOutDuration;
            // Lerp the emission color from its current value to black.
            Color newEmission = Color.Lerp(startEmission, Color.black, t);
            if (rend != null)
                rend.material.SetColor("_EmissionColor", newEmission);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Ensure the emission is fully off and disable it to remove bloom.
        if (rend != null)
        {
            rend.material.SetColor("_EmissionColor", Color.black);
            rend.material.DisableKeyword("_EMISSION");
        }
    }
}
