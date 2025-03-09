using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class coverRetreatShooting : templateNode
{
    public override float poll_priority(awareness known, relevant_information rel, decisions decision_script)
    {
        float return_value = 0f;
        if (decision_script.cover_spots.Count > 0)
        {
            float depth1;
            float depth2;
            float weight1 = 0.0001f;
            float weight2;
            List<Tuple<Vector3, float, int>> cover_spots = new List<Tuple<Vector3, float, int>>(decision_script.cover_spots);
            Tuple<Vector3, float, int> best_spot = cover_spots[0];
            foreach (Tuple<Vector3, float, int> spot in cover_spots)
            {
                if ((best_spot.Item3 - 2) <= 0)
                {
                    depth1 = 0.08f * best_spot.Item3;
                }
                else
                {
                    depth1 = best_spot.Item3 - 2;
                }

                if ((spot.Item3 - 2) <= 0)
                {
                    depth2 = 0.08f * best_spot.Item3;
                }
                else
                {
                    depth2 = spot.Item3 - 2;
                }
                weight1 = ((Mathf.Pow(best_spot.Item2, 5)) * (Mathf.Pow(depth1, 0.5f))) / (1+Mathf.Pow((best_spot.Item1 - rel.central_position).magnitude, 1f / 2f));
                weight2 = ((Mathf.Pow(spot.Item2, 5)) * (Mathf.Pow(depth2, 0.5f))) / (1+Mathf.Pow((spot.Item1 - rel.central_position).magnitude, 1f / 2f));
                if (weight2 > weight1)
                {
                    best_spot = spot;
                }
            }

            return_value = Mathf.Sqrt(weight1 / (Mathf.Sqrt(Mathf.Pow(weight1, 2) + ((19f / 81f) * Mathf.Pow(3, 2)))));

            return Mathf.Lerp(return_value, 1, decision_script.impulses[behaviour_name]);
        }
        else
        {
            return 0f;
        }
    }
}
