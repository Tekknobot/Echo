using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class StarMeshGenerator : MonoBehaviour
{
    [Header("Star Parameters")]
    [Tooltip("Number of star points (a 5-point star will have 10 vertices in 2D)")]
    public int starPoints = 5;
    [Tooltip("Outer radius for the star points")]
    public float outerRadius = 1f;
    [Tooltip("Inner radius for the star recesses (a good default for a 5-point star is ~0.382)")]
    public float innerRadius = 0.382f;
    [Tooltip("Thickness (depth) of the star")]
    public float depth = 0.2f;

    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 90f;

    void Start()
    {
        GenerateStar();
        // Automatically add a rotation component to the star.
        RotateYaxis rotator = gameObject.AddComponent<RotateYaxis>();
        rotator.rotationSpeed = rotationSpeed;
    }

    void GenerateStar()
    {
        int vertexCount2D = starPoints * 2;
        Vector2[] polyPoints = new Vector2[vertexCount2D];

        // Create 2D points for the star outline.
        float angleStep = 360f / vertexCount2D;
        float angle = 90f; // Start at 90° so the top is an outer point.

        for (int i = 0; i < vertexCount2D; i++)
        {
            float rad = Mathf.Deg2Rad * angle;
            float radius = (i % 2 == 0) ? outerRadius : innerRadius;
            polyPoints[i] = new Vector2(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius);
            angle += angleStep;
        }

        // Triangulate the 2D shape (front face) using an ear clipping algorithm.
        Triangulator triangulator = new Triangulator(polyPoints);
        int[] indices2D = triangulator.Triangulate();

        // Build vertices for front and back faces.
        Vector3[] vertices = new Vector3[vertexCount2D * 2];
        for (int i = 0; i < vertexCount2D; i++)
        {
            // Front face vertex (z = depth/2).
            vertices[i] = new Vector3(polyPoints[i].x, polyPoints[i].y, depth / 2f);
            // Back face vertex (z = -depth/2).
            vertices[i + vertexCount2D] = new Vector3(polyPoints[i].x, polyPoints[i].y, -depth / 2f);
        }

        List<int> triangles = new List<int>();

        // Front face triangles (using triangulated indices).
        for (int i = 0; i < indices2D.Length; i += 3)
        {
            triangles.Add(indices2D[i]);
            triangles.Add(indices2D[i + 1]);
            triangles.Add(indices2D[i + 2]);
        }

        // Back face triangles (reverse winding order so the normals point out).
        for (int i = 0; i < indices2D.Length; i += 3)
        {
            triangles.Add(indices2D[i + 2] + vertexCount2D);
            triangles.Add(indices2D[i + 1] + vertexCount2D);
            triangles.Add(indices2D[i] + vertexCount2D);
        }

        // Side faces: create quads for each edge connecting front and back.
        for (int i = 0; i < vertexCount2D; i++)
        {
            int next = (i + 1) % vertexCount2D;
            int currentFront = i;
            int nextFront = next;
            int nextBack = next + vertexCount2D;
            int currentBack = i + vertexCount2D;

            // First triangle of the quad.
            triangles.Add(currentFront);
            triangles.Add(nextFront);
            triangles.Add(nextBack);

            // Second triangle of the quad.
            triangles.Add(currentFront);
            triangles.Add(nextBack);
            triangles.Add(currentBack);
        }

        // Create and assign the mesh.
        Mesh mesh = new Mesh();
        mesh.name = "LowPolyStar";
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    // A simple ear clipping triangulator for 2D polygons.
    public class Triangulator
    {
        private List<Vector2> m_points;

        public Triangulator(Vector2[] points)
        {
            m_points = new List<Vector2>(points);
        }

        public int[] Triangulate()
        {
            List<int> indices = new List<int>();

            int n = m_points.Count;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (Area() > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int m = 0, v = nv - 1; nv > 2; )
            {
                if ((count--) <= 0)
                    break; // Polygon is probably non-simple.

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(u, v, w, nv, V))
                {
                    int a = V[u];
                    int b = V[v];
                    int c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    for (int s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }
            return indices.ToArray();
        }

        private float Area()
        {
            int n = m_points.Count;
            float A = 0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = m_points[p];
                Vector2 qval = m_points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return A * 0.5f;
        }

        private bool Snip(int u, int v, int w, int n, int[] V)
        {
            Vector2 A = m_points[V[u]];
            Vector2 B = m_points[V[v]];
            Vector2 C = m_points[V[w]];

            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;

            for (int p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = m_points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax = C.x - B.x;
            float ay = C.y - B.y;
            float bx = A.x - C.x;
            float by = A.y - C.y;
            float cx = B.x - A.x;
            float cy = B.y - A.y;
            float apx = P.x - A.x;
            float apy = P.y - A.y;
            float bpx = P.x - B.x;
            float bpy = P.y - B.y;
            float cpx = P.x - C.x;
            float cpy = P.y - C.y;
            float aCROSSbp = ax * bpy - ay * bpx;
            float cCROSSap = cx * apy - cy * apx;
            float bCROSScp = bx * cpy - by * cpx;
            return (aCROSSbp >= 0f) && (bCROSScp >= 0f) && (cCROSSap >= 0f);
        }
    }
}
