using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class pursueReady : templateNode
{

    // do if there are more targets
    // do if self is lower health

    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0.1f;

        return_value += 0.4f * (1 - (rel.health / rel.max_health));
        return_value += 0.4f * known.ghost_targets.Count / Mathf.Sqrt(Mathf.Pow(known.ghost_targets.Count, 2) + ((19f / 81f) * Mathf.Pow(decision_script.teammate_number_confidence, 2)));

        return Mathf.Lerp(return_value, 1f, decision_script.impulses[behaviour_name]);
    }

}
