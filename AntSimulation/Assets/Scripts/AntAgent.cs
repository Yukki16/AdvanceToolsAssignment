using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AntAgent : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 2f;
    public float senseRadius = 1f;
    public float restingTime = 1f;
    public float resistance = 3f;
    public int strenght = 10;
    [Range(0, 100)] public int chanceToFight = 50;

    [Header("Settings")]
    public float pheromoneStrength = 5f;
    public LayerMask foodLayer;
    public float helpMovementBoost = 1f;
    public Colony homeColony;
    public bool forceRest;

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

    [Header("ADN")]
    public TraitSkillLevel[] ADN = new TraitSkillLevel[6] { TraitSkillLevel.B, TraitSkillLevel.B, //Speed
        TraitSkillLevel.B, TraitSkillLevel.B, //Vision
        TraitSkillLevel.B, TraitSkillLevel.B, }; //Strenght

    public GenderChromosome[] gender = new GenderChromosome[2] { GenderChromosome.X, GenderChromosome.Y };

    [Serializable]
    public enum TraitSkillLevel
    {
        D,
        C,
        B,
        A,
        S,
        SS,
        SSS
    }

    [Serializable]
    public enum GenderChromosome
    { 
        X,
        Y
    }

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

            Collider2D[] foodHits = Physics2D.OverlapCircleAll(transform.position, senseRadius, foodLayer);
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
            GridManager.Instance.AddPheromone(transform.position, pheromoneStrength);
            CheckIfReachedBase();
            yield return null;
        }

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
            }
            carryingFood = false;
            foodDelivered++;
            homeColony.AddFood();
        }
    }


    public bool WillFight()
    {
        return !(UnityEngine.Random.Range(1, 101) > chanceToFight); //The higher the chance to fight so, like I have to inverse the result? Mainly due to naming
    }                                                   //This way chance to fight 0 -> always false
                                                        //Chance to fight 100 -> always true

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

        if (mutateSpeed < speedMutationChance)
        {
            var a = UnityEngine.Random.Range(0, 2); //Decide which of the 2 values gets mutated and mutate to better or worse by 1
            ADN[a] = (TraitSkillLevel)Mathf.Clamp((int)(ADN[a] + (UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1)), (int)TraitSkillLevel.D, (int)TraitSkillLevel.SSS); 
        }

        if (mutateVision < visionMutationChance)
        {
            var a = UnityEngine.Random.Range(2, 4); //Decide which of the 2 values gets mutated and mutate to better or worse by 1
            ADN[a] = (TraitSkillLevel)Mathf.Clamp((int)(ADN[a] + (UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1)), (int)TraitSkillLevel.D, (int)TraitSkillLevel.SSS);
        }

        if (mutateStrenght < strenghtMutationChance)
        {
            var a = UnityEngine.Random.Range(4, 6); //Decide which of the 2 values gets mutated and mutate to better or worse by 1
            ADN[a] = (TraitSkillLevel)Mathf.Clamp((int)(ADN[a] + (UnityEngine.Random.Range(0, 2) == 1 ? -1 : 1)), (int)TraitSkillLevel.D, (int)TraitSkillLevel.SSS);
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
}
