using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlantsManager : MonoBehaviour
{

    public GameObject plantsPrefab;
    public int initialNumOfPlants;
    public GameObject allPlants;
    
    private Vector3[] vertices;
    public float grassSpawningLimit = 0.5f;

    public float minSpawnDelay = 4f, maxSpawnDelay = 12f;
    public List<GameObject> plantsPopulation;

    private static PlantsManager _instance;
    public static PlantsManager Instance => _instance;

    public LayerMask decorationLayer;
    private void Awake()
    {
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
        vertices = MeshGenerator.Instance.GetVertices();
        plantsPopulation = new List<GameObject>();
        
        int plantsPlaced = 0;
        while(plantsPlaced < initialNumOfPlants)
        {
            int rand = Random.Range(0, vertices.Length);

            if (vertices[rand].y > grassSpawningLimit)
            {
                GameObject go = Instantiate(plantsPrefab, vertices[rand], Quaternion.identity);
                go.transform.Rotate(0, Random.Range(0,360), 0);
                go.transform.parent = allPlants.transform;
                plantsPlaced++;
                plantsPopulation.Add(go);
                checkPlantPosition(go);
            }
        }

        StartCoroutine(GrowAPlant());
    }

    IEnumerator GrowAPlant()
    {
        yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
 
        int rand = Random.Range(0, vertices.Length);
        while (vertices[rand].y < grassSpawningLimit)
        {
            rand = Random.Range(0, vertices.Length);
        }
        GameObject go = Instantiate(plantsPrefab, vertices[rand], Quaternion.identity);
        go.transform.Rotate(0, Random.Range(0,360), 0);
        go.transform.parent = allPlants.transform;
        plantsPopulation.Add(go);
        checkPlantPosition(go);

        StartCoroutine(GrowAPlant());
    }

    public void EatPlant(GameObject plant)
    {
        plantsPopulation.Remove(plant);
    }

    void checkPlantPosition(GameObject plant)
    {
        RaycastHit hit;
        if (Physics.Raycast(plant.transform.position + Vector3.up * 10, Vector3.down, out hit, 20f, decorationLayer))
        {
            plantsPopulation.Remove(plant);
            Destroy(plant);
        }
    }
}
