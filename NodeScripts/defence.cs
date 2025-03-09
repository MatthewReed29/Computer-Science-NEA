using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;


public class defence : templateNode
{
    // base on the higher number of allies
    // base on being further from enemies with a trail off
    // base on a defence parameter
    // base on hight, make them seem smarter if they hold the high ground (don't do much here)
    // hold your ground more when on higher health
    // base on cover but don't prioritise with very much (could use parameter for this)
    // more common on team having lower health
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0f;
        transformData target = known.target;
        transformData[] ghost_targets = new List<transformData>(known.ghost_targets).ToArray();

        float n = (rel.allies.Count) / Mathf.Sqrt(Mathf.Pow(rel.allies.Count, 2) + 4);
        float x = (decision_script.local_team_center - rel.central_position).magnitude;
        float a = (2 * (5f / 4f) * decision_script.mid_range);
        float d = ((-x) * (x - a) / (Mathf.Pow(0.5f * a, 2)));
        bool above = false;
        if(target != null)
        {
            above = (target.position.y - rel.central_position.y) > rel.height;
        }
        else
        {
            if(ghost_targets.Length > 0)
            {
                above = (ghost_targets[0].position.y - rel.central_position.y) > rel.height;
            }
            else
            {
                return 0f;
            }
        }

        float h = Mathf.Pow(rel.health / rel.max_health, 2);
        float th = 0f;
        relevant_information[] allies = new List<relevant_information>(rel.allies).ToArray();

        foreach(relevant_information ally in allies)
        {
            th += (ally.health / Mathf.Sqrt(Mathf.Pow(ally.health, 2) + 3 * ally.max_health));
        }
        th = th / allies.Length;
        if(allies.Length == 0)
        {
            th = 0f;
        }

        float cover_preference = 0f;

        List<Tuple<Vector3, float, int>> cover_spots = new List<Tuple<Vector3, float, int>>(decision_script.cover_spots);
        float ac = -(2f / 3f) * decision_script.look_for_cover_range;
        float k = (1f / 10f) * Mathf.Pow(ac, 2);
        float hold;

        foreach (Tuple<Vector3, float, int> spot in cover_spots)
        {
            x = (rel.central_position - spot.Item1).magnitude;
            hold = ((x + a) * Mathf.Sqrt(Mathf.Pow(ac, 2) + k)) / (ac * Mathf.Sqrt(Mathf.Pow(x + ac, 2) + k));
            if (hold > 0)
            {
                cover_preference += hold * spot.Item2;
            }
        }

        float c = (x / Mathf.Sqrt(Mathf.Pow(cover_preference, 2) + (0.5f * Mathf.Pow(decision_script.look_for_cover_range, 3)))) * 0.3f;

        return_value += n * 0.15f;
        return_value += d * 0.20f;
        return_value += h * 0.1f;
        return_value += th * 0.10f;
        return_value += c * 0.25f;
        if(above)
        {
            return_value += 0.05f;
        }
        return_value *= 0.85f;
        return Mathf.Lerp(return_value, 0.9f, decision_script.impulses[behaviour_name] * 0.9f);
    }
}
