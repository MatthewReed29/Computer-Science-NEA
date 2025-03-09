using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.AI;


public class flee : templateNode
{
    //weight if outnumbered, if there is little cover, is health is low
    //protect from fleeing when no enemies are seen

    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0f;

        float cover_confidence = 0f;

        for (int i = 0; i < decision_script.cover_spots.Count; i++)
        {
            cover_confidence += decision_script.cover_spots[i].Item2 * Mathf.Clamp((decision_script.cover_spots[i].Item1 - rel.central_position).magnitude, 1f, decision_script.look_for_cover_range / 1.25f);
        }

        cover_confidence = (cover_confidence / Mathf.Sqrt((Mathf.Pow(cover_confidence, 2) + Mathf.Pow(decision_script.look_for_cover_range / decision_script.cover_scale, 2))));

        float h = 1 - (rel.health / rel.max_health);

        float outnumbered = (known.ghost_targets.Count / (rel.allies.Count+ 1)) / 
            Mathf.Sqrt(Mathf.Pow(known.ghost_targets.Count / (rel.allies.Count + 1), 2) + ((19f/81f )* (Mathf.Pow(decision_script.teammate_number_confidence / decision_script.outnumbered_weighting, 2))));

        List<transformData> copy_ghosts = new List<transformData>(known.ghost_targets);
        bool one_seen = false;
        foreach(transformData td in copy_ghosts)
        {
            if(td.objects_seen)
            {
                one_seen = true;
            }
        }

        if(!one_seen)
        {
            return Mathf.Lerp(0f, 1f, decision_script.impulses[behaviour_name]);
        }

        return_value += 0.15f * (1 - cover_confidence);
        return_value += 0.3f * h;
        return_value += 0.2f * outnumbered;

        return Mathf.Lerp(return_value, 1f, decision_script.impulses[behaviour_name]);
        
    }
}
