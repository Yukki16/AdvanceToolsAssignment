using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    public int foodCount = 30;
    public float spawnAreaWidth = 40f;
    public float spawnAreaHeight = 20f;
    public float minDistanceFromBases = 5f;

    public Transform[] colonyBases;

    void Start()
    {
        SpawnFood();
    }

    void SpawnFood()
    {
        int spawned = 0;
        int attempts = 0;
        int maxAttempts = 1000;

        while (spawned < foodCount && attempts < maxAttempts)
        {
            Vector2 randomPos = new Vector2(
                Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f),
                Random.Range(-spawnAreaHeight / 2f, spawnAreaHeight / 2f)
            );

            bool tooCloseToBase = false;
            foreach (Transform basePos in colonyBases)
            {
                if (Vector2.Distance(basePos.position, randomPos) < minDistanceFromBases)
                {
                    tooCloseToBase = true;
                    break;
                }
            }

            if (!tooCloseToBase)
            {
                Instantiate(foodPrefab, randomPos, Quaternion.identity);
                spawned++;
            }

            attempts++;
        }
    }
}
