using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AntAgent : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 2f;
    public float senseRadius = 1f;
    public int strenght = 10;
    public int curiosity = 0;
    public int scouting = 0;
    public int personality = 0;

    public float fitness = 0;

    
    ////////////////////////////////////
    public float restingTime = 1f;
    public float resistance = 3f;

    [Header("Settings")]
    //public float pheromoneStrength = 5f;
    public LayerMask foodLayer;
    public LayerMask antLayer;
    //public float helpMovementBoost = 1f;
    public Colony homeColony;
    public bool forceRest;
    public Collider2D thisCollider;

    [Header("Results")]
    public int foodDelivered = 0;
    public int fightsFought = 0;
    public int fightsWon = 0;
    public int generation = 0;

    private Vector2 direction;
    private bool carryingFood = false;
    Transform carriedFood;

    [Header("Mutation Settings")]
    [Range(0, 100)] public float speedMutationChance = 20f;
    [Range(0, 100)] public float visionMutationChance = 20f;
    [Range(0, 100)] public float strenghtMutationChance = 20f;
    [Range(0, 100)] public float curiosityMutationChance = 20f;
    [Range(0, 100)] public float scoutingMutationChance = 20f;
    [Range(0, 100)] public float personalityMutationChance = 20f;

    [Header("ADN")]
    public int[] ADN = new int[] { 1, 1, //Speed
                                    1, 1, //Vision
                                    1, 1, //Strenght
                                    1, 1, //Curiosity
                                    1, 1, //Scouting (prefers to go to the food)
                                    1, 1,}; //Personality - passive/agressive


    void Start()
    {
        StartCoroutine(Moving());
    }

    IEnumerator Resting()
    {
        yield return new WaitForSeconds(restingTime);
        StartCoroutine(Moving());
    }

    IEnumerator Moving()
    {
        direction = UnityEngine.Random.insideUnitCircle.normalized;
        float timer = 0f;
        while (timer < resistance || carryingFood)
        {
            List<Collider2D> foodHits = Physics2D.OverlapCircleAll(transform.position, senseRadius, foodLayer).ToList();
            List<Collider2D> antHits = Physics2D.OverlapCircleAll(transform.position, senseRadius, antLayer).ToList();
            
            if(antHits.Contains(thisCollider))
                antHits.Remove(thisCollider);

            if(antHits.Count > 0)
            {

            }




            //check for everything, likability to go in that direction.
            Transform closestFood = null;
            float closestDist = Mathf.Infinity;

            foreach (var hit in foodHits)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestFood = hit.transform;
                }
            }

            //Add var to calculate if it is worth to get the food - like colony distance, distance to the food, need of food
            //a hungry gene - a tendancy to go towards food even if it is not hungry
            //curiosity gene - to move away from the colony/ stay close to the colony
            //if ant nearby - passiveness/agressiveness/following

            if (closestFood != null)
            {
                Vector2 toFood = (closestFood.position - transform.position).normalized;
                direction = Vector2.Lerp(direction, toFood, Time.deltaTime * 3f).normalized;
            }
            else
            {
                // Wander randomly
                direction += UnityEngine.Random.insideUnitCircle * 0.1f;
                direction = direction.normalized;
            }
            Move(direction);
            timer += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(Resting());

    }

    IEnumerator CarryFood()
    {
        Vector2 toHome = (homeColony.transform.position - transform.position).normalized;

        while (carryingFood)
        {
            Move(toHome);
            //GridManager.Instance.AddPheromone(transform.position, pheromoneStrength);
            CheckIfReachedBase();
            yield return null;
        }

        StartCoroutine(Resting());
    }
    void Move(Vector2 dir)
    {
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime /** helpMovementBoost*/);
    }


    void CheckIfReachedBase()
    {
        if (Vector2.Distance(transform.position, homeColony.transform.position) < 0.5f)
        {
            if (carriedFood != null)
            {
                Destroy(carriedFood.gameObject);
            }
            carryingFood = false;
            foodDelivered++;
            homeColony.AddFood();
        }
    }

    public void ResetStats()
    {
        foodDelivered = 0;
        fightsFought = 0;
        fightsWon = 0;
    }

    public void Mutate()
    {
        float mutateSpeed = UnityEngine.Random.Range(0f, 100f);
        float mutateVision = UnityEngine.Random.Range(0f, 100f);
        float mutateStrenght = UnityEngine.Random.Range(0f, 100f);
        float mutateCuriosity = UnityEngine.Random.Range(0f, 100f);
        float mutateScouting = UnityEngine.Random.Range(0f, 100f);
        float mutatePersonality = UnityEngine.Random.Range(0f, 100f);

        if (mutateSpeed < speedMutationChance)
        {
            var a = UnityEngine.Random.Range(0, 2); //Decide which of the 2 values gets mutated and mutate to better or worse by 1
            ADN[a] += UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1; 
        }

        if (mutateVision < visionMutationChance)
        {
            var a = UnityEngine.Random.Range(2, 4); //Decide which of the 2 values gets mutated and mutate to better or worse by 1
            ADN[a] += UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1;
        }

        if (mutateStrenght < strenghtMutationChance)
        {
            var a = UnityEngine.Random.Range(4, 6); //Decide which of the 2 values gets mutated and mutate to better or worse by 1
            ADN[a] += UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1;
        }

        if (mutateCuriosity < speedMutationChance)
        {
            var a = UnityEngine.Random.Range(6, 8); //Decide which of the 2 values gets mutated and mutate to better or worse by 1
            ADN[a] += UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1;
        }

        if (mutateScouting < visionMutationChance)
        {
            var a = UnityEngine.Random.Range(8, 10); //Decide which of the 2 values gets mutated and mutate to better or worse by 1
            ADN[a] += UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1;
        }

        if (mutatePersonality < strenghtMutationChance)
        {
            var a = UnityEngine.Random.Range(10, 12); //Decide which of the 2 values gets mutated and mutate to better or worse by 1
            ADN[a] += UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1;
        }

        CalculateStats();
    }

    public void CalculateStats()
    {
        moveSpeed = 1 + 0.5f * (int)ADN[0] + 0.5f * (int)ADN[1];
        senseRadius = 1 + 0.5f * (int)ADN[2] + 0.5f * (int)ADN[3];
        strenght = 1 + (int)ADN[4] + (int)ADN[5];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag(Tags.T_Food))
        {
            StopAllCoroutines();
            carryingFood = true;
            collision.transform.parent = this.transform;
            carriedFood = collision.transform;
            StartCoroutine(CarryFood());
        }
    }

    public void CalculateFitness()
    {

    }
}
