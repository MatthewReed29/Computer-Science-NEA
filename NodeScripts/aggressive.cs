using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aggressive : templateNode
{
    // equally as likely on all healths 
    // prefered when the opponent is lower health
    // prefered when there is a numbers advantage (bare in mind this is always whern there is a player)
    // should prefer medium distance but discourage extremes (ideally closer distances)
    // this is not the persue node
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0f;
        transformData[] ghost_targets = new List<transformData>(known.ghost_targets).ToArray();
        transformData target = known.target;
        float h;
        if(target != null)
        {
            h = 1 - Mathf.Pow(known.target.info.health / known.target.info.max_health, 2.5f);
        }
        else
        {
            h = 0f;
        }
        int x = 1;
        for(int i = 0; i < ghost_targets.Length; i++)
        {
            if (globalTracker.isPlayer(ghost_targets[i].host))
            {
                x -= 2;
            }
            else
            {
                x--;
            }
        }
        for(int i = 0; i < rel.allies.Count; i++)
        {
            if (globalTracker.isPlayer(rel.allies[i].host))
            {
                x += 2;
            }
            else
            {
                x++;
            }
        }
        float e = Mathf.Pow((((x) / Mathf.Sqrt(Mathf.Pow(x, 2f) + (10 * decision_script.outnumbered_weighting))) + 1f) / 2f, 2);

        float bx = (decision_script.local_team_center - rel.central_position).magnitude;
        float c = decision_script.mid_range * 1.5f;
        float k = 19f / 81f * Mathf.Pow(c, 2);
        float y = ((bx - c) * Mathf.Sqrt(Mathf.Pow(c, 2) + k) / (c * Mathf.Sqrt(Mathf.Pow(bx + c, 2) + k)));
        if (y < 0)
        {
            y = 0;
        }
        return_value += y * 0.25f;
        return_value += h * 0.35f;
        return_value += e * 0.35f;
        return Mathf.Lerp(return_value, 1, decision_script.impulses[behaviour_name] * 0.9f);
    }
}
