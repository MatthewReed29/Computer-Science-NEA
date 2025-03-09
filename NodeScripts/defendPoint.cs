using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.XR;


public class defendPoint : templateNode
{
    // base on how confident you are in the engagement
    // base on cover nearby
    // base on high ground
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0f;

        float cover_preference = 0f;

        List<Tuple<Vector3, float, int>> cover_spots = new List<Tuple<Vector3, float, int>>(decision_script.cover_spots);
        float a = -(2f / 3f) * decision_script.look_for_cover_range;
        float k = (1f / 10f) * Mathf.Pow(a, 2);
        float hold;
        float x = 0;

        foreach (Tuple<Vector3, float, int> spot in cover_spots)
        {
            x = (rel.central_position - spot.Item1).magnitude;
            hold = ((x + a) * Mathf.Sqrt(Mathf.Pow(a, 2) + k)) / (a * Mathf.Sqrt(Mathf.Pow(x + a, 2) + k));
            if (hold > 0)
            {
                cover_preference += hold * spot.Item2;
            }
        }
        bool above = (rel.central_position.y - decision_script.local_enemy_center.y) > rel.height;
        return_value += (cover_preference/ Mathf.Sqrt(Mathf.Pow(cover_preference, 2) + (0.5f * Mathf.Pow(decision_script.look_for_cover_range, 3)))) * 0.5f;
        x = (decision_script.local_team_center - decision_script.local_enemy_center).magnitude;
        return_value += 0.35f * (x / Mathf.Sqrt(Mathf.Pow(x, 2) + (19f / 81f) * Mathf.Pow(decision_script.mid_range * 0.9f, 2)));

        if (above)
        {
            return_value += 0.1f;
        }

        return Mathf.Lerp(return_value, 1, decision_script.impulses[behaviour_name]);
    }
}
