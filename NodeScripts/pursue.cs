using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class pursue : templateNode
{
    // if you can't see target 
    // if you can't see many ghost targets
    // if you are higher health
    // if there are less enemies (more often used against player)
    // if the enemies are far away (maybe take away at close ranges to discourage close range weird behaviours)
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0f;
        if(known.target != null && known.ghost_targets.Count != 0)
        {

            bool target_seen = known.target.objects_seen;

            transformData[] ghost_targets = new List<transformData>(known.ghost_targets).ToArray();

            float cant_see_percentage = 0f;

            foreach(transformData ghost in ghost_targets)
            {
                if(!ghost.objects_seen)
                {
                    cant_see_percentage++;
                }
            }
            cant_see_percentage /= ghost_targets.Length;

            float h = rel.health / rel.max_health;

            float n = 1 - (ghost_targets.Length / Mathf.Sqrt(Mathf.Pow(ghost_targets.Length, 2) + ((19f/81f) * Mathf.Pow(decision_script.teammate_number_confidence, 2))));

            float x = (known.target.position - rel.central_position).magnitude;
            float d = Mathf.Sqrt(x / Mathf.Sqrt(Mathf.Pow(x, 2) + ((19f/81f) * Mathf.Pow(decision_script.mid_range * 1.5f, 2))));

            return_value += h * 0.05f;
            return_value += n * 0.25f;
            return_value += d * 0.3f;
            return_value += cant_see_percentage * 0.20f;
            if(!target_seen)
            {
                return_value += 0.2f;
            }

            if(decision_script.state.Substring(0, Mathf.Min(decision_script.state.Length, 4)).ToLower() == "flee")
            {
                return return_value *= 0.1f;
            }
            return Mathf.Lerp(return_value, 1f, decision_script.impulses[behaviour_name]);
        }
        else
        {
            return 0f;
        }
    }
}
