using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [Range(0.5f, 4)] public float simulationSpeed = 1;
    
    private string rabbitPopulationSizeData = "C:/Users/Marin/Desktop/rabbitPopulationSizeData.txt";
    private string foxPopulationSizeData = "C:/Users/Marin/Desktop/foxPopulationSizeData.txt";
    private string rabbitParametersData = "C:/Users/Marin/Desktop/rabbitParametersData.txt";
    private int iteration = 0;

    private RabbitManager rm;

    // Start is called before the first frame update
    void Start()
    {
        rm = RabbitManager.Instance;
        File.AppendAllText(rabbitPopulationSizeData, iteration++ + "\t" + rm.initalNumberOfRabbits + Environment.NewLine);
        File.AppendAllText(foxPopulationSizeData, iteration++ + "\t" + FoxManager.Instance.foxPopulation.Count + Environment.NewLine);
        StartCoroutine(WriteToFile());
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = simulationSpeed;
    }

    private IEnumerator WriteToFile()
    {
        yield return new WaitForSeconds(3);
        File.AppendAllText(rabbitPopulationSizeData, iteration++ + "\t" + RabbitManager.Instance.rabbitPopulation.Count + Environment.NewLine);
        File.AppendAllText(foxPopulationSizeData, iteration++ + "\t" + FoxManager.Instance.foxPopulation.Count + Environment.NewLine);
        if (rm.rabbitPopulation.Count > 0)
        {
            rm.CalculateAverageParameters();
            File.AppendAllText(rabbitParametersData,
                iteration++ + "\t" + rm.avgSpeed + "\t" + rm.avgSense + "\t" + rm.avgSize + Environment.NewLine);
        }

        if (RabbitManager.Instance.rabbitPopulation.Count > 0 || FoxManager.Instance.foxPopulation.Count > 0)
            StartCoroutine(WriteToFile());
    }
}
