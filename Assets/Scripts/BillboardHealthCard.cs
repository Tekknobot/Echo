using UnityEngine;
using UnityEngine.UI;

public class BillboardHealthCard : MonoBehaviour
{
    private Canvas canvas;

    void Awake()
    {
        // If this GameObject has a Canvas component, assign the main camera to its worldCamera field.
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                canvas.worldCamera = mainCam;
            }
        }
    }

    void LateUpdate()
    {
        // Get the main camera (must have the "MainCamera" tag)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Rotate this object to always face the camera.
            transform.LookAt(mainCam.transform);
            // Optionally, rotate 180 degrees if your object is backwards.
            // transform.Rotate(0, 180f, 0);
        }
    }
}
