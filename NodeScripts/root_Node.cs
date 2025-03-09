using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class root_Node : templateNode
{
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        return 0f;
    }
}