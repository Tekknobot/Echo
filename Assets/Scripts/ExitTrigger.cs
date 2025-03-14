using UnityEngine;
using TMPro;

public class ExitTrigger : MonoBehaviour {
    private MazeGenerator mazeGenerator;
    private bool hasTriggered = false;
    // Make level static so it persists across different instances of ExitTrigger.
    private static int level = 1;
    private TMP_Text levelText;

    void Start() {
        // Find the MazeGenerator by tag "Maze"
        GameObject mazeObj = GameObject.FindGameObjectWithTag("Maze");
        if (mazeObj != null) {
            mazeGenerator = mazeObj.GetComponent<MazeGenerator>();
        } else {
            Debug.LogWarning("Maze object with tag 'Maze' not found!");
        }
        
        // Find the TextMeshPro object with tag "LevelText"
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
            // Increment the level counter.
            level++;
            // Update the LevelText UI.
            if (levelText != null) {
                levelText.text = "Level " + level;
            }
            // Reconfigure the maze layout in-place.
            mazeGenerator.ReconfigureMaze();
            hasTriggered = true;
            Destroy(gameObject);
        }
    }
    
    // Optional: Reset the trigger when the player exits, making it reusable.
    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            hasTriggered = false;
        }
    }

    public static void ResetLevel() {
        level = 1;
        GameObject levelTextObj = GameObject.FindGameObjectWithTag("LevelText");
        if (levelTextObj != null) {
            TMP_Text tmp = levelTextObj.GetComponent<TMP_Text>();
            if (tmp != null) {
                tmp.text = "Level " + level;
            }
        }
    }

}
