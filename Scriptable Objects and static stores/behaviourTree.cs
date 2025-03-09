using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu]

public class behaviourTree : ScriptableObject
{
    public templateNode root_node;
    public void decision_calc(awareness known, relevant_information rel, ref decisions decision_script)
    {
        decision_script.previous_tree_path.Clear();
        for(int i = 0; i < decision_script.tree_path.Count; i++)
        {
            decision_script.previous_tree_path.Add(decision_script.tree_path[i]);
        }
        decision_script.tree_path.Clear();
        root_node.call_lower(known, rel, ref decision_script);
    }
}
