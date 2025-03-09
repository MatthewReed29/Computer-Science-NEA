using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class shootingRetreat : templateNode
{
    //favoured over fleeing when there is more cover
    //favoured when enemies are further away
    //favoured when more allies to retreat to
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0.2f;

        float t = Mathf.Pow((rel.allies.Count + 1) / Mathf.Sqrt(Mathf.Pow((rel.allies.Count + 1), 2) + (19f/81f) * Mathf.Pow(decision_script.teammate_number_confidence, 2)), 2);

        float cover_confidence = 0;
        for (int i = 0; i < decision_script.cover_spots.Count; i++)
        {
            cover_confidence += decision_script.cover_spots[i].Item2;
        }

        float closest_distance = 0f;

        transformData[] ghost_targets = known.ghost_targets.ToArray();

        if(ghost_targets.Length > 0)
        {
            closest_distance = (rel.central_position - ghost_targets[0].position).magnitude;
        }

        for(int i = 1; i < ghost_targets.Length; i++)
        {
            if (ghost_targets[i].objects_seen)
            {
                if(closest_distance > (rel.central_position - ghost_targets[i].position).magnitude)
                {
                    closest_distance = (rel.central_position - ghost_targets[0].position).magnitude;
                }
            }
        }

        closest_distance = 1 - (closest_distance / Mathf.Sqrt(Mathf.Pow(closest_distance, 2) + ((19f / 81f) * Mathf.Pow(decision_script.safe_distance * 2, 2))));

        cover_confidence = (cover_confidence / Mathf.Sqrt((Mathf.Pow(cover_confidence, 2) + Mathf.Pow(decision_script.look_for_cover_range / decision_script.cover_scale, 2))));

        return_value += Mathf.Pow(cover_confidence, 2) * 0.3f;
        return_value += closest_distance * 0.15f;
        return_value += t * 0.35f;

        return_value = Mathf.Clamp01(return_value);
        return Mathf.Lerp(return_value ,1f, decision_script.impulses[behaviour_name]);
    }
}
