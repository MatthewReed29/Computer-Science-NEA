using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Playables;
using UnityEngine.Rendering;

public class passive : templateNode
{
    // medium range basically mandated
    // prefer on higher healths
    // make more average and made for the others to compete against
    // base on cover with a focus on distance from you as opposed to depth or distance from the player
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0f;
        float x = (decision_script.local_enemy_center - rel.central_position).magnitude;
        float c = decision_script.mid_range * 2;
        float y = Mathf.Pow(((-x) * (x - c) / (Mathf.Pow(0.5f * c, 2))) , 5);
        if(y < 0)
        {
            y = 0;
        }

        return_value += y * 0.5f;
        return_value += (rel.health/rel.max_health) * 0.2f;

        float cover_preference = 0f;

        List<Tuple<Vector3, float, int>> cover_spots = new List<Tuple<Vector3, float, int>>(decision_script.cover_spots);
        float a = -(2f / 3f) * decision_script.look_for_cover_range;
        float k = (1f/10f) * Mathf.Pow(a, 2);
        float hold;

        foreach (Tuple<Vector3, float, int> spot in cover_spots)
        {
            x = (rel.central_position - spot.Item1).magnitude;
            hold = ((x + a) * Mathf.Sqrt(Mathf.Pow(a, 2) + k)) / (a * Mathf.Sqrt(Mathf.Pow(x + a, 2) + k));
            if(hold > 0)
            {
                cover_preference += (hold * 0.5f) + (0.5f * hold * spot.Item2);
            }
        }

        return_value += (cover_preference / Mathf.Sqrt(Mathf.Pow(cover_preference, 2) + (0.5f * Mathf.Pow(decision_script.look_for_cover_range, 3)))) * 0.3f;

        //Debug.Log(return_value);

        return Mathf.Lerp(return_value, 1, decision_script.impulses[behaviour_name] * 0.9f);
    }
}
