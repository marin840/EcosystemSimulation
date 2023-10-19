using System.Collections;
using System.Collections.Generic;
using AnimalBehaviour;
using UnityEngine;

public class FoxManager : MonoBehaviour
{
    public int initalNumberOfFoxes;
    public GameObject foxMalePrefab, foxFemalePrefab;
    public List<GameObject> foxPopulation;

    private Vector3[] vertices;

    public GameObject allFoxes;
    public float foxSpawningLimit = 0.4f;

    public int hungerDeaths, thirstDeaths;
    
    private static FoxManager _instance;
    public static FoxManager Instance => _instance;

    public int spawnDelay;
    
    
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
        StartCoroutine(InitialFoxesSpawn());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator InitialFoxesSpawn()
    {
        yield return new WaitForSeconds(spawnDelay);
        
        foxPopulation = new List<GameObject>();
        
        vertices = MeshGenerator.Instance.GetVertices();
        
        int foxesPlaced = 0;
        while(foxesPlaced < initalNumberOfFoxes)
        {
            int rand = Random.Range(0, vertices.Length);

            if (vertices[rand].y > foxSpawningLimit)
            {
                GameObject go;
                
                if (foxesPlaced < initalNumberOfFoxes / 2)
                {
                    go = Instantiate(foxMalePrefab, vertices[rand], Quaternion.identity);
                    go.GetComponent<AnimalStateManager>().isMale = true;
                }
                else
                {
                    go = Instantiate(foxFemalePrefab, vertices[rand], Quaternion.identity);
                    go.GetComponent<AnimalStateManager>().isMale = false;
                }
                
                AnimalStateManager fox = go.GetComponent<AnimalStateManager>();
                fox.AnimalSpeed *= Random.Range(0.9f, 1.1f);
                fox.SenseRange *= Random.Range(0.9f, 1.1f);
                
                float sizeChange = Random.Range(-5, 6)/100f;
                go.transform.localScale += new Vector3(sizeChange, sizeChange, sizeChange);
                
                

                go.transform.Rotate(0, Random.Range(0,360), 0);
                foxPopulation.Add(go);
                go.transform.parent = allFoxes.transform;
                foxesPlaced++;
            }
        }
    }
    
    public void CreateNewFox(bool isMale, float speed, float senseRange, float size, Vector3 position)
    {
        GameObject go;

        if(isMale)
            go = Instantiate(foxMalePrefab, position, Quaternion.identity);
        else
            go = Instantiate(foxFemalePrefab, position, Quaternion.identity);

        AnimalStateManager rabbit = go.GetComponent<AnimalStateManager>();
        rabbit.isUnderDeveloped = true;
        rabbit.AnimalSpeed = speed/2;
        rabbit.SenseRange = senseRange/2;
        rabbit.transform.localScale = new Vector3(size/2, size/2, size/2);

        if (isMale)
            rabbit.isMale = true;

        go.transform.Rotate(0, Random.Range(0,360), 0);
        foxPopulation.Add(go);
        go.transform.parent = allFoxes.transform;
    }
    
    public void FoxDies(GameObject rabbit, string type)
    {
        if (type == "hunger")
            hungerDeaths++;
        if (type == "thirst")
            thirstDeaths++;

        foxPopulation.Remove(rabbit);
    }
}
