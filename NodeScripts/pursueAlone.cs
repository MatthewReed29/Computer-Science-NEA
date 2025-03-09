using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class pursueAlone : templateNode
{
    // do if there is less enemies
    // (generally stick around 0.4 - 0.6 and let the other node overtake if they need)

    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float x = (known.ghost_targets.Count);
        return 0.45f + (0.2f * (1 - (x / Mathf.Sqrt(Mathf.Pow(x, 2) + ((19f/81f) * Mathf.Pow(rel.allies.Count, 2))))));
    }
}
