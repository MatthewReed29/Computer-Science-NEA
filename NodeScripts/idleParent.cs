using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class idleParent : templateNode
{
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        if (known.ghost_targets.Count == 0)
        {
            return 1f;
        }
        else
        {
            return 0f;
        }
    }
}
