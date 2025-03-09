using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Retreat : templateNode
{

    // base on the number of seen enemies
    // base on the number of nearby cover
    // base on closest distance to enemy
    // base on the health you are on
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        if(known.target != null)
        {
            float return_value = 0f;

            float closest_distance = decision_script.mid_range * 2;

            transformData[] ghost_targets = new List<transformData>(known.ghost_targets).ToArray();

            for(int i = 0; i < ghost_targets.Length; i++)
            {
                if((rel.central_position - ghost_targets[i].position).magnitude < closest_distance)
                {
                    closest_distance = (rel.central_position - ghost_targets[i].position).magnitude;
                }
            }

            float d = 1f - (closest_distance / (Mathf.Sqrt(Mathf.Pow(closest_distance, 2) + ((19f/81f) * Mathf.Pow(decision_script.safe_distance , 2)))));

            float cover_weight = 0f;

            Tuple<Vector3, float, int>[] cover_spots = new List<Tuple<Vector3, float, int>>(decision_script.cover_spots).ToArray();

            foreach(Tuple<Vector3, float, int> spot in cover_spots)
            {
                cover_weight += spot.Item2 / ((spot.Item1 - rel.central_position).magnitude + 1f);
            }

            float a = 0f;

            for (int x = -(int)decision_script.look_for_cover_range; x < decision_script.look_for_cover_range + 1; x++)
            {
                for (int z = -(int)decision_script.look_for_cover_range; z < decision_script.look_for_cover_range + 1; z++)
                {
                    a += Mathf.Sqrt((x*x) + (z * z));
                }
            }

            cover_weight = cover_weight / Mathf.Sqrt(Mathf.Pow(cover_weight, 2) + ((19f/81f) * Mathf.Pow(a, 2)));

            float h = 1f -(rel.health / rel.max_health);

            int number_of_seen_enemies = 0;

            foreach(transformData target in ghost_targets)
            {
                if(target.objects_seen)
                {
                    number_of_seen_enemies++;
                }
                if(target.info.player)
                {
                    number_of_seen_enemies += 2;
                }
            }

            float n = number_of_seen_enemies / Mathf.Sqrt(Mathf.Pow(number_of_seen_enemies, 2) + ((19f / 81f) * Mathf.Pow(decision_script.teammate_number_confidence, 2f)));

            return_value += n * 0.3f;
            return_value += (1 - cover_weight) * 0.10f;
            return_value += d * 0.35f;
            return_value += h * 0.3f;

            return Mathf.Lerp(return_value, 1, decision_script.impulses[behaviour_name]);
        }
        else
        {
            return 0f;
        }

    }
}
