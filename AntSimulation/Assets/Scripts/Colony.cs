using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using static AntAgent;
using UnityEngine.UI;

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
        SaveSystem.Save("Generations", bestAntsPerCycle);
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
            var agent = ant.GetComponent<AntAgent>();
            agent.homeColony = this;
            agent.gender[1] = (AntAgent.GenderChromosome)UnityEngine.Random.Range(0, 2);
            ants.Add(agent);
        }
    }

    void Start()
    {
        defaultAnt.ApplyTo(antPrefab.GetComponent<AntAgent>());
        antPrefab.GetComponent<AntAgent>().CalculateStats();
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
        SaveSystem.Save($"Generations_{colonyName}", bestAntsPerCycle);
    }

    void RunMutationCycle()
    {
        // Sort by best performance
        ants.Sort((a, b) => b.foodDelivered.CompareTo(a.foodDelivered));

        AntAgent bestFemale = null;
        AntAgent bestMale = null;

        foreach (var ant in ants) 
        {
            if (bestFemale == null && ant.gender[1] == GenderChromosome.X)
            {
                bestFemale = ant;
            }
            else if (bestMale == null && ant.gender[1] == GenderChromosome.Y)
            {
                bestMale = ant;
            }

            if (bestFemale != null && bestMale != null)
                break;
        }

        if(bestFemale == null)
        {
            bestFemale = new AntAgent();
            defaultAnt.ApplyTo(bestFemale);
            bestFemale.CalculateStats();
        }

        if (bestMale == null)
        {
            bestMale = new AntAgent();
            defaultAnt.ApplyTo(bestMale);
            bestMale.CalculateStats();
        }

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

    public AntAgent.TraitSkillLevel[] MateAnts(AntAgent male, AntAgent female)
    {
        AntAgent.TraitSkillLevel[] ADN = new TraitSkillLevel[6];

        ADN[0] = male.ADN[0];
        ADN[1] = female.ADN[1];
        ADN[2] = male.ADN[2];
        ADN[3] = female.ADN[3];
        ADN[4] = male.ADN[4];
        ADN[5] = female.ADN[5];

        return ADN;
    }
}

[Serializable]
public class AntData
{
    public string name;

    public float moveSpeed;
    public float senseRadius;
    public int strenght;
    public int generation;

    public AntAgent.TraitSkillLevel[] ADN = new AntAgent.TraitSkillLevel[6];
    public AntAgent.GenderChromosome[] gender = new AntAgent.GenderChromosome[2];
    public AntData(AntAgent agent)
    {
        moveSpeed = agent.moveSpeed;
        senseRadius = agent.senseRadius;
        strenght = agent.strenght;
        generation = agent.generation;

        ADN = agent.ADN;
        gender = agent.gender;
    }

    public void ApplyTo(AntAgent agent)
    {
        agent.ADN = ADN;
        agent.gender = gender;
    }
}

