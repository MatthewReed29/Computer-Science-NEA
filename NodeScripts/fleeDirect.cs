using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class fleeDirect : templateNode
{
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0.5f;
        if(decision_script.cover_spots.Count > 0)
        {
            float depth1;
            float depth2;
            float weight1 = 0.0001f;
            float weight2;
            List<Tuple<Vector3, float, int>> cover_spots = new List<Tuple<Vector3, float, int>>(decision_script.cover_spots);
            Tuple<Vector3, float, int> best_spot = cover_spots[0];
            foreach (Tuple<Vector3, float, int> spot in cover_spots)
            {
                if ((best_spot.Item3 - 2) <= 0)
                {
                    depth1 = 1f / (4f - best_spot.Item3);
                }
                else
                {
                    depth1 = best_spot.Item3 - 2;
                }
                if ((spot.Item3 - 2) <= 0)
                {
                    depth2 = 1f / (4f - spot.Item3);
                }
                else
                {
                    depth2 = spot.Item3 - 2;
                }
                weight1 = ((Mathf.Pow(1 + best_spot.Item2, 2) / 4f) * (Mathf.Pow(depth1, 1.5f))) / Mathf.Pow((best_spot.Item1 - rel.central_position).magnitude, 1f / 2f);
                weight2 = ((Mathf.Pow(1 + spot.Item2, 2) / 4f) * (Mathf.Pow(depth2, 1.5f))) / Mathf.Pow((spot.Item1 - rel.central_position).magnitude, 1f / 2f);
                if (weight2 > weight1)
                {
                    best_spot = spot;
                }
            }
            return_value = return_value / 2f;
            return_value += return_value * (1 - Mathf.Sqrt(weight1 / Mathf.Sqrt(Mathf.Pow(weight1, 2) + 20)));
        }
        if ((decision_script.local_team_center - rel.central_position).magnitude < decision_script.team_stay_range_natural || rel.allies.Count == 0)
        {
            return_value *= 1.1f;
        }

        List<transformData> ghost_targets = new List<transformData>(known.ghost_targets);
        float closest_ditance = (rel.central_position - known.target.position).magnitude;
        for(int i = 0; i < ghost_targets.Count; i++)
        {
            if((rel.central_position - ghost_targets[i].position).magnitude < closest_ditance)
            {
                closest_ditance = (rel.central_position - ghost_targets[i].position).magnitude;
            }
        }
        return_value += 1 - (closest_ditance / (Mathf.Sqrt(Mathf.Pow(closest_ditance, 2) + ((19f / 81f) * Mathf.Pow(decision_script.safe_distance * 2, 2)))));
        return_value /= 2;
        return Mathf.Clamp01(Mathf.Lerp(return_value, 1, decision_script.impulses[behaviour_name] * 0.9f));
    }
}
