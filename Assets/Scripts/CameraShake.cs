using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public float defaultShakeDuration = 0.1f;
    public float defaultShakeMagnitude = 0.1f;

    private Vector3 originalLocalPos;

    void Awake()
    {
        originalLocalPos = transform.localPosition;
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localPosition = originalLocalPos + Random.insideUnitSphere * magnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalLocalPos;
    }
}
