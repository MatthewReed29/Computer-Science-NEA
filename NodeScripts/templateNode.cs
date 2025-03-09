using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class templateNode : ScriptableObject
{
    public templateNode[] children;
    public string behaviour_name = "idle";

    abstract public float poll_priority(awareness known, relevant_information rel, decisions decision_script);

    virtual public void call_lower(awareness known, relevant_information rel, ref decisions decision_script)
    {
        if(children.Length > 0)
        {
            float[] poll_weights = new float[children.Length];
            for (int child_index = 0; child_index < children.Length; child_index++)
             {
                poll_weights[child_index] = children[child_index].poll_priority(known, rel, decision_script);
            }
            float highest = 0f;
            int highest_index = 0;
            for (int decide_choice = 0; decide_choice < poll_weights.Length; decide_choice++)
            {
                if (highest < poll_weights[decide_choice])
                {
                    highest = poll_weights[decide_choice];
                    highest_index = decide_choice;
                }
            }
            apply_impulse(rel, ref decision_script, children[highest_index].behaviour_name);
            children[highest_index].call_lower(known, rel, ref decision_script);
        }
        else
        {
            decision_script.state = behaviour_name;
        }
        

    }

    virtual public void apply_impulse(relevant_information rel, ref decisions decision_script, string child)
    {
        bool first_impulse = true;
        decision_script.tree_path.Add(child);
        foreach(string name in decision_script.previous_tree_path)
        {
            if(name == child)
            {
                first_impulse = false;
                break;
            }
        }
        if(first_impulse)
        {
            decision_script.impulses[child] = 1f;
            for(int i = 0; i < children.Length; i++)
            {
                if (children[i].behaviour_name != child)
                {
                    decision_script.impulses[children[i].behaviour_name] = 0f;
                }
            }
        }
        else
        {
            decision_script.impulses[child] -= decision_script.impulse_weights[child] * decision_script.time_delta;
            if(decision_script.impulses[child] < 0f)
            {
                decision_script.impulses[child] = 0f;
            }
        }
    }
}

