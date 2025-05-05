using UnityEngine;

public class AntSpawner : MonoBehaviour
{
    public GameObject antTeamAPrefab;
    public GameObject antTeamBPrefab;

    public Transform baseA;
    public Transform baseB;

    public int antsPerColony = 10;
    public float spawnRadius = 1f;

    void Start()
    {
        SpawnColony(antTeamAPrefab, baseA);
        SpawnColony(antTeamBPrefab, baseB);
    }

    void SpawnColony(GameObject prefab, Transform baseTransform)
    {
        for (int i = 0; i < antsPerColony; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            GameObject ant = Instantiate(prefab, (Vector2)baseTransform.position + offset, Quaternion.identity);
            ant.GetComponent<AntAgent>().homeColony = baseTransform.GetComponent<Colony>();
        }
    }
}
