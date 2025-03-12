using UnityEngine;
using System.Collections;

public class BlastEffect : MonoBehaviour
{
    [HideInInspector]
    public float scaleDuration = 0.1f;    // Time to scale up to max radius.
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
    public float downDistance = 0f;      // Distance to translate downward.
    [HideInInspector]
    public float downDuration = 0f;       // Duration of the downward translation.

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

        // Get the MeshRenderer from the instantiated blastVisual.
        MeshRenderer mainRenderer = blastVisual.GetComponent<MeshRenderer>();

        // Instantiate the provided material to avoid modifying the original asset.
        Material mainMat = Instantiate(blastMaterial);
        mainRenderer.material = mainMat;

        // Set the initial color with full alpha.
        Color initialColor = blastColor;
        initialColor.a = 1f;
        mainMat.color = initialColor;

        // Create an additional mesh effect by duplicating the blastVisual as a child.
        GameObject additionalMesh = Instantiate(blastVisual, blastVisual.transform.position, blastVisual.transform.rotation, blastVisual.transform);
        MeshRenderer additionalRenderer = additionalMesh.GetComponent<MeshRenderer>();
        Material additionalMat = Instantiate(blastMaterial);
        additionalRenderer.material = additionalMat;
        Color additionalColor = blastColor;
        additionalColor.a = 1f;
        additionalMat.color = additionalColor;

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

        // Store the initial metallic and smoothness values.
        float initialMetallic = mainMat.GetFloat("_Metallic");
        float initialSmoothness = mainMat.GetFloat("_Smoothness");

        // Phase 2: Fade Out at Constant Scale.
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            // Fade the main blast material's alpha.
            Color c = mainMat.color;
            c.a = 1f - t;
            mainMat.color = c;

            // Fade metallic and smoothness properties on the main material.
            mainMat.SetFloat("_Metallic", initialMetallic * (1f - t));
            mainMat.SetFloat("_Smoothness", initialSmoothness * (1f - t));

            // Fade the additional mesh's material alpha.
            Color addC = additionalMat.color;
            addC.a = 1f - t;
            additionalMat.color = addC;

            // Fade metallic and smoothness on the additional material.
            additionalMat.SetFloat("_Metallic", initialMetallic * (1f - t));
            additionalMat.SetFloat("_Smoothness", initialSmoothness * (1f - t));

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
