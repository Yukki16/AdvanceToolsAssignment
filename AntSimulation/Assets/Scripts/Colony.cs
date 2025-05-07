using UnityEngine;
using System.Collections.Generic;

public class Colony : MonoBehaviour
{
    public List<AntAgent> ants = new List<AntAgent>();
    public string colonyName = "Colony A";
    public int foodScore = 0;

    public void AddFood()
    {
        foodScore++;
        Debug.Log(colonyName + " collected food. Total: " + foodScore);
    }
}
