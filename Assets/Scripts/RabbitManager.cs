using System.Collections;
using System.Collections.Generic;
using AnimalBehaviour;
using UnityEngine;

public class RabbitManager : MonoBehaviour
{

    public int initalNumberOfRabbits;
    public GameObject rabbitMalePrefab, rabbitFemalePrefab;
    public List<GameObject> rabbitPopulation;

    private Vector3[] vertices;

    public GameObject allRabbits;
    public float rabbitSpawningLimit = 0.35f;

    public int hungerDeaths, thirstDeaths, foxKills;
    public float avgSpeed, avgSense, avgSize;
    
    private static RabbitManager _instance;
    public static RabbitManager Instance => _instance;
    
    
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
        rabbitPopulation = new List<GameObject>();
        
        vertices = MeshGenerator.Instance.GetVertices();

        int rabbitsPlaced = 0;
        while(rabbitsPlaced < initalNumberOfRabbits)
        {
            int rand = Random.Range(0, vertices.Length);

            if (vertices[rand].y > rabbitSpawningLimit)
            {
                GameObject go;
                
                if (rabbitsPlaced < initalNumberOfRabbits / 2)
                {
                    go = Instantiate(rabbitMalePrefab, vertices[rand], Quaternion.identity);
                    go.GetComponent<AnimalStateManager>().isMale = true;
                }
                else
                {
                    go = Instantiate(rabbitFemalePrefab, vertices[rand], Quaternion.identity);
                    go.GetComponent<AnimalStateManager>().isMale = false;
                }
                
                AnimalStateManager rabbit = go.GetComponent<AnimalStateManager>();
                rabbit.AnimalSpeed *= Random.Range(0.9f, 1.1f);
                rabbit.SenseRange *= Random.Range(0.9f, 1.1f);
                
                float sizeChange = Random.Range(-5, 6)/100f;
                go.transform.localScale += new Vector3(sizeChange, sizeChange, sizeChange);

                go.transform.Rotate(0, Random.Range(0,360), 0);
                rabbitPopulation.Add(go);
                go.transform.parent = allRabbits.transform;
                rabbitsPlaced++;
            }
        }
    }

    public void CreateNewRabbit(bool isMale, float speed, float senseRange, float size, Vector3 position)
    {
        GameObject go;

        if(isMale)
            go = Instantiate(rabbitMalePrefab, position, Quaternion.identity);
        else
            go = Instantiate(rabbitFemalePrefab, position, Quaternion.identity);

        AnimalStateManager rabbit = go.GetComponent<AnimalStateManager>();
        rabbit.isUnderDeveloped = true;
        rabbit.AnimalSpeed = speed/2;
        rabbit.SenseRange = senseRange/2;
        rabbit.transform.localScale = new Vector3(size/2, size/2, size/2);

        if (isMale)
            rabbit.isMale = true;

        go.transform.Rotate(0, Random.Range(0,360), 0);
        rabbitPopulation.Add(go);
        go.transform.parent = allRabbits.transform;
    }

    public void RabbitDies(GameObject rabbit, string type)
    {
        if (type == "hunger")
            hungerDeaths++;
        if (type == "thirst")
            thirstDeaths++;
        if (type == "fox")
            foxKills++;

        rabbitPopulation.Remove(rabbit);
    }

    public void CalculateAverageParameters()
    {
        float sumSpeed = 0, sumSense = 0, sumSize = 0;
        int n = rabbitPopulation.Count;

        foreach (GameObject go in rabbitPopulation)
        {
            AnimalStateManager rabbit = go.GetComponent<AnimalStateManager>();
            sumSpeed += rabbit.AnimalSpeed;
            sumSense += rabbit.SenseRange;
            sumSize += rabbit.transform.localScale.x;
        }
        
        avgSpeed = sumSpeed / n;
        avgSense = sumSense / n;
        avgSize = sumSize / n;
    }
}
