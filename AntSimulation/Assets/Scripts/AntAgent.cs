using System.Collections;
using UnityEngine;

public class AntAgent : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float senseRadius = 1f;
    public float restingTime = 1f;
    public float resistance = 3f;

    public float pheromoneStrength = 5f;
    public LayerMask foodLayer;
    public Colony homeColony;
    public float helpMovementBoost = 1f;

    private bool carryingFood = false;
    private Vector2 direction;

    public int fightsFought = 0;
    public int fightsWon = 0;
    [Range(0, 100)] public int chanceToFight = 50;

    public bool forceRest;
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
        while (timer < resistance)
        {

            Collider2D food = Physics2D.OverlapCircle(transform.position, senseRadius, foodLayer);
            if (food)
            {
                carryingFood = true;
                food.transform.parent = this.transform;
                carriedFood = food.transform;
                StartCoroutine(CarryFood());
                yield break;
            }
            else
            {
                // Try follow pheromone trail to help another ant
                /*Vector2 trailDir = GridManager.Instance.GetBestPheromoneDirection(transform.position);

                if (trailDir != Vector2.zero)
                {
                    direction = Vector2.Lerp(direction, trailDir, Time.deltaTime * 5f).normalized;
                }*/
                /*else
                {
                    direction += Random.insideUnitCircle * 0.1f;
                    direction = direction.normalized;
                }*/
                Move(direction);
            }
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
            homeColony.AddFood();
        }
    }

    public bool WillFight()
    {
        return !(Random.Range(1, 101) > chanceToFight); //The higher the chance to fight so, like I have to inverse the result? Mainly due to naming
    }                                                   //This way chance to fight 0 -> always false
                                                        //Chance to fight 100 -> always true
}
