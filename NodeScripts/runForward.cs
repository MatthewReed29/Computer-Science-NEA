using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class runForward : templateNode
{
    // do if numbers advantage
    // do if further away
    // do if other team is spread out (push advantage)
    // do if team is generally high health
    // do if much of the enemy team object not seen (precuror behaviour to a later peruse), no reference to target
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0f;

        bool numbers_advantage = (known.ghost_targets.Count < rel.allies.Count - 1);

        float x = (decision_script.local_enemy_center - decision_script.local_team_center).magnitude;
        float d = x / Mathf.Sqrt(Mathf.Pow(x, 2) + ((19f / 81f) * Mathf.Pow(decision_script.mid_range , 2)));

        transformData[] targets = new List<transformData>(known.ghost_targets).ToArray();
        float average_enemy_spread = 0;
        int average_count = 0;

        for (int i = 0; i < targets.Length; i++)
        {
            if ((targets[i].position - rel.central_position).magnitude < (2f / 3f) * known.seeing_range)
            {
                average_enemy_spread += (targets[i].position - decision_script.local_enemy_center).magnitude;
                average_count++;
            }
        }

        float s = 1f - (average_enemy_spread / Mathf.Sqrt(Mathf.Pow(average_enemy_spread, 2) + ((19f / 81f) * Mathf.Pow(decision_script.mid_range * (3f/4f), 2))));

        float average_team_health = rel.health / rel.max_health;
        average_count = 1;
        relevant_information[] allies = new List<relevant_information>(rel.allies).ToArray();

        for (int i = 0; i < allies.Length; i++)
        {
            average_team_health += allies[i].health / allies[i].max_health;
            average_count++;
        }

        average_team_health /= average_count;
        if (average_count == 0)
        {
            average_team_health = 0f;
        }
        average_team_health = Mathf.Pow(average_team_health, 3f / 2f);

        int number_infront = 0;
        float number_not_seen = 0;

        for (int i = 0; i < targets.Length; i++)
        {
            if (Vector3.Dot(rel.forward, targets[i].position - rel.central_position) > -10)
            {
                number_infront++;
                if(!targets[i].objects_seen)
                {
                    number_not_seen += 1f;
                }
            }
        }

        number_not_seen /= number_infront;

        return_value += 0.25f * d;
        return_value += 0.25f * s;
        if(numbers_advantage)
        {
            return_value += 0.15f;
        }
        return_value += 0.15f * average_team_health;
        return_value += 0.2f * number_not_seen;

        return_value = Mathf.Clamp01(return_value);
        return Mathf.Lerp(return_value, 1, decision_script.impulses[behaviour_name]);
    }
}
