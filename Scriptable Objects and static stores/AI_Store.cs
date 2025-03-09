using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;


public static class AI_Store
{
    private static List<decisions> characters = new List<decisions>();
    private static Thread decision_thread;
    public static void remove_target(GameObject g)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i] == g.GetComponent<decisions>())
            {
                characters.RemoveAt(i);
            }
        }
    }

    public static void add_target(GameObject g)
    {
        characters.Add(g.GetComponent<decisions>());
    }

    public static void start()
    {
        decision_thread = new Thread(() => poll_decisions());
        decision_thread.Start();
    }

    public static void end()
    {
        decision_thread.Abort();
        characters.Clear();
    }

    private static void poll_decisions()
    {
        while(true)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            for(int i = 0; i < characters.Count; i++)
            {
                if (characters[i] != null)
                {
                    characters[i].get_decision();
                }
            }

            timer.Stop();
            if(timer.ElapsedMilliseconds < 500)
            {
                Thread.Sleep(500 - (int)(timer.ElapsedMilliseconds));
            }
        }
    }

    
}
