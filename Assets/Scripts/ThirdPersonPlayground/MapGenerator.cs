using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] int size = 100;
    [SerializeField] float scale = .1f;
    [SerializeField] float waterLevel = .4f;
    [SerializeField] Material terrainMaterial;
    [SerializeField] Material edgeMaterial;

    [SerializeField] GameObject[] treePrefabs;
    [SerializeField] float treeNoiseScale = .05f;
    [SerializeField] float treeDensity = .5f;

    [SerializeField] float riverNoiseScale = .06f;
    [SerializeField] int rivers = 5;

    Cell[,] grid;

    private void Start()
    {
        float[,] noiseMap = new float[size, size];
        float xOffset = Random.Range(-10000f, 10000f);
        float yOffset = Random.Range(-10000f, 10000f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                noiseMap[x, y] = Mathf.PerlinNoise(x * scale + xOffset, y * scale + yOffset);
            }
        }

        float[,] falloffMap = new float[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float xv = x / (float)size * 2 - 1;
                float yv = y / (float)size * 2 - 1;
                float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
                falloffMap[x, y] = Mathf.Pow(v, 3f) / (Mathf.Pow(v, 3f) + Mathf.Pow(2.2f - 2.2f * v, 3f));
            }
        }

        grid = new Cell[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                grid[x, y] = new Cell((noiseMap[x, y] - falloffMap[x, y]) < waterLevel);
            }
        }

        GenerateRivers(grid);
        DrawTerrainMesh(grid);
        DrawEdgeMesh(grid);
        DrawTexture(grid);
        GenerateTrees(grid);
    }

    void GenerateRivers(Cell[,] grid)
    {
        float[,] noiseMap = new float[size, size];
        float xOffset = Random.Range(-10000f, 10000f);
        float yOffset = Random.Range(-10000f, 10000f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                noiseMap[x, y] = Mathf.PerlinNoise(x * riverNoiseScale + xOffset, y * riverNoiseScale + yOffset);
            }
        }

        GridGraph gg = AstarData.active.graphs[0] as GridGraph;
        gg.center = new Vector3(size / 2f - .5f, 0, size / 2f - .5f);
        gg.SetDimensions(size, size, 1);
        AstarData.active.Scan(gg);
        AstarData.active.AddWorkItem(new AstarWorkItem(ctx =>
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    GraphNode node = gg.GetNode(x, y);
                    node.Walkable = noiseMap[x, y] > waterLevel;
                }
            }
        }));
        AstarData.active.FlushGraphUpdates();

        int k = 0;
        for (int i = 0; i < rivers; i++)
        {
            GraphNode start = gg.nodes[Random.Range(16, size - 16)];
            GraphNode end = gg.nodes[Random.Range(size * (size - 1) + 16, size * size - 16)];
            ABPath path = ABPath.Construct((Vector3)start.position, (Vector3)end.position, (Path p) =>
            {
                for (int j = 0; j < p.path.Count; j++)
                {
                    GraphNode node = p.path[j];
                    int x = Mathf.RoundToInt(((Vector3)node.position).x);
                    int y = Mathf.RoundToInt(((Vector3)node.position).z);
                    grid[x, y]._isWater = true;
                }
                k++;
            });
            AstarPath.StartPath(path);
            AstarPath.BlockUntilCalculated(path);
        }
    }

    void DrawTerrainMesh(Cell[,] grid)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                float xCord = -size * .5f + x;
                float yCord = -size * .5f + y;
                if (!cell._isWater)
                {
                    Vector3 a = new Vector3(xCord - .5f, 0, yCord + .5f);
                    Vector3 b = new Vector3(xCord + .5f, 0, yCord + .5f);
                    Vector3 c = new Vector3(xCord - .5f, 0, yCord - .5f);
                    Vector3 d = new Vector3(xCord + .5f, 0, yCord - .5f);
                    Vector2 uvA = new Vector2(xCord / (float)size, yCord / (float)size);
                    Vector2 uvB = new Vector2((xCord + 1) / (float)size, yCord / (float)size);
                    Vector2 uvC = new Vector2(xCord / (float)size, (yCord + 1) / (float)size);
                    Vector2 uvD = new Vector2((xCord + 1) / (float)size, (yCord + 1) / (float)size);
                    Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                    Vector2[] uv = new Vector2[] { uvA, uvB, uvC, uvB, uvD, uvC };
                    for (int k = 0; k < 6; k++)
                    {
                        vertices.Add(v[k]);
                        triangles.Add(triangles.Count);
                        uvs.Add(uv[k]);
                    }
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshCollider>();
    }

    void DrawEdgeMesh(Cell[,] grid)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                float xCord = -size * .5f + x;
                float yCord = -size * .5f + y;
                if (!cell._isWater)
                {
                    if (x > 0)
                    {
                        Cell left = grid[x - 1, y];
                        if (left._isWater)
                        {
                            Vector3 a = new Vector3(xCord - .5f, 0, yCord + .5f);
                            Vector3 b = new Vector3(xCord - .5f, 0, yCord - .5f);
                            Vector3 c = new Vector3(xCord - .5f, -1, yCord + .5f);
                            Vector3 d = new Vector3(xCord - .5f, -1, yCord - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (x < size - 1)
                    {
                        Cell right = grid[x + 1, y];
                        if (right._isWater)
                        {
                            Vector3 a = new Vector3(xCord + .5f, 0, yCord + .5f);
                            Vector3 b = new Vector3(xCord + .5f, 0, yCord - .5f);
                            Vector3 c = new Vector3(xCord + .5f, -1, yCord + .5f);
                            Vector3 d = new Vector3(xCord + .5f, -1, yCord - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (y > 0)
                    {
                        Cell down = grid[x, y - 1];
                        if (down._isWater)
                        {
                            Vector3 a = new Vector3(xCord - .5f, 0, yCord - .5f);
                            Vector3 b = new Vector3(xCord + .5f, 0, yCord - .5f);
                            Vector3 c = new Vector3(xCord - .5f, -1, yCord - .5f);
                            Vector3 d = new Vector3(xCord + .5f, -1, yCord - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (y < size - 1)
                    {
                        Cell up = grid[x, y + 1];
                        if (up._isWater)
                        {
                            Vector3 a = new Vector3(xCord - .5f, 0, yCord + .5f);
                            Vector3 b = new Vector3(xCord + .5f, 0, yCord + .5f);
                            Vector3 c = new Vector3(xCord - .5f, -1, yCord + .5f);
                            Vector3 d = new Vector3(xCord + .5f, -1, yCord + .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        GameObject edgeObj = new GameObject("Edge");
        edgeObj.transform.SetParent(transform);

        MeshFilter meshFilter = edgeObj.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = edgeObj.AddComponent<MeshRenderer>();
        meshRenderer.material = edgeMaterial;
        edgeObj.AddComponent<MeshCollider>();
    }

    void DrawTexture(Cell[,] grid)
    {
        Texture2D texture = new Texture2D(size, size);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                int xCord = (int)(-size * .5f + x);
                int yCord = (int)(-size * .5f + y);
                if (cell._isWater)
                {
                    texture.SetPixel(xCord, yCord, Color.blue);
                } else
                {
                    texture.SetPixel(xCord, yCord, Color.green);
                }
            }
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.material = terrainMaterial;
            meshRenderer.material.mainTexture = texture;
        }
    }

    void GenerateTrees(Cell[,] grid)
    {
        float[,] noiseMap = new float[size, size];
        (float xOffset, float yOffset) = (Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x * treeNoiseScale + xOffset, y * treeNoiseScale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                float xCord = -size * .5f + x;
                float yCord = -size * .5f + y;
                if (!cell._isWater)
                {
                    float v = Random.Range(0f, treeDensity);
                    if (noiseMap[x, y] < v)
                    {
                        GameObject prefab = treePrefabs[0];
                        GameObject tree = Instantiate(prefab, transform);
                        tree.transform.position = new Vector3(xCord, 0, yCord);
                        tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                        tree.transform.localScale = Vector3.one * Random.Range(.8f, 1.2f);
                    }
                }
            }
        }
    }
}
