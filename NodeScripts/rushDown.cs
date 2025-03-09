using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class rushDown : templateNode
{
    // do if target is not seen but ghost is seen
    // do if health is low and panick is high (small bump)
    // do if final member of the fireteam (slightly bigger but still small)
    // do if enemy health is lower
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0f;
        transformData target = known.target;
        bool target_away;

        if(target != null)
        {
            target_away = (!target.objects_seen && target.ghost_seen);
        }
        else
        {
            target_away = false;
        }
        
        float h = 1 - Mathf.Pow(rel.health / rel.max_health, 1f/3f);

        float eh;
        if (target != null)
        {
            eh = 1 - Mathf.Pow(target.info.health / target.info.max_health, 1f / 3f);
        }
        else
        {
            eh = 0;
        }

        int lowest_team_value = known.ghost_targets.Count;

        if(lowest_team_value > rel.allies.Count + 1)
        {
            lowest_team_value = rel.allies.Count + 1;
        }

        float team_size_weight = Mathf.Clamp01(1 - ((lowest_team_value - 0.5f) / Mathf.Sqrt(Mathf.Pow(lowest_team_value - 0.5f, 2) + (19f / 81f) * Mathf.Pow(decision_script.teammate_number_confidence + 1, 2))));

        return_value += h * 0.15f;
        return_value += eh * 0.2f;
        return_value += 0.25f * team_size_weight;
        if(target_away)
        {
            return_value += 0.4f;
        }
        if(target != null)
        {
            if ((target.position - rel.central_position).magnitude < decision_script.safe_distance)
            {
                return 0;
            }
        }
        return Mathf.Lerp(return_value, 1, decision_script.impulses[behaviour_name]);
    }
}