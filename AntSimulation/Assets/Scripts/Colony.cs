using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class Colony : MonoBehaviour
{
    public float cycleDuration = 30f;
    public int nrOfCycles = 30;

    public GameObject antPrefab;
    public List<AntAgent> ants = new List<AntAgent>();

    public string colonyName = "Colony A";
    public int foodScore = 0;

    public int antsInColony = 10;
    public float spawnRadius = 1f;

    public List<FitAntsPerCycle> bestAntsPerCycle = new List<FitAntsPerCycle>();

    //Quick reference to redo the food every cycle. The food still gets placed randomly as before.
    public FoodSpawner foodSpawner;

    public AntData defaultAnt;

    [Serializable]
    public struct FitAntsPerCycle
    {
        public int generation;
        public AntData antData;
    }

    private void OnDisable()
    {
        SaveSystem.SaveAntsToCVS($"Generations_{colonyName}", bestAntsPerCycle);
    }

    public void AddFood()
    {
        foodScore++;
        Debug.Log(colonyName + " collected food. Total: " + foodScore);
    }
    //if one of the colonies gets anihilated, could stop and say it.
    //Unefficient way of writing it, I know :D
    void SpawnColony(GameObject prefab, Transform baseTransform, bool initialize = false)
    {
        antsInColony = 5 + foodScore / 2; //for every 2 food get 1 more child, with a minimum of 5 per colony
        for (int i = 0; i < antsInColony; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
            GameObject ant = Instantiate(prefab, (Vector2)baseTransform.position + offset, Quaternion.identity);
            var agent = ant.GetComponent<AntAgent>();
            agent.homeColony = this;
            agent.CalculateStats();
            ants.Add(agent);
        }
    }

    void Start()
    {
        defaultAnt.ApplyTo(antPrefab.GetComponent<AntAgent>());
        antPrefab.GetComponent<AntAgent>().CalculateStats();
        antPrefab.GetComponent<AntAgent>().generation = 0;
        foodSpawner.SpawnFood();
        SpawnColony(antPrefab, this.transform, true);
        StartCoroutine(CycleRoutine());
    }

    IEnumerator CycleRoutine()
    {
        while (nrOfCycles > 0)
        {
            yield return new WaitForSeconds(cycleDuration);
            RunMutationCycle();
            nrOfCycles--;
            foodScore = 0;
        }
        SaveSystem.SaveAntsToCVS($"Generations_{colonyName}", bestAntsPerCycle);
    }

    void RunMutationCycle()
    {
        ants.RemoveAll(a => a.dead == true);
        if(ants.Count <= 1 ) 
        {
            Debug.Log("Colony Dead");
            return;
        }
        // Sort by best performance
        foreach(var agent in ants)
        {
            agent.CalculateFitness();
        }

        ants.Sort((a, b) => b.fitness.CompareTo(a.fitness));

        AntAgent bestFemale = ants[0];
        AntAgent bestMale = ants[1];

        //////////////////////////////// Save the data
        FitAntsPerCycle record = new FitAntsPerCycle
        {
            generation = bestAntsPerCycle.Count / 2,
            antData = new AntData(bestFemale),
        };

        record.antData.name = $"BestFemale";
        bestAntsPerCycle.Add(record);

        record = new FitAntsPerCycle
        {
            generation = bestAntsPerCycle[bestAntsPerCycle.Count - 1].generation,
            antData = new AntData(bestMale)
        };

        record.antData.name = $"BestMale";
        bestAntsPerCycle.Add(record);
        //////////////////////////////////

        AntAgent newAnt = antPrefab.GetComponent<AntAgent>();

        //bestAntsPerCycle[bestAntsPerCycle.Count - 1].antData.ApplyTo(newAnt);

        newAnt.ADN = MateAnts(bestMale, bestFemale);

        foreach (var ant in ants)
        {
            Destroy(ant.gameObject);
        }
        ants.Clear();

        newAnt.Mutate();
        newAnt.ResetStats();
        newAnt.generation++;

        foodSpawner.SpawnFood();

        SpawnColony(newAnt.gameObject, this.transform);
    }

    public int[] MateAnts(AntAgent male, AntAgent female)
    {
        int[] ADN = new int[14];

        for (int i = 0; i < ADN.Length; i++)
        {
            ADN[i] = UnityEngine.Random.Range(0, 2) == 1? male.ADN[i] : female.ADN[i];
        }

        return ADN;
    }
}

[Serializable]
public class AntData
{
    public string name;

    public float moveSpeed = 2f;
    public float senseRadius = 1f;
    public int strenght = 10;
    public int curiosity = 0;
    public int scouting = 0;
    public int pasiveness = 0;
    public int agressiveness = 0;

    public float fitness = 0;

    public int generation;

    public int[] ADN = new int[] { 1, 1, //Speed
                                    1, 1, //Vision
                                    1, 1, //Strenght
                                    1, 1, //Curiosity
                                    1, 1, //Scouting (prefers to go to the food)
                                    1, 1,
                                    1, 1,}; //Personality - passive/agressive
    public AntData(AntAgent agent)
    {
        moveSpeed = agent.moveSpeed;
        senseRadius = agent.senseRadius;
        strenght = agent.strenght;
        generation = agent.generation;

        curiosity = agent.curiosity;
        scouting = agent.scouting;
        pasiveness = agent.pasiveness;
        agressiveness = agent.agressiveness;

        fitness = agent.fitness;

        ADN = agent.ADN;
    }

    public void ApplyTo(AntAgent agent)
    {
        agent.ADN = ADN;
    }
}

