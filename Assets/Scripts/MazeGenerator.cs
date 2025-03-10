using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]
public class MazeGenerator : MonoBehaviour {
    [Header("Maze Settings")]
    public int width = 10;              // Number of cells horizontally
    public int height = 10;             // Number of cells vertically
    public float cellSize = 4f;         // Distance between cell centers

    [Header("Wall Settings")]
    public float wallHeight = 4f;       // Height of the walls
    // Array of possible wall colors: Yellow, Red, Blue, and Green.
    public Color[] wallColors = new Color[] { Color.yellow, Color.red, Color.blue, Color.green };

    [Header("Colors")]
    public Color floorColor = Color.green;

    [Header("Prefabs")]
    public GameObject playerPrefab;     // Assign your player prefab in the Inspector
    public GameObject exitPrefab;       // Assign an exit marker prefab in the Inspector (optional)

    [Header("Enemy Settings")]
    public GameObject enemyPrefab;      // Assign your enemy prefab in the Inspector
    public int enemyCount = 5;          // Number of enemies to spawn

    private Cell[,] cells;

    public Cell[,] Cells {
        get { return cells; }
    }

    void Start() {
        InitializeCells();
        GenerateMaze();
        SetMazeEntrances();   // Remove an outer wall for the entrance and the exit
        BuildMaze();
        CreateFloor();
        SpawnPlayer();
        SpawnExit();          // Optionally spawn an exit marker at the exit cell
        SpawnEnemies();       // Spawn enemies throughout the maze
    }

