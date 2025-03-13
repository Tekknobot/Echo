using UnityEngine;

public class ExitTrigger : MonoBehaviour {
    private MazeGenerator mazeGenerator;
    private bool hasTriggered = false;

    void Start() {
        // Find the GameObject with the tag "Maze"
        GameObject mazeObj = GameObject.FindGameObjectWithTag("Maze");
        if (mazeObj != null) {
            mazeGenerator = mazeObj.GetComponent<MazeGenerator>();
        } else {
            Debug.LogWarning("Maze object with tag 'Maze' not found!");
        }
    }

    void OnTriggerEnter(Collider other) {
        if (!hasTriggered && other.CompareTag("Player") && mazeGenerator != null) {
            hasTriggered = true;
            // Reconfigure the maze layout in-place.
            mazeGenerator.ReconfigureMaze();
        }
    }
}
