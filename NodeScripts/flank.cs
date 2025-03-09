using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class flank : templateNode
{
    //make almost impossible if other teammate is flanking (otherwise it isn't a flank)
    //if enemy team and allies are closely bunched up (helps spread the teams out with this behaviour)
    //if there are relatively similar numbers of enemies and allies (use behaviour as a way to break even engagements)
    //make unlikely; behaviour involves breaking combat so this should happen infrequently
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0f;
        relevant_information[] allies = new List<relevant_information>(rel.allies).ToArray();

        bool shared_decision = false;

        for (int i = 0; i < allies.Length; i++)
        {
            if (allies[i].decision_script.state == behaviour_name)
            {
                shared_decision = true;
                break;
            }
        }

        transformData[] targets = new List<transformData>(known.ghost_targets).ToArray();
        float average_enemy_spread = 0;
        int average_count = 0;

        for (int i = 0; i < targets.Length; i++)
        {
            if ((targets[i].position - rel.central_position).magnitude < (2f / 3f) * known.seeing_range)
            {
                average_enemy_spread += (targets[i].position - decision_script.local_enemy_center).magnitude;
                average_count++;
            }
        }

        average_enemy_spread /= average_count;

        average_count = 0;
        float average_team_spread = 0f;

        for (int i = 0; i < allies.Length; i++)
        {
            if ((allies[i].central_position - rel.central_position).magnitude < (2f / 3f) * known.seeing_range)
            {
                average_team_spread += (allies[i].central_position - decision_script.local_team_center).magnitude;
                average_count++;
            }
        }

        average_team_spread /= average_count;

        int nearby_enemies = 0;

        for(int i = 0; i < targets.Length; i++)
        {
            if(known.target != null)
            {
                if ((targets[i].position - known.target.position).magnitude < decision_script.mid_range)
                {
                    nearby_enemies++;
                }
            }
            else
            {
                nearby_enemies++;
            }
        }

        int team_different = Mathf.Abs(nearby_enemies - rel.allies.Count - 1);

        float b = Mathf.Exp(-Mathf.Pow(((float)team_different), 2));

        float percentage_cover_spots = 0f;

        if(decision_script.cover_spots.Count > 0)
        {
            Tuple<Vector3, float, int>[] cover_spots = new List<Tuple<Vector3, float, int>>(decision_script.cover_spots).ToArray();

            foreach (Tuple<Vector3, float, int> spot in cover_spots)
            {
                if (spot.Item2 > 0.3f)
                {
                    percentage_cover_spots += 1.1f;
                }
            }
            percentage_cover_spots /= Mathf.Pow(decision_script.look_for_cover_range * 2, 2);

            percentage_cover_spots = Mathf.Sqrt(percentage_cover_spots);
        }



        if(!shared_decision)
        {
            return_value += 0.08f;
        }
        else
        {
            return_value -= 0.3f;
        }

        return_value += b * 0.2f;
        if(rel.allies.Count == 0)
        {
            return_value -= 0.15f;
        }else if(known.ghost_targets.Count == 1)
        {
            return_value += 0.2f * (1f - average_team_spread / Mathf.Sqrt(Mathf.Pow(average_team_spread, 2) + (19f / 81f) * Mathf.Pow((decision_script.team_stay_range_max + decision_script.team_stay_range_natural / 2), 2)));
        }
        else
        {
            return_value += 0.2f * (1f - average_team_spread / Mathf.Sqrt(Mathf.Pow(average_team_spread, 2) + (19f / 81f) * Mathf.Pow((decision_script.team_stay_range_max + decision_script.team_stay_range_natural / 2), 2)));
            return_value += 0.25f * (1f - average_enemy_spread / Mathf.Sqrt(Mathf.Pow(average_enemy_spread, 2) + (19f / 81f) * Mathf.Pow((decision_script.team_stay_range_max + decision_script.team_stay_range_natural / 2), 2)));
        }

        return_value += percentage_cover_spots * 0.25f;


        return Mathf.Lerp(return_value, 1, decision_script.impulses[behaviour_name]);
    }
}
