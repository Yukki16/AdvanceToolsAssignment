using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public int width = 50;
    public int height = 30;
    public float cellSize = 1f;

    private float[,] pheromoneMap;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        pheromoneMap = new float[width, height];
    }

    void Update()
    {
        DecayPheromones(Time.deltaTime);
    }

    private void DecayPheromones(float deltaTime)
    {
        float decayRate = 1f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                pheromoneMap[x, y] = Mathf.Max(0, pheromoneMap[x, y] - decayRate * deltaTime);
            }
        }
    }

    public void AddPheromone(Vector2 worldPosition, float amount)
    {
        Vector2Int gridPos = WorldToGrid(worldPosition);

        if (IsInsideGrid(gridPos))
        {
            pheromoneMap[gridPos.x, gridPos.y] += amount;
        }
    }

    public Vector2 GetBestPheromoneDirection(Vector2 worldPosition)
    {
        Vector2Int gridPos = WorldToGrid(worldPosition);
        Vector2 bestDirection = Vector2.zero;
        float maxPheromone = 0f;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector2Int neighbor = new Vector2Int(gridPos.x + dx, gridPos.y + dy);

                if (IsInsideGrid(neighbor))
                {
                    if (pheromoneMap[neighbor.x, neighbor.y] > maxPheromone)
                    {
                        maxPheromone = pheromoneMap[neighbor.x, neighbor.y];
                        bestDirection = new Vector2(dx, dy).normalized;
                    }
                }
            }
        }

        return bestDirection;
    }

    private Vector2Int WorldToGrid(Vector2 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x + width * cellSize * 0.5f) / cellSize);
        int y = Mathf.FloorToInt((worldPos.y + height * cellSize * 0.5f) / cellSize);
        return new Vector2Int(x, y);
    }

    private bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }
}
