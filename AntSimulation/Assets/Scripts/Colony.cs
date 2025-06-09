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


    public void AddFood()
    {
        foodScore++;
        Debug.Log(colonyName + " collected food. Total: " + foodScore);
    }

    void SpawnColony(GameObject prefab, Transform baseTransform)
    {
        for (int i = 0; i < antsInColony; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
            GameObject ant = Instantiate(prefab, (Vector2)baseTransform.position + offset, Quaternion.identity);
            ant.GetComponent<AntAgent>().homeColony = this;
            ants.Add(ant.GetComponent<AntAgent>());
        }
    }

    void Start()
    {
        defaultAnt.ApplyTo(antPrefab.GetComponent<AntAgent>());
        antPrefab.GetComponent<AntAgent>().generation = 0;
        foodSpawner.SpawnFood();
        SpawnColony(antPrefab, this.transform);
        StartCoroutine(CycleRoutine());
    }

    IEnumerator CycleRoutine()
    {
        while (nrOfCycles > 0)
        {
            yield return new WaitForSeconds(cycleDuration);
            RunMutationCycle();
            nrOfCycles--;
        }
    }

    void RunMutationCycle()
    {
        // Sort by best performance
        ants.Sort((a, b) => b.foodDelivered.CompareTo(a.foodDelivered));

        int survivors = ants.Count / 2;
        AntAgent best = ants[0];

        // Save a data snapshot of the best performer
        FitAntsPerCycle record = new FitAntsPerCycle
        {
            generation = bestAntsPerCycle.Count,
            antData = new AntData(best)
        };
        bestAntsPerCycle.Add(record);

        AntAgent newAnt = antPrefab.GetComponent<AntAgent>();

        bestAntsPerCycle[bestAntsPerCycle.Count - 1].antData.ApplyTo(newAnt);

        // Clear ants from the scene
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
}

[Serializable]
public class AntData
{
    public float moveSpeed;
    public float senseRadius;
    public float resistance;
    public float pheromoneStrength;
    public int strenght;
    public int generation;

    public AntData(AntAgent agent)
    {
        moveSpeed = agent.moveSpeed;
        senseRadius = agent.senseRadius;
        resistance = agent.resistance;
        pheromoneStrength = agent.pheromoneStrength;
        strenght = agent.strenght;
        generation = agent.generation;
    }

    public void ApplyTo(AntAgent agent)
    {
        agent.moveSpeed = moveSpeed;
        agent.senseRadius = senseRadius;
        agent.resistance = resistance;
        agent.pheromoneStrength = pheromoneStrength;
        agent.strenght = strenght;
        agent.generation = generation;
    }
}

