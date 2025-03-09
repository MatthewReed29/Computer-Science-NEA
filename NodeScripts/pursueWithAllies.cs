using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class pursueWithAllies : templateNode
{
    // more common if near allies
    // more common if other allies near you are doing so
    // more common if you there are more known opponents
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0.1f;
        List<int> nearby_allies = new List<int>();
        relevant_information[] allies = new List<relevant_information>(rel.allies).ToArray();

        for (int i = 0; i < allies.Length; i++)
        {
            if ((allies[i].central_position - rel.central_position).magnitude <= decision_script.team_stay_range_natural)
            {
                nearby_allies.Add(i);
            }
        }

        float a = nearby_allies.Count / Mathf.Sqrt(Mathf.Pow(nearby_allies.Count, 2) + ((19f / 81f) * decision_script.teammate_number_confidence));

        float same_decision = 0f;

        for (int i = 0; i < nearby_allies.Count; i++)
        {
            foreach(templateNode child in children)
            {
                if(allies[nearby_allies[i]].decision_script.state == child.behaviour_name)
                {
                    same_decision += 1f;
                    break;
                }
            }
        }
        same_decision = same_decision / Mathf.Sqrt(Mathf.Pow(same_decision, 2) + ((19f / 81f) * (decision_script.teammate_number_confidence / 2f)));

        List<transformData> targets = new List<transformData>(known.ghost_targets);
        List<transformData> not_seen_targets = new List<transformData>();

        for(int i = 0; i < targets.Count; i++)
        {
            if(!targets[i].objects_seen)
            {
                not_seen_targets.Add(targets[i]);
            }
        }
        float n = not_seen_targets.Count / Mathf.Sqrt(Mathf.Pow(not_seen_targets.Count, 2) + ((19f/81f) * Mathf.Pow(allies.Length + 1, 2)));

        return_value += n * 0.2f;
        return_value += same_decision * 0.3f;
        return_value += a * 0.4f;

        return Mathf.Lerp(return_value, 1, decision_script.impulses[behaviour_name]);
    }
}
