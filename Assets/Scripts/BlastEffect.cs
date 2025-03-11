using UnityEngine;
using System.Collections;

public class BlastEffect : MonoBehaviour
{
    [HideInInspector]
    public float scaleDuration = 0.3f;    // Time to scale up to max radius.
    [HideInInspector]
    public float fadeDuration = 1f;       // Time to fade out after reaching max radius.
    [HideInInspector]
    public float blastRadius = 5f;        // Blast radius to scale the effect.
    [HideInInspector]
    public Color blastColor;              // Base color of the blast effect.
    [HideInInspector]
    public Material blastMaterial;        // Predefined material for the blast effect (must support transparency).

    // Additional parameters for the downward translation.
    [HideInInspector]
    public float downDistance = 10f;      // Distance to translate downward.
    [HideInInspector]
    public float downDuration = 1f;     // Duration of the downward translation.

    void Start()
    {
        StartCoroutine(PlayEffect());
    }

    IEnumerator PlayEffect()
    {
        // Create a sphere to serve as the blast visual.
        GameObject blastVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        blastVisual.transform.position = transform.position;

        // Remove the collider so it doesn't interfere with gameplay.
        Collider col = blastVisual.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }

        // Instantiate the provided material to avoid modifying the original asset.
        Material mat = Instantiate(blastMaterial);
        blastVisual.GetComponent<Renderer>().material = mat;

        // Ensure the shader supports transparency (e.g., Transparent/Diffuse).
        // Optionally, adjust the render queue if needed:
        // mat.renderQueue = 3000;

        // Set the initial color with full alpha.
        Color initialColor = blastColor;
        initialColor.a = 1f;
        mat.color = initialColor;

        // Calculate scaling: start at zero and scale to the full diameter (blastRadius * 2).
        Vector3 initialScale = Vector3.zero;
        Vector3 finalScale = Vector3.one * blastRadius * 2f;

        // Phase 1: Scale Up
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleDuration);
            blastVisual.transform.localScale = Vector3.Lerp(initialScale, finalScale, t);
            yield return null;
        }
        // Ensure the sphere is exactly at maximum scale.
        blastVisual.transform.localScale = finalScale;

        // Phase 2: Fade Out at Constant Scale over a maximum duration.
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration); // t goes from 0 to 1 over fadeDuration.
            // Set the alpha so that it decreases linearly from 1 to 0.
            Color c = mat.color;
            c.a = 1f - t;
            mat.color = c;
            yield return null;
        }
        
        // Phase 3: Translate the blast downward quickly.
        Vector3 initialPosition = blastVisual.transform.position;
        Vector3 targetPosition = initialPosition + Vector3.down * downDistance;
        elapsed = 0f;
        while (elapsed < downDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / downDuration);
            blastVisual.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
            yield return null;
        }

        // Cleanup: Destroy the blast visual and its container.
        Destroy(blastVisual);
        Destroy(gameObject);
    }
}
