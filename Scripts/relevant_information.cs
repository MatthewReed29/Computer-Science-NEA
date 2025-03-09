using System.Collections.Generic;
using UnityEngine;

public class relevant_information : MonoBehaviour
{
    public int allegiance;
    public bool alive;
    public bool player;
    public int score = 0;
    public Vector3 central_position;
    public Vector3 central_offset;
    public List<relevant_information> allies;
    public List<Transform> view_points;
    public Transform eye_level;
    public Vector3 forward;
    public attacks attacks;
    public float height;
    public float health;
    public float max_health;
    public decisions decision_script;
    public awareness known;
    public GameObject host;

    public void SetAllegiance(int input)
    {
        allegiance = input;
        SkinnedMeshRenderer[] renders = this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        Color team_color = new Color();
        System.Random rand = new System.Random(allegiance);

        team_color.r = (float)rand.NextDouble();
        team_color.g = (float)rand.NextDouble();
        team_color.b = (float)rand.NextDouble();
        team_color.a = 1;
        foreach (SkinnedMeshRenderer mesh_renderer in renders)
        {
            mesh_renderer.material.color = team_color;
        }
    }

    void Update()
    {
        central_position = transform.position + central_offset;
        forward = eye_level.forward;
        for (int i = 0; i < allies.Count; i++)
        {
            if (!allies[i].alive)
            {
                allies.RemoveAt(i);
            }
        }
    }

    private void Awake()
    {
        decision_script = GetComponent<decisions>();
        alive = true;
        attacks = GetComponent<attacks>();
        known = GetComponent<awareness>();
        host = this.gameObject;
    }


    public void recieve_teamate_info(awareness information)
    {
        bool new_addition;
        for(int i = 0; i < information.ghost_targets.Count; i++)
        {
            new_addition = true;
            for(int j = 0; j < known.ghost_targets.Count; j++)
            {
                if (known.ghost_targets[j].host == information.ghost_targets[i].host)
                {
                    if (information.ghost_targets[i].last_updated > known.ghost_targets[j].last_updated)
                    {
                        known.ghost_targets[j].recieve_shared(information.ghost_targets[i]);
                    }
                    new_addition = false;
                }
            }
            if(new_addition)
            {
                known.ghost_targets.Add(information.ghost_targets[i]);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(central_position, 0.2f);
    }

}
