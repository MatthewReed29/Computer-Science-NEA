using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class coverCombat : templateNode
{
    // do when there is close cover (doesn't have to be that close, it can be travelled to)
    // do when emey is not very close (leave some distance, e.g. mid range)
    // prefer when self on low health
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        if (decision_script.cover_spots.Count == 0)
        {
            return 0f;
        }
        else
        {
            float return_value = 0f;
            Tuple<Vector3, float, int>[] cover_spots = new List<Tuple<Vector3, float, int>>(decision_script.cover_spots).ToArray();

            Tuple<Vector3, float, int> best_spot = cover_spots[0];
            for (int i = 1; i < cover_spots.Length; i++)
            {
                if ((cover_spots[i].Item1 - rel.central_position).magnitude * (2 - cover_spots[i].Item2) * (cover_spots[i].Item1 - rel.central_position).magnitude < 
                    (best_spot.Item1 - rel.central_position).magnitude * (2 - best_spot.Item2) * (best_spot.Item1 - rel.central_position).magnitude && cover_spots[i].Item3 <= 2)
                {
                    best_spot = cover_spots[i];
                }
            }

            float d = (best_spot.Item1 - rel.central_position).magnitude / Mathf.Sqrt(Mathf.Pow((best_spot.Item1 - rel.central_position).magnitude, 2) + ((19f / 81f) * Mathf.Pow(decision_script.cover_consideration_distance, 2)));
            float enemy_distance;
            if (known.target != null)
            {
                enemy_distance = (known.target.position - rel.central_position).magnitude;
            }
            else
            {
                enemy_distance = (decision_script.local_enemy_center - rel.central_position).magnitude;
            }
            
            transformData[] ghost_targets = new List<transformData>(known.ghost_targets).ToArray();
            for(int i = 0; i < ghost_targets.Length; i++)
            {
                if(enemy_distance > (ghost_targets[i].position - rel.central_position).magnitude)
                {
                    enemy_distance = (ghost_targets[i].position - rel.central_position).magnitude;
                }
            }

            float y = enemy_distance / Mathf.Sqrt(Mathf.Pow(enemy_distance, 2) + (19f / 81f) * decision_script.safe_distance);
            float h = 1 - (rel.health / rel.max_health);

            return_value += d * 0.45f;
            return_value += y * 0.35f;
            return_value += h * 0.2f;

            return Mathf.Lerp(return_value, 1, decision_script.impulses[behaviour_name]);
        }

    }
}
