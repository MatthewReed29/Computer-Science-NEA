using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class object_script : MonoBehaviour
{
    public float[] perlin_values;
    public float[] perlin_weights;
    public float[] perlin_offset_x;
    public float[] perlin_offset_z;
    public float perlin_scale_x;
    public float perlin_scale_z;
    public float gloabal_magnitude_modifier;
    public float pearlin_threshold;
    public int warping_itterations;

    virtual public Vector3 decide_position(Vector3 input_vertex, int seed)
    {
        System.Random r = new System.Random((int)(seed * input_vertex.x * input_vertex.z));
        input_vertex.x += mesh_generation.thread_random_range(r, -0.3f, 0.3f);
        return input_vertex;
    }

    virtual public bool weight_position(int seed, Vector3 terrain_point)
    {
        float place_poll = perlin_noise_dw(terrain_point, seed).y;
        if (place_poll >= pearlin_threshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    virtual public void sort_nav_and_rotate()
    {

    }

    virtual public Vector3 perlin_noise_dw(Vector3 vertex_input, int seed)
    {
        System.Random r = new System.Random(seed);
        float Xwarp= mesh_generation.thread_random_range(r, 0.5f, 3f);
        float Zwarp = mesh_generation.thread_random_range(r, 0.5f, 3f);
        Vector2 store_vect = new Vector2();
        for (int w = 0; w < warping_itterations; w++)
        {
            store_vect = new Vector2(perlin_noise_point(vertex_input.x + store_vect.x, vertex_input.z + store_vect.y), 
                perlin_noise_point(vertex_input.x + Xwarp + store_vect.x, vertex_input.z + Zwarp + store_vect.y));
            Xwarp += mesh_generation.thread_random_range(r, -1, 1);
            Zwarp += mesh_generation.thread_random_range(r, -1, 1);
        }
        vertex_input.y = perlin_noise_point(vertex_input.x + store_vect.x, vertex_input.z + store_vect.y);
        return vertex_input;
    }

    float perlin_noise_point(float x, float z)
    {
        float y = 0;
        for (int layer_index = 0; layer_index < perlin_values.Length; layer_index++)
        {
            y = y + Mathf.Abs(Mathf.PerlinNoise(perlin_offset_x[layer_index] + ((x) * perlin_values[layer_index] / perlin_scale_x), 
                perlin_offset_z[layer_index] + (z) * perlin_values[layer_index] / perlin_scale_z)) * perlin_weights[layer_index] * gloabal_magnitude_modifier;
        }
        return y;
    }

    public void setup_offsets(int seed)
    {
        seed = seed * seed;
        System.Random r = new System.Random(seed);
        for (int i = 0; i < perlin_offset_x.Length; i++)
        {
            perlin_offset_x[i] = mesh_generation.thread_random_range(r, -1000f, 1000f);
            perlin_offset_z[i] = mesh_generation.thread_random_range(r, -1000f, 1000f);
        }
    }

}
