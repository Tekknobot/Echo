using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFlash : MonoBehaviour
{
    public float flashDuration = 0.3f; // Duration for the flash to fade out.
    private Image flashImage;

    void Awake()
    {
        flashImage = GetComponent<Image>();
        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }
    }

    public void Flash()
    {
        StopAllCoroutines(); // In case multiple flashes overlap.
        StartCoroutine(FlashCoroutine());
    }

    IEnumerator FlashCoroutine()
    {
        // Set the flash image to fully opaque (white).
        Color c = flashImage.color;
        c.a = 1f;
        flashImage.color = c;

        float timer = 0f;
        while (timer < flashDuration)
        {
            timer += Time.deltaTime;
            // Gradually interpolate alpha from 1 to 0.
            c.a = Mathf.Lerp(1f, 0f, timer / flashDuration);
            flashImage.color = c;
            yield return null;
        }
        c.a = 0f;
        flashImage.color = c;
    }
}
