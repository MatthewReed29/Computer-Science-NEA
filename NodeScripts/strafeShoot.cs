using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class strafeShoot : templateNode
{
    // default compared to cover shoot
    // prefer based on other teammates doing coverCombat (don't want the same spots used)
    // slight lean towards when on on high health
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0.8f;

        int local_team = 0;

        relevant_information[] allies = new List<relevant_information>(rel.allies).ToArray();

        for (int i = 0; i < allies.Length; i++)
        {
            if((allies[i].central_position - rel.central_position).magnitude <= (2f/3f) * known.seeing_range)
            {
                local_team++;
            }
        }

        float t = (float)local_team / Mathf.Sqrt(Mathf.Pow(local_team, 2) + ((19f/81f) * decision_script.teammate_number_confidence));

        float h = (rel.health / rel.max_health);

        return_value *= t * h;

        return Mathf.Lerp(return_value, 1f, decision_script.impulses[behaviour_name]);
    }
}
