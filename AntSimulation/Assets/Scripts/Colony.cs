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

    public AntData defaultMale;
    public AntData defaultFemale;

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
    //if one of the colonies gets anihilated, could stop and say it.
    //Unefficient way of writing it, I know :D
    void SpawnColony(GameObject prefab, Transform baseTransform, bool initialize = false)
    {
        //At least 1 female
        Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
        GameObject ant = Instantiate(prefab, (Vector2)baseTransform.position + offset, Quaternion.identity);
        var agent = ant.GetComponent<AntAgent>();
        agent.homeColony = this;
        agent.gender[1] = GenderChromosome.X;

       
        if (initialize)
        {
            if (agent.gender[1] == GenderChromosome.X)
            {
                defaultFemale.ApplyTo(agent);
            }
            else
            {
                defaultMale.ApplyTo(agent);
            }
        }
        agent.CalculateStats();
        ants.Add(agent);

        //At least 1 male
        offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
        ant = Instantiate(prefab, (Vector2)baseTransform.position + offset, Quaternion.identity);
        agent = ant.GetComponent<AntAgent>();
        agent.homeColony = this;
        agent.gender[1] = GenderChromosome.Y;

        if (initialize)
        {
            if (agent.gender[1] == GenderChromosome.Y)
            {
                defaultFemale.ApplyTo(agent);
            }
            else
            {
                defaultMale.ApplyTo(agent);
            }
        }
        agent.CalculateStats();
        ants.Add(agent);

        for (int i = 2; i < antsInColony; i++)
        {
            offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
            ant = Instantiate(prefab, (Vector2)baseTransform.position + offset, Quaternion.identity);
            agent = ant.GetComponent<AntAgent>();
            agent.homeColony = this;
            agent.gender[1] = (AntAgent.GenderChromosome)UnityEngine.Random.Range(0, 2);
            if (initialize)
            {
                if (agent.gender[1] == GenderChromosome.X)
                {
                    defaultFemale.ApplyTo(agent);
                }
                else
                {
                    defaultMale.ApplyTo(agent);
                }
            }
            agent.CalculateStats();
            ants.Add(agent);
        }
    }

    void Start()
    {
        //defaultAnt.ApplyTo(antPrefab.GetComponent<AntAgent>());
        //antPrefab.GetComponent<AntAgent>().CalculateStats();
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

        if (bestFemale == null)
        {
            bestFemale = new AntAgent();
            defaultFemale.ApplyTo(bestFemale);
            bestFemale.CalculateStats();
        }

        if (bestMale == null)
        {
            bestMale = new AntAgent();
            defaultMale.ApplyTo(bestMale);
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

    public TraitSkillLevel[] MateAnts(AntAgent male, AntAgent female)
    {
        TraitSkillLevel[] ADN = new TraitSkillLevel[6];

        ADN[0] = male.ADN[0]; // 50% change for each from each parent
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

    public TraitSkillLevel[] ADN = new TraitSkillLevel[6] {
    TraitSkillLevel.B, TraitSkillLevel.B,
    TraitSkillLevel.B, TraitSkillLevel.B,
    TraitSkillLevel.B, TraitSkillLevel.B,};
    public GenderChromosome[] gender = new GenderChromosome[2] {GenderChromosome.X, GenderChromosome.Y};
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

