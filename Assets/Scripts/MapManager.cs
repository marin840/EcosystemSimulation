using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{

    public GameObject waterTile, sandTile, grassTile;
    public float scale = 1.0F;

    [SerializeField] private int mapWidth = 100, mapLength = 100;
    [SerializeField] private float waterSandLimit = 0.3f, sandGrassLimit = 0.5f;

    [SerializeField] private GameObject[,] map;
    [SerializeField] private GameObject[] trees;

    public GameObject allTrees;
    public GameObject tree;
    public int numOfTrees;
    void Start()
    {
        map = new GameObject[mapWidth, mapLength];
        trees = new GameObject[numOfTrees];
        
        for (int i = 0; i < mapLength; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                float xCoord = (float)i / mapWidth * scale;
                float yCoord = (float)j / mapLength * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                
                if (sample < waterSandLimit)
                {
                    map[i,j] = Instantiate(waterTile, new Vector3(i,0,j), Quaternion.identity);
                }
                if (sample >= waterSandLimit && sample < sandGrassLimit)
                {
                    map[i,j] = Instantiate(sandTile, new Vector3(i,0,j), Quaternion.identity);
                }
                if (sample > sandGrassLimit)
                {
                    map[i,j] = Instantiate(grassTile, new Vector3(i,0,j), Quaternion.identity);
                }

                // if (sample < waterSandLimit - 0.10 || sample > sandGrassLimit + 0.15)
                // {
                //     Color currentColor = map[i, j].GetComponent<Renderer>().material.color;
                //     Color newColor = new Color(currentColor.r - darkeningAmount, currentColor.g - darkeningAmount, currentColor.b - darkeningAmount);
                //     map[i,j].GetComponent<Renderer>().material.SetColor("_Color", newColor);
                // }
                
                Color currentColor = map[i, j].GetComponent<Renderer>().material.color;
                float mixColor = 0.5f - sample;

                if (map[i, j].CompareTag("Grass") || map[i,j].CompareTag("Sand"))
                {
                    Transform tileTransform = map[i, j].GetComponent<Transform>();
                    Vector3 heightVec = tileTransform.localScale;
                    heightVec.y *= 10 * sample;
                    tileTransform.localScale = heightVec;
                }
                
                if (map[i, j].CompareTag("Water"))
                    mixColor = (sample - 0.15f) * 2;

                if (map[i, j].CompareTag("Sand"))
                    mixColor  = (0.4f - sample) * 2;
                
                
                Color newColor = new Color(currentColor.r + mixColor, currentColor.g + mixColor, currentColor.b + mixColor);
                map[i,j].GetComponent<Renderer>().material.SetColor("_Color", newColor);

                map[i, j].transform.parent = gameObject.transform;
            }
        }

        int itemsPlaced = 0;
        while(itemsPlaced < numOfTrees)
        {
            int randX = Random.Range(0, 100);
            int randY = Random.Range(0, 100);

            if (map[randX, randY].CompareTag("Grass"))
            {
                trees[itemsPlaced] = Instantiate(tree, new Vector3(randX,4,randY), Quaternion.identity);
                trees[itemsPlaced].transform.parent = allTrees.transform;
                itemsPlaced++;
            }
        }
    }
    
}
