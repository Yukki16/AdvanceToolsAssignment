using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Food : MonoBehaviour
{
    Dictionary<AntAgent, Colony> antsOnFood = new Dictionary<AntAgent, Colony>();

    public void AddMe(AntAgent antAtFood)
    {
        antsOnFood.Add(antAtFood, antAtFood.homeColony);

        
    }

    void CheckForFight()
    {
        var keys = antsOnFood.Keys.ToList();

        for (int i = 0; i < keys.Count; i++)
        {
            for (int j = i + 1; j < keys.Count; j++)
            {
                AntAgent antA = keys[i];
                AntAgent antB = keys[j];

                if (antsOnFood[antA] != antsOnFood[antB])
                {
                    // Found a pair from different colonies
                    InvokeFight(antA, antB);
                    return; // Exit after initiating the first fight
                }
            }
        }

    }

    public void InvokeFight(AntAgent a, AntAgent b)
    {
        bool fightA = a.WillFight();
        bool fightB = b.WillFight();


    }
}
