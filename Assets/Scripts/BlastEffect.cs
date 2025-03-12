using UnityEngine;
using System.Collections;

public class BlastEffect : MonoBehaviour
{
    [HideInInspector]
    public float scaleDuration = 0.1f;    // Time to scale up and fade in.
    [HideInInspector]
    public float fadeDuration = 1f;       // Time to fade out after reaching full scale.
    [HideInInspector]
    public float blastRadius = 5f;        // Blast radius to scale the effect.
    [HideInInspector]
    public Color blastColor;              // Base color of the blast effect.
    [HideInInspector]
    public Material blastMaterial;        // Material for the blast effect (must support transparency).

    // Additional parameters for downward translation.
    [HideInInspector]
    public float downDistance = 0f;       // Distance to translate downward.
    [HideInInspector]
    public float downDuration = 0f;       // Duration of the downward translation.

    void Start()
    {
        StartCoroutine(PlayEffect());
    }

    IEnumerator PlayEffect()
    {
        // Create a sphere for the blast visual.
        GameObject blastVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        blastVisual.transform.position = transform.position;

        // Remove the collider so it doesn't interfere with gameplay.
        Collider col = blastVisual.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }

        // Get the MeshRenderer and instantiate the material.
        MeshRenderer mainRenderer = blastVisual.GetComponent<MeshRenderer>();
        Material mainMat = Instantiate(blastMaterial);
        mainRenderer.material = mainMat;

        // Determine the target metallic and smoothness values from the provided material.
        float targetMetallic = blastMaterial.GetFloat("_Metallic");
        float targetSmoothness = blastMaterial.GetFloat("_Smoothness");

        // Set starting values for the material: fully transparent and zero metallic/smoothness.
        Color initialColor = blastColor;
        initialColor.a = 0f;
        mainMat.color = initialColor;
        mainMat.SetFloat("_Metallic", 0f);
        mainMat.SetFloat("_Smoothness", 0f);

        // Calculate scaling: from zero to the full diameter (blastRadius * 2).
        Vector3 initialScale = Vector3.zero;
        Vector3 finalScale = Vector3.one * blastRadius * 2f;

        // Phase 1: Scale Up and Fade In (alpha, metallic, and smoothness go from 0 to target values) over scaleDuration.
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleDuration);

            // Scale up.
            blastVisual.transform.localScale = Vector3.Lerp(initialScale, finalScale, t);

            // Fade in: alpha goes from 0 to 1.
            Color c = mainMat.color;
            c.a = t;
            mainMat.color = c;

            // Increase metallic and smoothness from 0 to target.
            mainMat.SetFloat("_Metallic", targetMetallic * t);
            mainMat.SetFloat("_Smoothness", targetSmoothness * t);

            yield return null;
        }
        // Ensure final scale and full opacity with target metallic/smoothness.
        blastVisual.transform.localScale = finalScale;
        Color opaqueColor = blastColor;
        opaqueColor.a = 1f;
        mainMat.color = opaqueColor;

        // Store the current metallic and smoothness as the initial values for fade-out.
        float initialMetallic = targetMetallic;
        float initialSmoothness = targetSmoothness;

        // Phase 2: Fade Out (alpha, metallic, and smoothness decrease to 0) over fadeDuration.
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            // Fade out alpha.
            Color c = mainMat.color;
            c.a = 1f - t;
            mainMat.color = c;
            // Fade metallic and smoothness.
            mainMat.SetFloat("_Metallic", initialMetallic * (1f - t));
            mainMat.SetFloat("_Smoothness", initialSmoothness * (1f - t));

            yield return null;
        }

        // Phase 3: Optional downward translation (if downDuration > 0).
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

        // Cleanup.
        Destroy(blastVisual);
        Destroy(gameObject);
    }
}
