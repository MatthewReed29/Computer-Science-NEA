using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class globalTracker
{
    public static GameObject player;
    public static List<relevant_information> knowables = new List<relevant_information>();
    public static void remove_target(GameObject g)
    {
        for(int i = 0; i < knowables.Count; i++)
        {
            if (knowables[i] == g.GetComponent<relevant_information>())
            {
                knowables.RemoveAt(i);
            }
            if(g == player)
            {
                player = null;
            }
        }
    }

    public static void add_target(GameObject g)
    {
        knowables.Add(g.GetComponent<relevant_information>());
        if(g.tag == "player")
        {
            player = g;
        }
    }

    public static bool isPlayer(GameObject g)
    {
        if(player == null)
        {
            return false;
        }
        else if (g == player)
        {
            return true;
        }
        return false;

    }

    public static void Clear()
    {
        player = null;
        knowables.Clear();
    }

}
