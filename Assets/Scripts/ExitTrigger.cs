using UnityEngine;
using TMPro;

public class ExitTrigger : MonoBehaviour {
    private MazeGenerator mazeGenerator;
    private bool hasTriggered = false;
    private int level = 1;
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
            hasTriggered = true;
            // Increment the level counter.
            level++;
            // Update the LevelText UI.
            if (levelText != null) {
                levelText.text = "Level " + level;
            }
            // Reconfigure the maze layout in-place.
            mazeGenerator.ReconfigureMaze();
        }
    }
}
