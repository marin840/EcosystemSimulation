//using System;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[RequireComponent((typeof(MeshFilter)))]
public class MeshGenerator : MonoBehaviour
{
    private Mesh mesh, colliderMesh;
    [SerializeField] private Material _material;
    private Vector3[] vertices, colliderVertices;
    private Vector2[] uvs;
    private int[] triangles, colliderTriangles;

    public int height = 100;
    public int width = 100;
    
    private Texture2D texture;

    public int treeTypes = 1;
    public GameObject[] treeModels;
    public int numOfTrees;
    public GameObject allTrees;

    public int decorationTypes;
    public GameObject[] decorations;
    public int numOfDecorations;
    public GameObject allDecorations;
    
    public GameObject[] sedgesModel;
    public int numOfSedges;
    public GameObject allSedges;
    
    private static MeshGenerator _instance;
    public static MeshGenerator Instance => _instance;

    private void Awake()
    {
        vertices = new Vector3[(height+1)*(width+1)];
        colliderVertices = new Vector3[(height+1)*(width+1)];
        
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        texture = new Texture2D(width+1, height+1, TextureFormat.RGBA32, false);
        mesh = new Mesh();
        colliderMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();

        GenerateEnvironment(treeModels, numOfTrees, allTrees, 0.5f, 1f);
        GenerateEnvironment(decorations, numOfDecorations, allDecorations, 0.5f, 1f);
        GenerateEnvironment(sedgesModel, numOfSedges, allSedges, 0.3f, 0.4f);
        
        UpdateMesh();
    }

    void CreateShape()
    {
        uvs = new Vector2[(height+1)*(width+1)];
        
        int newNoise = Random.Range(0,10000);
        List<int> waterIndices = new List<int>();

        for (int z = 0, i = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                float y = Mathf.PerlinNoise(x*0.05f + newNoise/10f, z*0.05f + newNoise/10f);
                vertices[i] = new Vector3(x*10.0f/width, y, z*10.0f/height);
                colliderVertices[i] = new Vector3(x*10.0f/width, y, z*10.0f/height);
                //
                if (colliderVertices[i].y < 0.33)
                {
                    colliderVertices[i] += Vector3.up*10;
                    waterIndices.Add(i);
                }
                //
                uvs[i] = new Vector2((float)x / width, (float)z / height);
                texture.SetPixel(x, z, new Color(y, y, y, 1));
                i++;
            }
        }

        AdjustCollider(waterIndices);
        
        texture.Apply();
        _material.SetTexture("_NoiseTex", texture);

        triangles = new int[width * height * 6];
        colliderTriangles = new int[width * height * 6];
        
        int numOfVertices = 0;
        int numOfTriangles = 0;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[numOfTriangles + 0] = numOfVertices + 0;
                triangles[numOfTriangles + 1] = numOfVertices + width + 1;
                triangles[numOfTriangles + 2] = numOfVertices + 1;
                triangles[numOfTriangles + 3] = numOfVertices + 1;
                triangles[numOfTriangles + 4] = numOfVertices + width + 1;
                triangles[numOfTriangles + 5] = numOfVertices + width + 2;

                numOfVertices++;
                numOfTriangles += 6;
            }

            numOfVertices++;
        }

        colliderTriangles = triangles;
    }

    private void GenerateEnvironment(GameObject[] models, int number, GameObject parent, float low, float high)
    {
        int itemsPlaced = 0;
        while(itemsPlaced < number)
        {
            int rand = Random.Range(0, vertices.Length);

            if (vertices[rand].y > low && vertices[rand].y < high)
            {
                GameObject go = Instantiate(models[Random.Range(0, models.Length)], vertices[rand], Quaternion.identity);
                go.transform.Rotate(0, Random.Range(0,360), 0);
                go.transform.parent = parent.transform;
                itemsPlaced++;
            }
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        colliderMesh.Clear();
        colliderMesh.vertices = colliderVertices;
        colliderMesh.triangles = colliderTriangles;
        colliderMesh.uv = uvs;
        
        GetComponent<MeshCollider>().sharedMesh = colliderMesh;
        mesh.RecalculateNormals();
        colliderMesh.RecalculateNormals();
    }

    public Vector3[] GetVertices()
    {
        return vertices;
    }

    void AdjustCollider(List<int> waterIndices)
    {
        foreach(int index in waterIndices)
        {
            List<int> neighbors = CalculateNeighbors(index);
            Vector3 deltaVec = CalculateDeltaVector(index, neighbors);
            colliderVertices[index] += deltaVec;
        }
    }

    List<int> CalculateNeighbors(int index)
    {
        int width = this.width + 1;
        List<int> neighbors = new List<int>();
        if (index % width != 0)
        {
            neighbors.Add(index - 1);
        }

        if (index < colliderVertices.Length && (index + 1) % width != 0)
        {
            neighbors.Add(index + 1);
        }

        if (index >= width)
        {
            neighbors.Add(index - width);
        }

        if (index + width < colliderVertices.Length)
        {
            neighbors.Add(index + width);
        }
        
        return neighbors;
    }

    Vector3 CalculateDeltaVector(int index, List<int> neighbors)
    {
        Vector3 deltaVec = Vector3.zero;
        foreach (int i in neighbors)
        {
            if (vertices[i].y > 0.35f)
            {
                deltaVec += vertices[i] - vertices[index];
            }
        }

        return new Vector3(deltaVec.x, 0, deltaVec.z) * 2;
    }
    
    /*private void OnDrawGizmos()
    {
        foreach (var i in debugList)
        {
            Gizmos.DrawSphere(colliderVertices[i], 0.05f);
        }
    }*/
}


