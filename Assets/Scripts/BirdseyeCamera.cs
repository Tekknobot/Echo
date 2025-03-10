using UnityEngine;

public class BirdseyeCamera : MonoBehaviour
{
    [Header("Maze Reference (Optional)")]
    // Optionally assign the MazeGenerator (if available in the scene)
    public MazeGenerator mazeGenerator;

    [Header("Camera Settings")]
    // Height above the maze to place the camera
    public float cameraHeight = 50f;
    // Extra padding (in world units) to ensure the maze fits comfortably
    public float padding = 5f;

    void Start()
    {
        // Ensure this camera uses orthographic projection
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
        }
        
        if (mazeGenerator != null)
        {
            // Calculate the maze's total width and height in world units
            float mazeWidth = mazeGenerator.width * mazeGenerator.cellSize;
            float mazeHeight = mazeGenerator.height * mazeGenerator.cellSize;
            
            // Compute the center of the maze (adjusting for cellSize offset)
            Vector3 mazeCenter = new Vector3(
                mazeWidth / 2 - mazeGenerator.cellSize / 2,
                0,
                mazeHeight / 2 - mazeGenerator.cellSize / 2
            );

            // Position the camera above the maze center
            transform.position = new Vector3(mazeCenter.x, cameraHeight, mazeCenter.z);
            
            // Rotate the camera to look straight down (bird's-eye view)
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // Adjust the orthographic size to ensure the entire maze is visible
            // Orthographic size is half the vertical size of the viewing volume
            // Use the larger half-dimension of the maze plus extra padding
            if (cam != null)
            {
                float sizeX = mazeWidth / 2f;
                float sizeZ = mazeHeight / 2f;
                cam.orthographicSize = Mathf.Max(sizeX, sizeZ) + padding;
            }
        }
        else
        {
            Debug.LogWarning("MazeGenerator not assigned. Set the mazeGenerator field or position the camera manually.");
        }
    }
}
