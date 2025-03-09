using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tree_placement1 : object_script
{
    public float max_height;

    public override bool weight_position(int seed, Vector3 terrain_point)
    {
        if(terrain_point.y > max_height)
        {
            return false;
        }
        else
        {
            return base.weight_position(seed, terrain_point);
        }
    }

    public override Vector3 decide_position(Vector3 input_vertex, int seed)
    {
        System.Random r = new System.Random((int)(seed * input_vertex.x * input_vertex.z));
        input_vertex.x += mesh_generation.thread_random_range(r, -0.6f, 0.6f);
        return input_vertex;
    }

}
