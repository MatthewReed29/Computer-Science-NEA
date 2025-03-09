using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[CreateAssetMenu]
public class biome : ScriptableObject
{
    public float[] decide_biome_perlin_values;
    public float[] decide_biome_perlin_weights;
    public float[] decide_biome_perlin_offset_x;
    public float[] decide_biome_perlin_offset_z;
    public float[] terrain_perlin_values;
    public float[] terrain_perlin_weights;
    public float terrain_heigh_modifier;
    public float gloabal_magnitude_modifier;
    public float warping_itterations;
    //public Material[] textures;
    public Color color;
    public GameObject[] placeable_objects;
    public float thingy;
    public float pow;
    public float thresh;

    public float poll_biome_weight(float x, float z, float perlin_scale_x, float perlin_scale_z, int seed)
    {
        System.Random gen = new System.Random(seed);

        decide_biome_perlin_weights = mesh_generation.list_sum_one(decide_biome_perlin_weights.ToList<float>()).ToArray();
        
        float y = 0;
        for (int layer_index = 0; layer_index < decide_biome_perlin_values.Length; layer_index++)
        {
            y = y + Mathf.PerlinNoise(decide_biome_perlin_offset_x[layer_index] + (((float)x) * decide_biome_perlin_values[layer_index] / perlin_scale_x),
                decide_biome_perlin_offset_z[layer_index] + ((float)z) * decide_biome_perlin_values[layer_index] / perlin_scale_z) * decide_biome_perlin_weights[layer_index];
        }
        x += (y * mesh_generation.thread_random_range(gen, 0.5f, 3f));
        z += (y * mesh_generation.thread_random_range(gen, 0.5f, 3f));
        y = 0f;

        for (int layer_index = 0; layer_index < decide_biome_perlin_values.Length; layer_index++)
        {
            y = y + Mathf.PerlinNoise(decide_biome_perlin_offset_x[layer_index] + (((float)x) * decide_biome_perlin_values[layer_index] / perlin_scale_x),
                decide_biome_perlin_offset_z[layer_index] + ((float)z) * decide_biome_perlin_values[layer_index] / perlin_scale_z) * decide_biome_perlin_weights[layer_index];
        }
        return y;
    }

    public void setup_offsets(int seed)
    {
        seed = seed * seed;
        System.Random r = new System.Random(seed);
        for (int i = 0; i < decide_biome_perlin_offset_x.Length; i++)
        {
            decide_biome_perlin_offset_x[i] = mesh_generation.thread_random_range(r, -1000f, 1000f);
            decide_biome_perlin_offset_z[i] = mesh_generation.thread_random_range(r, -1000f, 1000f);
        }
    }

    public Vector3 perlin_noise_dw(Vector3 vertex_input, float perlin_scale_x , float perlin_scale_z, float[] perlin_offset_x, float[] perlin_offset_z, int seed)
    {
        System.Random r = new System.Random(seed);
        float Xwarp = mesh_generation.thread_random_range(r, 0.5f, 3f);
        float Zwarp = mesh_generation.thread_random_range(r, 0.5f, 3f);
        Vector2 store_vect = new Vector2();
        for (int w = 0; w < warping_itterations; w++)
        {
            store_vect = new Vector2(perlin_noise_point(vertex_input.x + store_vect.x, vertex_input.z + store_vect.y, perlin_scale_x, perlin_scale_z, perlin_offset_x, perlin_offset_z), 
                perlin_noise_point(vertex_input.x + Xwarp + store_vect.x, vertex_input.z + Zwarp + store_vect.y, perlin_scale_x, perlin_scale_z, perlin_offset_x, perlin_offset_z));
            Xwarp += mesh_generation.thread_random_range(r, -1, 1);
            Zwarp += mesh_generation.thread_random_range(r, -1, 1);
        }
        vertex_input.y = perlin_noise_point(vertex_input.x + store_vect.x, vertex_input.z + store_vect.y, perlin_scale_x, perlin_scale_z, perlin_offset_x, perlin_offset_z);

        vertex_input.y += terrain_heigh_modifier;

        return vertex_input;
    }

    float perlin_noise_point(float x, float z, float perlin_scale_x, float perlin_scale_z, float[] perlin_offset_x, float[] perlin_offset_z)
    {
        float y = 0;
        for (int layer_index = 0; layer_index < terrain_perlin_values.Length; layer_index++)
        {
            float m = Mathf.Abs(Mathf.PerlinNoise(perlin_offset_x[layer_index] + ((x) * terrain_perlin_values[layer_index] / perlin_scale_x),
                perlin_offset_z[layer_index] + (z) * terrain_perlin_values[layer_index] / perlin_scale_z));
            if (m > thresh)
            {
               m += Mathf.Pow((m - thresh) * thingy, pow);
            }
            y += m * terrain_perlin_weights[layer_index] * gloabal_magnitude_modifier;
            //y = y + Mathf.Abs(Mathf.PerlinNoise(perlin_offset_x[layer_index] + ((x) * terrain_perlin_values[layer_index] / perlin_scale_x),
            //perlin_offset_z[layer_index] + (z) * terrain_perlin_values[layer_index] / perlin_scale_z)) * terrain_perlin_weights[layer_index] * gloabal_magnitude_modifier;
        }
        return y;
    }
}
// funny how a couple of lines and a helpful youtube video can radically overhault a boring terrain generation technique
