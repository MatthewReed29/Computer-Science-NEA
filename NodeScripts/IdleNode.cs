using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleNode : templateNode
{
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        return 0.5f;
    }
}
