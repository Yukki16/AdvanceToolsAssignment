using UnityEngine;

public class Colony : MonoBehaviour
{
    public string colonyName = "Colony A";
    public int foodScore = 0;

    public void AddFood()
    {
        foodScore++;
        Debug.Log(colonyName + " collected food. Total: " + foodScore);
    }
}