    // Initialize a grid of cells.
    void InitializeCells() {
        cells = new Cell[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                cells[x, y] = new Cell(x, y);
            }
        }
    }

    // Maze generation using recursive backtracking.
    void GenerateMaze() {
        Stack<Cell> stack = new Stack<Cell>();
        Cell current = cells[0, 0];
        current.visited = true;
        int visitedCells = 1;
        int totalCells = width * height;

        while (visitedCells < totalCells) {
            List<Cell> neighbors = GetUnvisitedNeighbors(current);
            if (neighbors.Count > 0) {
                Cell chosen = neighbors[Random.Range(0, neighbors.Count)];
                RemoveWall(current, chosen);
                stack.Push(current);
                current = chosen;
                current.visited = true;
                visitedCells++;
            } else if (stack.Count > 0) {
                current = stack.Pop();
            }
        }
    }

    List<Cell> GetUnvisitedNeighbors(Cell cell) {
        List<Cell> neighbors = new List<Cell>();
        int x = cell.x;
        int y = cell.y;
        if (y < height - 1 && !cells[x, y + 1].visited) neighbors.Add(cells[x, y + 1]);
        if (y > 0 && !cells[x, y - 1].visited) neighbors.Add(cells[x, y - 1]);
        if (x < width - 1 && !cells[x + 1, y].visited) neighbors.Add(cells[x + 1, y]);
        if (x > 0 && !cells[x - 1, y].visited) neighbors.Add(cells[x - 1, y]);
        return neighbors;
    }

    void RemoveWall(Cell current, Cell neighbor) {
        int xDiff = neighbor.x - current.x;
        int yDiff = neighbor.y - current.y;
        if (xDiff == 1) {
            current.walls[1] = false;
            neighbor.walls[3] = false;
        } else if (xDiff == -1) {
            current.walls[3] = false;
            neighbor.walls[1] = false;
        } else if (yDiff == 1) {
            current.walls[2] = false;
            neighbor.walls[0] = false;
        } else if (yDiff == -1) {
            current.walls[0] = false;
            neighbor.walls[2] = false;
        }
    }

    // Remove the left wall of the starting cell and the top wall of the ending cell.
    void SetMazeEntrances() {
        // Entrance at (0,0): remove left wall (index 3)
        cells[0, 0].walls[3] = false;
        // Exit at (width-1, height-1): remove top wall (index 2)
        cells[width - 1, height - 1].walls[2] = false;
    }

    // Build the maze using Cube primitives and assign colors based on cell area.
    void BuildMaze() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Vector3 cellCenter = new Vector3(x * cellSize, 0, y * cellSize);
                Cell cell = cells[x, y];
                // Get the color based on the quadrant.
                Color groupColor = GetGroupColor(cell);

                // Adjust Y position by half the wall height so the bottom rests on the floor.
                if (cell.walls[0])
                    CreateWall(cellCenter + new Vector3(0, wallHeight / 2, -cellSize / 2), new Vector3(cellSize, wallHeight, 0.5f), groupColor);  // Bottom
                if (cell.walls[1])
                    CreateWall(cellCenter + new Vector3(cellSize / 2, wallHeight / 2, 0), new Vector3(0.5f, wallHeight, cellSize), groupColor);  // Right
                if (cell.walls[2])
                    CreateWall(cellCenter + new Vector3(0, wallHeight / 2, cellSize / 2), new Vector3(cellSize, wallHeight, 0.5f), groupColor);  // Top
                if (cell.walls[3])
                    CreateWall(cellCenter + new Vector3(-cellSize / 2, wallHeight / 2, 0), new Vector3(0.5f, wallHeight, cellSize), groupColor);  // Left
            }
        }
    }

    // Returns a color based on the cell's quadrant.
    Color GetGroupColor(Cell cell) {
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
        // Bottom Left
        if (cell.x < halfWidth && cell.y < halfHeight)
            return wallColors[0];
        // Bottom Right
        else if (cell.x >= halfWidth && cell.y < halfHeight)
            return wallColors[1];
        // Top Left
        else if (cell.x < halfWidth && cell.y >= halfHeight)
            return wallColors[2];
        // Top Right
        else
            return wallColors[3];
    }

    // Create a wall at the given position, scale, and with the given color.
    void CreateWall(Vector3 position, Vector3 scale, Color color) {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.transform.parent = transform;
        wall.tag = "wall";
        
        Renderer wallRenderer = wall.GetComponent<Renderer>();
        if (wallRenderer != null) {
            wallRenderer.material.color = color;
        }
        
        BoxCollider collider = wall.GetComponent<BoxCollider>();
        if(collider == null) {
            collider = wall.AddComponent<BoxCollider>();
        }
    }

    // Spawn the player at the starting cell (cell (0,0))
    void SpawnPlayer() {
        if (playerPrefab != null) {
            Vector3 spawnPos = new Vector3(0, 2, 0); // Adjust height if necessary
            Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        } else {
            Debug.LogWarning("Player Prefab not assigned!");
        }
    }

    // Create a floor to cover the maze so the player doesn't fall continuously.
    void CreateFloor() {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.tag = "floor";
        floor.layer = LayerMask.NameToLayer("Floor");
        
        float centerX = (width * cellSize) / 2 - cellSize / 2;
        float centerZ = (height * cellSize) / 2 - cellSize / 2;
        floor.transform.position = new Vector3(centerX, 0, centerZ);
        
        float scaleX = (width * cellSize) / 10f;
        float scaleZ = (height * cellSize) / 10f;
        floor.transform.localScale = new Vector3(scaleX, 1, scaleZ);

        Renderer floorRenderer = floor.GetComponent<Renderer>();
        if (floorRenderer != null) {
            floorRenderer.material.color = floorColor;
        }
    }

    // Spawn an exit marker at the exit cell (cell (width-1, height-1))
    void SpawnExit() {
        if (exitPrefab != null) {
            Vector3 exitPos = new Vector3((width - 1) * cellSize, 0, (height - 1) * cellSize);
            Instantiate(exitPrefab, exitPos, Quaternion.identity);
        } else {
            Debug.LogWarning("Exit prefab not assigned!");
        }
    }

    // Spawn enemies randomly throughout the maze (excluding cells near the start and exit)
    void SpawnEnemies() {
        if (enemyPrefab == null) {
            Debug.LogWarning("Enemy Prefab not assigned!");
            return;
        }

        List<Vector2Int> validCells = new List<Vector2Int>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                // Exclude cells near the start (0,0) and near the exit (width-1, height-1)
                bool nearStart = (x <= 1 && y <= 1);
                bool nearExit = (x >= width - 2 && y >= height - 2);
                if (nearStart || nearExit)
                    continue;
                validCells.Add(new Vector2Int(x, y));
            }
        }

        // Shuffle the list
        for (int i = 0; i < validCells.Count; i++) {
            Vector2Int temp = validCells[i];
            int randomIndex = Random.Range(i, validCells.Count);
            validCells[i] = validCells[randomIndex];
            validCells[randomIndex] = temp;
        }

        int spawnCount = Mathf.Min(enemyCount, validCells.Count);
        for (int i = 0; i < spawnCount; i++) {
            Vector2Int cellCoords = validCells[i];
            Vector3 enemyPos = new Vector3(cellCoords.x * cellSize, 1, cellCoords.y * cellSize);
            Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
        }
    }
}

// Helper class for maze cells
public class Cell {
    public int x, y;
    public bool visited = false;
    // Walls: 0 = bottom, 1 = right, 2 = top, 3 = left
    public bool[] walls = new bool[] { true, true, true, true };

    public Cell(int _x, int _y) {
        x = _x;
        y = _y;
    }
}
