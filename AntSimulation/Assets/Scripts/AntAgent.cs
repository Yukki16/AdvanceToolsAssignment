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
    public int pasiveness = 0;
    public int agressiveness = 0;

    public float fitness = 0;


    ////////////////////////////////////
    public float restingTime = 1f;
    public float resistance = 3f;

    public float helpMovementBoost = 1;

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
    [HideInInspector] public bool carryingFood = false;
    public bool dead = false;

    Transform carriedFood;

    [Header("Mutation Settings")]
    [Range(0, 100)] public float speedMutationChance = 20f;
    [Range(0, 100)] public float visionMutationChance = 20f;
    [Range(0, 100)] public float strenghtMutationChance = 20f;
    [Range(0, 100)] public float curiosityMutationChance = 20f;
    [Range(0, 100)] public float scoutingMutationChance = 20f;
    [Range(0, 100)] public float passivenessMutationChance = 20f;
    [Range(0, 100)] public float agressivenessMutationChance = 20f;

    [Header("ADN")]
    public int[] ADN = new int[] { 1, 1, //Speed
                                    1, 1, //Vision
                                    1, 1, //Strenght
                                    1, 1, //Curiosity
                                    1, 1, //Scouting (prefers to go to the food)
                                    1, 1,//Personality - passive
                                    1, 1,}; // agressive


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
            antHits.Remove(thisCollider);

            //Filter out friendly ants
            antHits.RemoveAll(hit => hit.GetComponent<AntAgent>().homeColony == this.homeColony);

            //If ants in sight
            int personalityWeight = 0;
            if (antHits.Count > 0)
            {
                int minValue = Mathf.Min(pasiveness, agressiveness);
                int offset = (minValue < 0) ? -minValue : 0;
                int pasivenessWeight = pasiveness + offset;
                int agressivenessWeight = agressiveness + offset;
                int total = pasivenessWeight + agressivenessWeight;

                if (total > 0)
                {
                    int roll = UnityEngine.Random.Range(0, total);
                    if (roll >= pasivenessWeight) //if chooses violence on the ant we add it to the weight of the behaviours
                        personalityWeight = agressiveness;
                }
            }

            //If food in sight
            if (foodHits.Count > 0)
            {
                int minValue = Mathf.Min(scouting, curiosity);
                int offset = (minValue < 0) ? -minValue : 0;
                int curiosityWeight = curiosity + offset;
                int scoutingWeight = scouting + offset;
                int total = curiosityWeight + scoutingWeight;

                if (total > 0)
                {
                    int roll = UnityEngine.Random.Range(0, total);
                    bool choosesCuriosity = roll < curiosityWeight;
                    if(curiosityWeight == scoutingWeight)
                    {
                        choosesCuriosity = UnityEngine.Random.Range(0, 2) == 0? false : true;
                    }

                    if (choosesCuriosity)
                    {
                        if (personalityWeight > 0) //it does have an ant nearby
                        {
                            int combinedWeight = personalityWeight + curiosity;
                            int combatRoll = UnityEngine.Random.Range(0, combinedWeight);
                            if (combatRoll >= personalityWeight)
                            {
                                //Explore
                                Vector2 toBase = (homeColony.transform.position - transform.position).normalized;
                                float minAngle = 30f; //Minimum angle to explore away from home
                                do
                                {
                                    direction = UnityEngine.Random.insideUnitCircle.normalized;
                                }
                                while (Vector2.Angle(direction, toBase) < minAngle);
                            }
                            else
                            {
                                //Goes for a random ant of the one it sees
                                int enemyRoll = UnityEngine.Random.Range(0, antHits.Count);
                                direction = (antHits[enemyRoll].transform.position - transform.position).normalized;
                            }
                        }
                        else
                        {
                            //Explore
                            Vector2 toBase = (homeColony.transform.position - transform.position).normalized;
                            float minAngle = 30f;
                            do
                            {
                                direction = UnityEngine.Random.insideUnitCircle.normalized;
                            }
                            while (Vector2.Angle(direction, toBase) < minAngle);
                        }
                    }
                    else
                    {
                        if (personalityWeight > 0)
                        {
                            int combinedWeight = personalityWeight + scouting;
                            int combatRoll = UnityEngine.Random.Range(0, combinedWeight);
                            if (combatRoll >= personalityWeight)
                            {
                                //Chooses a random food to go for that is in sight
                                int foodRoll = UnityEngine.Random.Range(0, foodHits.Count);
                                direction = (foodHits[foodRoll].transform.position - transform.position).normalized;
                            }
                            else
                            {
                                //Goes for a random ant of the one it sees
                                int enemyRoll = UnityEngine.Random.Range(0, antHits.Count);
                                direction = (antHits[enemyRoll].transform.position - transform.position).normalized;
                            }
                        }
                        else
                        {
                            //Chooses a random food to go for that is in sight
                            int foodRoll = UnityEngine.Random.Range(0, foodHits.Count);
                            direction = (foodHits[foodRoll].transform.position - transform.position).normalized;
                        }
                    }
                }
                else
                {
                    // Total weight = 0, ant is indecisive, goes randomly
                    direction += UnityEngine.Random.insideUnitCircle * 0.1f;
                    direction = direction.normalized;
                }
            }
            else if (personalityWeight > 0) //it still does have an ant nearby -- last fallback to either attack or explore
            {
                int combinedWeight = personalityWeight + curiosity;
                int combatRoll = UnityEngine.Random.Range(0, combinedWeight);
                if (combatRoll >= personalityWeight)
                {
                    //Explore
                    Vector2 toBase = (homeColony.transform.position - transform.position).normalized;
                    float minAngle = 30f; //Minimum angle to explore away from home
                    do
                    {
                        direction = UnityEngine.Random.insideUnitCircle.normalized;
                    }
                    while (Vector2.Angle(direction, toBase) < minAngle);
                }
                else
                {
                    //Goes for a random ant of the one it sees
                    int enemyRoll = UnityEngine.Random.Range(0, antHits.Count);
                    direction = (antHits[enemyRoll].transform.position - transform.position).normalized;
                }
            }
            else //no food, no enemies, ultimate fallback whether it is curious to explore or just doesn't know
            {
                int lastRoll = UnityEngine.Random.Range(0, 2);
                if (lastRoll == 0)
                {
                    Vector2 toBase = (homeColony.transform.position - transform.position).normalized;
                    float minAngle = 30f; //Minimum angle to explore away from home
                    do
                    {
                        direction = UnityEngine.Random.insideUnitCircle.normalized;
                    }
                    while (Vector2.Angle(direction, toBase) < minAngle);
                }
                else
                {
                    direction += UnityEngine.Random.insideUnitCircle * 0.1f;
                    direction = direction.normalized;
                }
            }

            Move(direction);
            timer += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(Resting());
    }

    IEnumerator CarryFood()
    {

        while (carryingFood)
        {
            Vector2 toHome = (homeColony.transform.position - transform.position).normalized;
            Move(toHome);
            //GridManager.Instance.AddPheromone(transform.position, pheromoneStrength);
            CheckIfReachedBase();
            if (dead)
            {
                break;
            }
            yield return null;
        }

        if(!dead)
        StartCoroutine(Resting());
    }
    void Move(Vector2 dir)
    {
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime * helpMovementBoost);
    }


    void CheckIfReachedBase()
    {
        if (Vector2.Distance(transform.position, homeColony.transform.position) < 0.5f)
        {
            if (carriedFood != null)
            {
                Destroy(carriedFood.gameObject);
                homeColony.AddFood();
                foodDelivered++; //Extra point for finding the food to bring home
            }
            carryingFood = false;
            foodDelivered++;
            helpMovementBoost = 1;
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
        float mutatePersonalityPassive = UnityEngine.Random.Range(0f, 100f);
        float mutatePersonalityAgressive = UnityEngine.Random.Range(0f, 100f);

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

        if (mutateCuriosity < curiosityMutationChance)
        {
            var a = UnityEngine.Random.Range(6, 8); //Decide which of the 2 values gets mutated and mutate to better or worse by 1
            ADN[a] += UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1;
        }

        if (mutateScouting < scoutingMutationChance)
        {
            var a = UnityEngine.Random.Range(8, 10); //Decide which of the 2 values gets mutated and mutate to better or worse by 1
            ADN[a] += UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1;
        }

        if (mutatePersonalityPassive < passivenessMutationChance)
        {
            var a = UnityEngine.Random.Range(10, 12); //Decide which of the 2 values gets mutated and mutate to better or worse by 1
            ADN[a] += UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1;
        }

        if (mutatePersonalityAgressive < agressivenessMutationChance)
        {
            var a = UnityEngine.Random.Range(12, 14); //Decide which of the 2 values gets mutated and mutate to better or worse by 1
            ADN[a] += UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1;
        }

        CalculateStats();
    }

    public void CalculateStats()
    {
        moveSpeed = 1 + 0.5f * ADN[0] + 0.5f * ADN[1];
        senseRadius = Mathf.Max(0.1f, 1 + 0.5f * ADN[2] + 0.5f * ADN[3]); //Minimum value required for function to work
        strenght = 1 + ADN[4] + ADN[5];
        curiosity = 1 + ADN[6] + ADN[7];
        scouting = 1 + ADN[8] + ADN[9];
        pasiveness = 1 + ADN[10] + ADN[11];
        agressiveness = 1 + ADN[12] + ADN[13];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.T_Food))
        {
            if (carryingFood)
                return;
            if(collision.transform.parent != null)
            {
                if(collision.transform.parent.GetComponent<AntAgent>().homeColony != homeColony)
                    InvokeFight(this, collision.transform.parent.GetComponent<AntAgent>());
                else
                {
                    helpMovementBoost += 0.25f;
                    collision.transform.parent.GetComponent<AntAgent>().helpMovementBoost = helpMovementBoost;
                    carryingFood = true;
                    StartCoroutine(CarryFood());
                }
                return;
            }
            if (!dead)
            {
                StopAllCoroutines();
                carryingFood = true;
                collision.transform.parent = this.transform;
                carriedFood = collision.transform;
                StartCoroutine(CarryFood());
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag(Tags.T_Ant))
        {
            if(collision.transform.GetComponent<AntAgent>().homeColony == homeColony)
            {
                return;
            }
            if (dead || collision.transform.GetComponent<AntAgent>().dead)
            {
                return;
            }
            else
                InvokeFight(this, collision.transform.GetComponent<AntAgent>());
        }
    }

    public void CalculateFitness()
    {
        float fightingFitness = 0;
        if (fightsFought > 0)
        {
            fightingFitness = fightsWon / fightsFought;
        }
        fitness = foodDelivered + fightingFitness;
    }

    public void InvokeFight(AntAgent thisAnt, AntAgent enemy)
    {
        if (thisAnt.strenght == enemy.strenght)
        {
            int roll = UnityEngine.Random.Range(0, 2); //Random roll for fight win
            if (roll == 0)
            {
                if (enemy.carryingFood) //move the food to the winning ant
                {
                    thisAnt.StopAllCoroutines();
                    carryingFood = true;
                    carriedFood = enemy.carriedFood.transform;
                    enemy.carriedFood.transform.parent = this.transform;
                    enemy.carryingFood = false;
                    thisAnt.StartCoroutine(CarryFood());
                }

                enemy.dead = true;
                thisAnt.fightsWon++;
            }
            else
            {
                if (thisAnt.carryingFood)
                {
                    enemy.StopAllCoroutines();
                    carryingFood = true;
                    carriedFood = thisAnt.carriedFood.transform;
                    thisAnt.carriedFood.transform.parent = this.transform;
                    thisAnt.carryingFood = false;
                    enemy.StartCoroutine(CarryFood());
                }
                thisAnt.dead = true;
                enemy.fightsWon++;
            }
        }
        else
        if (thisAnt.strenght > enemy.strenght)
        {
            if (enemy.carryingFood)
            {
                thisAnt.StopAllCoroutines();
                carryingFood = true;
                carriedFood = enemy.carriedFood.transform;
                enemy.carriedFood.transform.parent = this.transform;
                enemy.carryingFood = false;
                thisAnt.StartCoroutine(CarryFood());
            }
            enemy.dead = true;
            thisAnt.fightsWon++;
        }
        else
        {
            if (thisAnt.carryingFood)
            {
                enemy.StopAllCoroutines();
                carryingFood = true;
                carriedFood = thisAnt.carriedFood.transform;
                thisAnt.carriedFood.transform.parent = this.transform;
                thisAnt.carryingFood = false;
                enemy.StartCoroutine(CarryFood());
            }
            thisAnt.dead = true;
            enemy.fightsWon++;
        }

        thisAnt.fightsFought++;
        enemy.fightsFought++;
    }

}
