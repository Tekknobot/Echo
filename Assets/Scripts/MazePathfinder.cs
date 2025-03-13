using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MazeGenerator))]
public class MazePathfinder : MonoBehaviour {
    public MazeGenerator mazeGenerator;    // Reference to the MazeGenerator component.
    public LineRenderer lineRenderer;      // Reference to the LineRenderer to draw the path.
    public float lineYOffset = 0.1f;         // Height offset for the line (above the floor).
    public float lineWidth = 0.2f;           // Width of the line.

    void Start() {
        // If the MazeGenerator reference is not set in the Inspector, try to get it from the same GameObject.
        if (mazeGenerator == null) {
            mazeGenerator = GetComponent<MazeGenerator>();
        }
        // Ensure the MazePathfinder GameObject is a child of the MazeGenerator,
        // so it moves and rotates with the maze.
        transform.SetParent(mazeGenerator.transform);

        // If no LineRenderer is assigned, add one.
        if (lineRenderer == null) {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // Configure the LineRenderer.
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        // Use a basic material.
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.positionCount = 0;

        // Compute the initial path using BFS.
        List<Cell> path = FindPath();
        if (path != null && path.Count > 0) {
            DrawPath(path);
        } else {
            Debug.LogWarning("Path not found!");
        }
    }

    // Use BFS to find a path from the starting cell (0,0) to the exit (width-1, height-1).
    List<Cell> FindPath() {
        int width = mazeGenerator.width;
        int height = mazeGenerator.height;
        Cell[,] cells = mazeGenerator.Cells;
        bool[,] visited = new bool[width, height];
        Cell[,] parent = new Cell[width, height];
        Queue<Cell> queue = new Queue<Cell>();

        // Start at cell (0,0).
        Cell start = cells[0, 0];
        visited[0, 0] = true;
        queue.Enqueue(start);

        // Define the target cell.
        Cell target = cells[width - 1, height - 1];

        while (queue.Count > 0) {
            Cell current = queue.Dequeue();
            if (current == target) {
                break;
            }
            // Get all accessible neighbors (cells that can be reached, meaning no wall between).
            List<Cell> neighbors = GetAccessibleNeighbors(current, cells, width, height);
            foreach (Cell neighbor in neighbors) {
                if (!visited[neighbor.x, neighbor.y]) {
                    visited[neighbor.x, neighbor.y] = true;
                    parent[neighbor.x, neighbor.y] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Reconstruct the path from target to start if it exists.
        if (!visited[target.x, target.y]) {
            Debug.LogWarning("No path found!");
            return null;
        }
        List<Cell> path = new List<Cell>();
        Cell cellTracker = target;
        while (cellTracker != null) {
            path.Add(cellTracker);
            if (cellTracker == start)
                break;
            cellTracker = parent[cellTracker.x, cellTracker.y];
        }
        path.Reverse();
        return path;
    }

    // Get neighbors of the current cell that are reachable (no wall in between).
    List<Cell> GetAccessibleNeighbors(Cell cell, Cell[,] cells, int width, int height) {
        List<Cell> neighbors = new List<Cell>();
        int x = cell.x;
        int y = cell.y;
        // Walls array indices: 0 = bottom, 1 = right, 2 = top, 3 = left.
        // Check bottom neighbor.
        if (!cell.walls[0] && y > 0) {
            neighbors.Add(cells[x, y - 1]);
        }
        // Check right neighbor.
        if (!cell.walls[1] && x < width - 1) {
            neighbors.Add(cells[x + 1, y]);
        }
        // Check top neighbor.
        if (!cell.walls[2] && y < height - 1) {
            neighbors.Add(cells[x, y + 1]);
        }
        // Check left neighbor.
        if (!cell.walls[3] && x > 0) {
            neighbors.Add(cells[x - 1, y]);
        }
        return neighbors;
    }

    // Draw the path using the LineRenderer by converting cell coordinates to world positions.
    // This conversion is done relative to the MazeGenerator's transform position.
    void DrawPath(List<Cell> path) {
        // Use the MazeGenerator's transform position as the origin.
        DrawPath(path, mazeGenerator.transform.position);
    }

    // Overloaded DrawPath method that accepts an origin offset.
    void DrawPath(List<Cell> path, Vector3 origin) {
        int count = path.Count;
        lineRenderer.positionCount = count;
        float cellSize = mazeGenerator.cellSize;
        for (int i = 0; i < count; i++) {
            Cell cell = path[i];
            Vector3 position = origin + new Vector3(cell.x * cellSize, lineYOffset, cell.y * cellSize);
            lineRenderer.SetPosition(i, position);
        }
    }

    // Public method to reconfigure the pathfinder after the maze is reconfigured.
    // This method is called from MazeGenerator.ReconfigureMaze().
    public void Reconfigure(Vector3 newOrigin, Cell[,] newCells) {
        // Recalculate the path using the updated maze cells.
        List<Cell> path = FindPath(); // MazeGenerator.Cells is updated at this point.
        if (path != null && path.Count > 0) {
            DrawPath(path, newOrigin);
        } else {
            Debug.LogWarning("Path not found after reconfiguration!");
        }
    }
}
