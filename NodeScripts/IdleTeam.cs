using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class IdleTeam : templateNode
{
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        if (rel.allies.Count == 0)
        {
            return 0f;
        }
        else
        {
            return 1f;
        }
    }
}
