using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class fleeAllys : templateNode
{
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        if (rel.allies.Count == 0)
        {
            return 0f;
        }
        else
        {
            float return_value = 0;
            float x = (decision_script.local_team_center - rel.central_position).magnitude;
            float y = ((-x) * (x - (2f * (decision_script.team_stay_range_natural)))) / Mathf.Pow(decision_script.team_stay_range_natural, 2);
            if (y < 0)
            {
                y = 0f;
            }
            return_value = rel.allies.Count / Mathf.Sqrt(Mathf.Pow(rel.allies.Count, 2) + ((19f / 81f) * decision_script.outnumbered_weighting)) * 0.4f;
            return_value += y * 0.3f;
            x = (decision_script.local_team_center - decision_script.local_enemy_center).magnitude;
            return_value += (1 - (x / Mathf.Sqrt(Mathf.Pow(x, 2) + ((19f / 81f) * (decision_script.mid_range / 2f))))) * 0.3f;
            return Mathf.Lerp(return_value, 1, decision_script.impulses[behaviour_name] * 0.9f);
        }
    }
}
