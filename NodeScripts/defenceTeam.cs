using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class defenceTeam : templateNode
{
    // weight based on teammate proximity and health
    // maybe also throw in a check for the state being a retreat like one
    // make so you defend teammates if they are weak or running
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        if(rel.allies.Count > 0)
        {
            float return_value = 0f;
            relevant_information[] allies = new List<relevant_information>(rel.allies).ToArray();
            float min_health = 1f;
            int count = 0;
            float weighted_average_health = 0f;
            float weighted_average_count = 0f;

            for (int i = 0; i < allies.Length; i++)
            {
                if (allies[i].alive)
                {
                    if ((allies[i].health / allies[i].max_health) < min_health)
                    {
                        min_health = allies[i].health / allies[i].max_health;
                    }
                    count++;
                    weighted_average_health += (1 / Mathf.Pow(allies[i].health / allies[i].max_health, 2)) * (allies[i].health / allies[i].max_health);
                    weighted_average_count += (1 / Mathf.Pow(allies[i].health / allies[i].max_health, 2));
                }
            }

            weighted_average_health /= weighted_average_count;

            Vector3 help_map_average = Vector3.zero;
            weighted_average_count = 0f;


            for (int i = 0; i < allies.Length; i++)
            {
                if ((allies[i].central_position - rel.central_position).magnitude < (1f/2f) * known.seeing_range)
                {
                    help_map_average += allies[i].central_position * ((1 + (1 - (allies[i].health / allies[i].max_health))) / 2f);
                    weighted_average_count += ((1 + (1 - (allies[i].health / allies[i].max_health))) / 2f);
                }
            }
            help_map_average /= weighted_average_count;

            bool retreating_team = false;

            foreach(relevant_information ally in allies)
            {
                if((ally.central_position - ally.central_position).magnitude < known.seeing_range / 2f)
                {
                    if(ally.decision_script.state.Substring(0, Mathf.Min(7, ally.decision_script.state.Length)).ToLower() == "retreat" || 
                        ally.decision_script.state.Substring(0, Mathf.Min(4, ally.decision_script.state.Length)).ToLower() == "flee")
                    {
                        retreating_team = true;
                    }
                }
            }

            float k = Mathf.Pow(decision_script.team_stay_range_natural, 2) * (19f / 81f);
            float x = (rel.central_position - help_map_average).magnitude;

            return_value += 0.35f * (((x - decision_script.team_stay_range_natural) * Mathf.Sqrt(Mathf.Pow(decision_script.team_stay_range_natural, 2) + k)) / 
                (decision_script.team_stay_range_natural * Mathf.Sqrt(Mathf.Pow((x - decision_script.team_stay_range_natural), 2) + k)));
            return_value += 0.5f * (1 - weighted_average_health);
            if(retreating_team)
            {
                return_value += 0.15f;
            }
            return Mathf.Lerp(return_value, 1, decision_script.impulses[behaviour_name]);
        }
        else
        {
            return 0f;
        }
    }
}
