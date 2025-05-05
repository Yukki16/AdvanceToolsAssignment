using UnityEngine;

public class AntAgent : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float senseRadius = 1f;
    public float pheromoneStrength = 5f;
    public LayerMask foodLayer;
    public Colony homeColony;

    private bool carryingFood = false;
    private Vector2 direction;

    void Start()
    {
        direction = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        if (carryingFood)
        {
            Vector2 toHome = (homeColony.transform.position - transform.position).normalized;
            Move(toHome);
            GridManager.Instance.AddPheromone(transform.position, pheromoneStrength);
            CheckIfReachedBase();
        }
        else
        {
            Collider2D food = Physics2D.OverlapCircle(transform.position, senseRadius, foodLayer);

            if (food)
            {
                carryingFood = true;
                Destroy(food.gameObject);
            }
            else
            {
                // Try follow pheromone trail
                Vector2 trailDir = GridManager.Instance.GetBestPheromoneDirection(transform.position);

                if (trailDir != Vector2.zero)
                {
                    direction = Vector2.Lerp(direction, trailDir, Time.deltaTime * 5f).normalized;
                }
                else
                {
                    // Wander randomly
                    direction += Random.insideUnitCircle * 0.1f;
                    direction = direction.normalized;
                }

                Move(direction);
            }
        }
    }

    void Move(Vector2 dir)
    {
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    void CheckIfReachedBase()
    {
        if (Vector2.Distance(transform.position, homeColony.transform.position) < 0.5f)
        {
            carryingFood = false;
            homeColony.AddFood();
        }
    }

}
