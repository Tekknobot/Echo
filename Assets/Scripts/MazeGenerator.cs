using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]
public class MazeGenerator : MonoBehaviour {
    [Header("Maze Settings")]
    public int width = 10;
    public int height = 10;
    public float cellSize = 4f;

    [Header("Wall Settings")]
    public float wallHeight = 4f;
    public Color[] wallColors = new Color[] { Color.yellow, Color.red, Color.blue, Color.green };

    [Header("Colors")]
    public Color floorColor = Color.green;

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject exitPrefab;

    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public int enemyCount = 5;
    
    [Header("Materials")]
    // Predefined material for maze walls and other elements.
    public Material mazeElementMaterial;

    [Header("Floor Material")]
    // Assign a material with your desired floor texture in the Inspector.
    public Material floorMaterial;

    private Cell[,] cells;

    public Cell[,] Cells {
        get { return cells; }
    }

    void Start() {
        InitializeCells();
        GenerateMaze();
        SetMazeEntrances();
        BuildMaze();
        CreateFloor();
        SpawnPlayer();
        SpawnExit();
        SpawnEnemies();
    }

    void InitializeCells() {
        cells = new Cell[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                cells[x, y] = new Cell(x, y);
            }
        }
    }

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

    void SetMazeEntrances() {
        cells[0, 0].walls[3] = false;
        cells[width - 1, height - 1].walls[2] = false;
    }

    void BuildMaze() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Vector3 cellCenter = new Vector3(x * cellSize, 0, y * cellSize);
                Cell cell = cells[x, y];
                Color groupColor = GetGroupColor(cell);

                if (cell.walls[0])
                    CreateWall(cellCenter + new Vector3(0, wallHeight / 2, -cellSize / 2), new Vector3(cellSize, wallHeight, 0.5f), groupColor);
                if (cell.walls[1])
                    CreateWall(cellCenter + new Vector3(cellSize / 2, wallHeight / 2, 0), new Vector3(0.5f, wallHeight, cellSize), groupColor);
                if (cell.walls[2])
                    CreateWall(cellCenter + new Vector3(0, wallHeight / 2, cellSize / 2), new Vector3(cellSize, wallHeight, 0.5f), groupColor);
                if (cell.walls[3])
                    CreateWall(cellCenter + new Vector3(-cellSize / 2, wallHeight / 2, 0), new Vector3(0.5f, wallHeight, cellSize), groupColor);
            }
        }
    }

    Color GetGroupColor(Cell cell) {
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
        if (cell.x < halfWidth && cell.y < halfHeight)
            return wallColors[0];
        else if (cell.x >= halfWidth && cell.y < halfHeight)
            return wallColors[1];
        else if (cell.x < halfWidth && cell.y >= halfHeight)
            return wallColors[2];
        else
            return wallColors[3];
    }

    // Use the predefined material for maze elements.
    void CreateWall(Vector3 position, Vector3 scale, Color color) {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.transform.parent = transform;
        wall.tag = "wall";

        Renderer wallRenderer = wall.GetComponent<Renderer>();
        if (wallRenderer != null && mazeElementMaterial != null) {
            // Create an instance of the predefined material.
            Material instanceMat = Instantiate(mazeElementMaterial);
            instanceMat.color = color;
            wallRenderer.material = instanceMat;
        }

        if (wall.GetComponent<BoxCollider>() == null)
            wall.AddComponent<BoxCollider>();
    }

    void SpawnPlayer() {
        if (playerPrefab != null) {
            Vector3 spawnPos = new Vector3(0, 2, 0);
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            player.transform.rotation = Quaternion.LookRotation(Vector3.left);
        } else {
            Debug.LogWarning("Player Prefab not assigned!");
        }
    }

    // Updated CreateFloor: Use a floor material with a texture if assigned.
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
            Material floorMat;
            // Use the assigned floor material with texture if available.
            if (floorMaterial != null) {
                floorMat = Instantiate(floorMaterial);
            } else {
                floorMat = new Material(Shader.Find("Standard"));
            }
            floorMat.color = floorColor;
            // Adjust texture tiling so it scales based on maze size (which is affected by cellSize).
            floorMat.mainTextureScale = new Vector2((width * cellSize) / 10f, (height * cellSize) / 10f);
            floorRenderer.material = floorMat;
        }
    }


    void SpawnExit() {
        if (exitPrefab != null) {
            Vector3 exitPos = new Vector3((width - 1) * cellSize, 0, (height - 1) * cellSize);
            Instantiate(exitPrefab, exitPos, Quaternion.identity);
        } else {
            Debug.LogWarning("Exit prefab not assigned!");
        }
    }

    void SpawnEnemies() {
        if (enemyPrefab == null) {
            Debug.LogWarning("Enemy Prefab not assigned!");
            return;
        }

        List<Vector2Int> validCells = new List<Vector2Int>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                bool nearStart = (x <= 1 && y <= 1);
                bool nearExit = (x >= width - 2 && y >= height - 2);
                if (nearStart || nearExit)
                    continue;
                validCells.Add(new Vector2Int(x, y));
            }
        }

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

public class Cell {
    public int x, y;
    public bool visited = false;
    public bool[] walls = new bool[] { true, true, true, true };

    public Cell(int _x, int _y) {
        x = _x;
        y = _y;
    }
}
