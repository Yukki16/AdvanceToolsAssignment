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

    [Serializable]
    public struct FitAntsPerCycle
    {
        public int generation;
        public AntAgent ant;
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
        //Debug.Log("Mutating generation...");

        // Sort by best performance (e.g., food delivered)
        ants.Sort((a, b) => b.foodDelivered.CompareTo(a.foodDelivered));

        int survivors = ants.Count / 2;

        bestAntsPerCycle.Add(new FitAntsPerCycle());

        var a = bestAntsPerCycle[bestAntsPerCycle.Count - 1];
        a.generation = bestAntsPerCycle.Count - 1;

        ants[0].Mutate();
        ants[0].generation++;
        ants[0].ResetStats();

        a.ant = ants[0];

        bestAntsPerCycle[bestAntsPerCycle.Count - 1] = a;

        foreach (var ant in ants)
        {
            ant.gameObject.SetActive(false);
        }
        ants.Clear();

        SpawnColony(a.ant.gameObject, this.transform);
    }

    /*void CopyAndMutateFrom(AntAgent source, AntAgent target)
    {
        target.strenght = source.strenght;
        target.moveSpeed = source.moveSpeed;
        target.senseRadius = source.senseRadius;
        target.generation = source.generation + 1;

        // Now mutate randomly
        target.Mutate();
        target.ResetStats();
    }*/
}
