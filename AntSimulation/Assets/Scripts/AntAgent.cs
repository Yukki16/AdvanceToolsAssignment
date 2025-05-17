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
        direction = Random.insideUnitCircle.normalized;
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
                direction += Random.insideUnitCircle * 0.1f;
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
        return !(Random.Range(1, 101) > chanceToFight); //The higher the chance to fight so, like I have to inverse the result? Mainly due to naming
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
        float totalPoints = 10f; // arbitrary value you can balance

        // Randomize proportions
        float rand1 = Random.Range(1f, totalPoints - 1f);
        float rand2 = Random.Range(1f, totalPoints - rand1);
        float rand3 = totalPoints - rand1 - rand2;

        // Normalize and assign
        float total = rand1 + rand2 + rand3;
        strenght = Mathf.RoundToInt(rand1 / total * 15); // scale to usable values
        moveSpeed = (rand2 / total) * 5f; // scale speed (e.g. 0.5 to 5)
        senseRadius = (rand3 / total) * 3f; // adjust detection radius
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
