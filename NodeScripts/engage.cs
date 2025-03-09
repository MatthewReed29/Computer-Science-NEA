using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class engage : templateNode
{
    // set constant so other nodes compete for their chance but this is the 'default'
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        if(known.ghost_targets.Count > 0 && known.target != null)
        {
            return Mathf.Lerp(0.52f, 1f, decision_script.impulses[behaviour_name]);
        }
        else
        {
            return 0f;
        }
    }
}
