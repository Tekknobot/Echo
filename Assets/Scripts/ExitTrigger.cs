using UnityEngine;
using TMPro;
using System.Collections;

public class ExitTrigger : MonoBehaviour {
    private MazeGenerator mazeGenerator;
    private bool hasTriggered = false;
    // The level variable persists across instances.
    private static int level = 1;
    // Flag to ensure only one exit trigger increments the level per round.
    private static bool levelIncremented = false;
    private TMP_Text levelText;

    void Start() {
        // Reset the increment flag when the game starts.
        ResetIncrement();

        // Find the MazeGenerator by tag "Maze".
        GameObject mazeObj = GameObject.FindGameObjectWithTag("Maze");
        if (mazeObj != null) {
            mazeGenerator = mazeObj.GetComponent<MazeGenerator>();
        } else {
            Debug.LogWarning("Maze object with tag 'Maze' not found!");
        }
        
        // Find the TextMeshPro object with tag "LevelText".
        GameObject levelTextObj = GameObject.FindGameObjectWithTag("LevelText");
        if (levelTextObj != null) {
            levelText = levelTextObj.GetComponent<TMP_Text>();
            if (levelText != null) {
                levelText.text = "Level " + level;
            }
        } else {
            Debug.LogWarning("LevelText object with tag 'LevelText' not found!");
        }
    }

    void OnTriggerEnter(Collider other) {
        if (!hasTriggered && other.CompareTag("Player") && mazeGenerator != null) {
            // Increment level once per round.
            if (!levelIncremented) {
                level++;
                if (levelText != null) {
                    levelText.text = "Level " + level;
                }
                levelIncremented = true;
            }
            
            // Start a coroutine to handle maze reconfiguration and then reposition the player.
            StartCoroutine(ReconfigureAndReposition(other));
            
            // Prevent this trigger from firing repeatedly during reconfiguration.
            hasTriggered = true;
            StartCoroutine(ResetTriggerAfterDelay());
        }
    }
    
    // Optional: if desired, you can also reset on exit.
    void OnTriggerExit(Collider other) {
        // Uncomment if you want to reset immediately when the player leaves:
        // if (other.CompareTag("Player")) {
        //     hasTriggered = false;
        //     ResetIncrement();
        // }
    }

    // Static method to reset the level (if needed from elsewhere).
    public static void ResetLevel() {
        level = 1;
        ResetIncrement();
        GameObject levelTextObj = GameObject.FindGameObjectWithTag("LevelText");
        if (levelTextObj != null) {
            TMP_Text tmp = levelTextObj.GetComponent<TMP_Text>();
            if (tmp != null) {
                tmp.text = "Level " + level;
            }
        }
    }
    
    // Reset the static increment flag.
    public static void ResetIncrement() {
        levelIncremented = false;
    }
    
    // Coroutine to reset trigger flags after a delay.
    IEnumerator ResetTriggerAfterDelay() {
        yield return new WaitForSeconds(1f); // Adjust delay as needed.
        hasTriggered = false;
        ResetIncrement();
    }
    
    // Coroutine to reconfigure the maze then reposition the player.
    IEnumerator ReconfigureAndReposition(Collider playerCollider) {
        // First, reconfigure the maze.
        mazeGenerator.ReconfigureMaze();
        
        // Wait a short moment to let the maze reconfigure.
        yield return new WaitForSeconds(0f); // Adjust this delay as needed.
        
        // Ensure MazeGenerator exposes the updated maze origin via a public property.
        Vector3 newPlayerPos = mazeGenerator.MazeOrigin + new Vector3(0, 2, 0);
        playerCollider.transform.position = newPlayerPos;
    }
    
    // This method is called by MazeGenerator to reposition the exit trigger to the new exit location.
    public void MoveToExit(Vector3 mazeOrigin) {
        // Calculate the new exit position based on the maze origin.
        Vector3 newExitPos = mazeOrigin + new Vector3((mazeGenerator.width - 1) * mazeGenerator.cellSize, 0, (mazeGenerator.height - 1) * mazeGenerator.cellSize);
        transform.position = newExitPos;
    }
}
