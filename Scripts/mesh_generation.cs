using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using System.Linq;
using UnityEditor;
using System.Threading;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using System.Drawing;

public class Polygon
{
    public Vector3[] vertices;
    public int gradient_round;
    public float gradient;
    public bool been_done = false;
    public Polygon[] adjacent_polygons = new Polygon[3];
    public static List<Polygon> to_return = new List<Polygon>();

    public Polygon(Vector3[] Vertices)
    {
        vertices = Vertices;
    }
    public void input_gradient(float input)
    {
        gradient = input;
        if(gradient >= 1.0f)
        {
            gradient_round = 10;
        }else if(gradient >= 0.8f)
        {
            gradient_round = 8;
        }else if(gradient >= 0.6f)
        {
            gradient_round = 6;
        }else if(gradient > 0.4f)
        {
            gradient_round = 4;
        }else if(gradient > 0.2f)
        {
            gradient_round = 2;
        }
        else
        {
            gradient_round = 0;
        }
    }


    public void fill_reccurs(int level, int max_sub_size)
    {
        to_return.Add(this);
        this.been_done = true;
        level += 1;
        
        for (int poli = 0; poli < adjacent_polygons.Length; poli++)
        {
            if (adjacent_polygons[poli] != null)
            {
                Polygon pol = adjacent_polygons[poli];

                if (pol.been_done == false &&( gradient_round == pol.gradient_round))
                {
                    if (level < max_sub_size)
                    {
                        pol.fill_reccurs(level, max_sub_size);
                    }
                    else
                    {
                        pol.been_done = true;
                        to_return.Add(pol);
                    }

                }
                else if (!pol.been_done)
                {
                    bool alone_1 = true;
                    for(int i = 0; i < 3; i++)
                    {
                        if (pol.adjacent_polygons[i]!= null)
                        {
                            if (pol.gradient_round == pol.adjacent_polygons[i].gradient_round && !pol.adjacent_polygons[i].been_done)
                            {
                                alone_1 = false;
                                break;
                            }
                        }

                    }
                    if(alone_1)
                    {
                        pol.been_done = true;
                        to_return.Add(pol);
                    }
                }
            }
        }
    }
}


public class biome_data
{
    public biome biome;
    public float weight;

    public biome_data(biome b, float w)
    {
        biome = b;
        weight = w;
    }
}

public struct generate_command
{
    public Vector2 position;
    public int xSize;
    public int zSize;
    internal generate_command(Vector2 position, int xSize, int zSize)
    {
        this.position = position;
        this.xSize = xSize;
        this.zSize = zSize;
    }
}

public struct mesh_data
{
    public Vector3[] vertices;
    public int[] triangles;
    public int gradient_round;
    public UnityEngine.Color[] colours;
    public Vector2[] uvs;
    internal mesh_data(Vector3[] vertices, int[] triangles, int gradient, UnityEngine.Color[] colours, Vector2[] uvs)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.gradient_round = gradient;
        this.colours = colours;
        this.uvs = uvs;
    }
}


public class mesh_generation : MonoBehaviour
{
    public int xSize;
    public int zSize;
    public int perlin_scale_x;
    public int perlin_scale_z;
    public int texture_scaling;

    public Material material;
    
    private NavMeshSurface nav_surface;
    private NavMeshData data;

    public int max_sub_size;

    public float[] perlin_offset_x;
    public float[] perlin_offset_z;
    public biome[] boimes;

    private Queue<Tuple<List<mesh_data>,Vector3[], Dictionary<Vector2, List<biome_data>>, generate_command>> place_queue = 
        new Queue<Tuple<List<mesh_data>, Vector3[], Dictionary<Vector2, List<biome_data>>, generate_command>>();
    Queue<generate_command> commands = new Queue<generate_command>();

    int seed;

    public int biome_blend_radius;
    public int blend_squash;

    bool place_in_progress = false;

    Dictionary<Vector2 ,GameObject[]> chunks = new Dictionary<Vector2, GameObject[]>();

    GameObject camera;
    Vector3 camera_position;
    public float load_distance;
    public float unload_distance;
    Thread generate_thread;

    Tuple<List<mesh_data> ,Vector3[], Dictionary<Vector2, List<biome_data>>, generate_command> store_processing;

    generate_command store_commands;

    public static float thread_random_range(System.Random r, float min, float max)
    {
        return (((float)r.NextDouble() * (max - min)) + min);
    }

    private bool shares_vertices_check(Vector3[] one, Vector3[] two)
    {
        for (int i = 0; i < one.Length; i++)
        {
            for (int j = 0; j < two.Length; j++)
            {
                if (one[i] == two[j])
                {
                    return true;
                }
            }
        }
        return false;
    }

    void Start()
    {
        seed = start_statics.seed;
        if(start_statics.renderDistance > 0)
        {
            load_distance = start_statics.renderDistance;
        }
        else
        {

        }
        if (start_statics.chunk_size > biome_blend_radius)
        {
            xSize = start_statics.chunk_size;
            zSize = start_statics.chunk_size;
        }
        unload_distance = load_distance + (xSize / 2);
        StartCoroutine(chunk_management());
        nav_surface = GetComponent<NavMeshSurface>();
        UnityEngine.Random.seed = seed;
        for(int i = 0; i < boimes.Length; i++)
        {
            boimes[i].setup_offsets(seed + i);
        }
        UnityEngine.Random.seed = seed;
        for (int i = 0; i < perlin_offset_x.Length; i++)
        {
            UnityEngine.Random.seed = seed + i;
            perlin_offset_x[i] = UnityEngine.Random.Range(-1000f, 1000f);
            perlin_offset_z[i] = UnityEngine.Random.Range(-1000f, 1000f);
        }
        for (int x = 0; x < boimes.Length; x++)
        {
            for (int y = 0; y < boimes[x].placeable_objects.Length; y++)
            {
                boimes[x].placeable_objects[y].GetComponent<object_script>().setup_offsets(seed);
            }
        }
        data = new NavMeshData();
        generate_thread = new Thread(()
            =>
        {
            while (true)
            {
                if (commands.Count > 0)
                {
                    store_commands = commands.Dequeue();
                    if ((Mathf.Abs(store_commands.position.x + (xSize / 2) - camera_position.x) < unload_distance && 
                         Mathf.Abs(store_commands.position.y + (zSize / 2) - camera_position.z) < unload_distance))
                    {
                        Tuple<Vector3[], int[], Dictionary<Vector2, List<biome_data>>> v = 
                            create_mesh(new Vector3(store_commands.position.x, 0f, store_commands.position.y), store_commands.xSize, store_commands.zSize);
                        gradient_separation(v.Item1, v.Item2, v.Item3, store_commands);
                    }
                    else
                    {
                        chunks.Remove(store_commands.position);
                    }

                }
                else
                {
                    Thread.Sleep(50);
                }
            }

        });
        generate_thread.Start();
    }

    private void OnDestroy()
    {
        generate_thread.Abort();
    }

    IEnumerator chunk_management()
    {
        while(true)
        {
            if (camera != null)
            {
                camera_position = camera.transform.position;
            }
            else
            {
                foreach (GameObject g in GameObject.FindGameObjectsWithTag("MainCamera"))
                {
                    if (g.GetComponentInParent<Movement>() != null)
                    {
                        camera = g;
                    }
                }
                camera_position = Vector3.zero;
            }
            List<Vector2> remove_keys = new List<Vector2>();
            Vector2 camera_position2 = new Vector2(camera_position.x, camera_position.z);
            KeyValuePair<Vector2, GameObject[]>[] chunk_arr = chunks.ToArray(); 
            foreach (KeyValuePair<Vector2, GameObject[]> chunk in chunk_arr)
            {
                if ((Mathf.Abs(chunk.Key.x + (xSize / 2) - camera_position2.x) > unload_distance || 
                    Mathf.Abs(chunk.Key.y + (zSize / 2) - camera_position2.y) > unload_distance) && chunk.Value.Length > 0)
                {
                    for (int i = 0; i < chunk.Value.Length; i++)
                    {
                        if (i % 800 == 0)
                        {
                            yield return null;
                        }
                        Destroy(chunks[chunk.Key][i].GetComponent<MeshFilter>().mesh);
                        UnityEngine.Object.Destroy(chunks[chunk.Key][i]);
                    }
                    remove_keys.Add(chunk.Key);
                }
            }
            foreach(Vector2 key in remove_keys)
            {
                chunks.Remove(key);
            }

            Vector3 camera_chunk_position = new Vector3(camera_position.x - (camera_position.x % xSize), 0, camera_position.z - (camera_position.z % zSize));

            for (int x = 0; x < 2 * ((int)(load_distance)); x += xSize)
            {
                for (int z = 0; z < 2 * ((int)(load_distance)); z += zSize)
                {
                    if (!chunks.ContainsKey(new Vector2(camera_chunk_position.x + (x - (int)(load_distance)), camera_chunk_position.z + (z - (int)(load_distance)))))
                    {
                        commands.Enqueue(new generate_command(new Vector2(camera_chunk_position.x + (x - (int)(load_distance)), camera_chunk_position.z + (z - (int)(load_distance))), xSize, zSize));
                        chunks.Add(new Vector2(camera_chunk_position.x + (x - (int)(load_distance)), camera_chunk_position.z + (z - (int)(load_distance))), new GameObject[0]);
                    }
                }
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    private void Update()
    {
        if (place_queue.Count > 0 && !place_in_progress)
        {
            UnityEngine.Debug.Log(place_queue.Count);
            store_processing = place_queue.Dequeue();
            if((Mathf.Abs(store_processing.Item4.position.x + (xSize / 2) - camera_position.x) < unload_distance && 
                Mathf.Abs(store_processing.Item4.position.y + (zSize / 2) - camera_position.z) < unload_distance))
            {
                StartCoroutine(place_results(store_processing.Item1, store_processing.Item2, store_processing.Item3, store_processing.Item4));
                
            }
            else
            {
                chunks.Remove(store_processing.Item4.position);
            }
        }

    }



    Vector2[] find_in_circle(int radius, Vector2 center_position)
    {
        List<Vector2> places = new List<Vector2>();
        int y_range;
        for(int x = -radius; x < radius + 1; x++)
        {
            y_range = (int)(Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(x, 2)));
            for (int y = -y_range; y < y_range + 1; y++)
            {
                places.Add(new Vector2(x,y) + center_position);
            }
        }
        
        return places.ToArray();
    }

    Vector2[] find_as_circumfirence(int radius, Vector2 center_position)
    {
        List<Vector2> places = new List<Vector2>(radius * 4);
        for (int x = -radius; x < radius + 1; x++)
        {
            int y = -(int)(Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(x, 2)));
            places.Add(new Vector2(x, y) + center_position);
            if(y != 0)
            {
                places.Add(new Vector2(x, -y) + center_position);
            }
            
        }
        return places.ToArray();
    }

    float squash(float x, int rec)
    {
        rec--;
        float y = 3 * Mathf.Pow(x, 2) - 2 * Mathf.Pow(x, 3);
        if (rec <= 0)
        {
            return y;
        }
        else
        {
            return (squash(y, rec));
        }
    }


    Tuple<Vector3[], int[], Dictionary<Vector2, List<biome_data>>> create_mesh(Vector3 position, int xSize, int zSize)
    {
        Dictionary<Vector2, List<biome_data>> biome_per_vertex = new Dictionary<Vector2, List<biome_data>>();
        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                vertices[i] = position + new Vector3(x, 0, z);
                i++;
            }
        }

        for (int z = -(biome_blend_radius); z <= zSize + biome_blend_radius; z++)
        {
            for (int x = -(biome_blend_radius); x <= xSize + biome_blend_radius; x++)
            {
                if (!biome_per_vertex.ContainsKey(new Vector2(x + position.x, z + position.z)))
                {
                    biome_per_vertex.Add(new Vector2(x + position.x, z + position.z), new List<biome_data>());
                    biome_per_vertex[new Vector2(x + position.x, z + position.z)].Add(new biome_data(GetBiome(x + position.x, z + position.z), 1));
                }
            }
        }
        Vector2[] circle_map = find_in_circle(biome_blend_radius, Vector2.zero);
        Vector2[] circumfirence_map = find_as_circumfirence(biome_blend_radius, Vector2.zero);
        
        Parallel.For(0, vertices.Length, i =>
        {
            bool other_biome_present = false;
            List<biome_data> out_store_biomes = new List<biome_data>();
            other_biome_present = false;
            Vector2 key = new Vector2(vertices[i].x, vertices[i].z);
            biome compare_biome = biome_per_vertex[key][0].biome;


            foreach (Vector2 v in circumfirence_map)
            {
                out_store_biomes = biome_per_vertex[v + key];

                if (out_store_biomes[0].biome != compare_biome)
                {
                    other_biome_present = true;
                    break;
                }
            }
            bool biome_already_present = false;
            if (other_biome_present)
            {
                foreach (Vector2 V in circle_map)
                {
                    Vector2 v = V + key;
                    if (v != key)
                    {
                        out_store_biomes = biome_per_vertex[v];
                        if (out_store_biomes[0].biome != compare_biome)
                        {
                            biome_already_present = false;
                            float value = Mathf.Clamp01(Mathf.Pow(1 - ((v - key).magnitude - 0.5f) / (biome_blend_radius), 2));
                            for (int b = 1; b < biome_per_vertex[key].Count; b++)
                            {
                                if (out_store_biomes[0].biome == biome_per_vertex[key][b].biome)
                                {
                                    biome_already_present = true;
                                    if (value > biome_per_vertex[key][b].weight)
                                    {
                                        biome_per_vertex[key][b].weight = value;
                                    }
                                    break;
                                }
                            }
                            if (!biome_already_present)
                            {
                                biome_per_vertex[key].Add(new biome_data(out_store_biomes[0].biome, value));
                            }
                        }
                    }
                }
            }

            List<float> float_list = new List<float>();
            for (int w = 0; w < biome_per_vertex[key].Count; w++)
            {
                float_list.Add(biome_per_vertex[key][w].weight);
            }
            float_list = list_sum_one(float_list);
            for (int w = 0; w < biome_per_vertex[key].Count; w++)
            {
                biome_per_vertex[key][w].weight = float_list[w];
            }

            for (int b = 0; b < biome_per_vertex[key].Count; b++)
            {
                vertices[i].y += (biome_per_vertex[key][b].biome.perlin_noise_dw(vertices[i], perlin_scale_x, perlin_scale_z, perlin_offset_x, perlin_offset_z, seed).y * 
                                    biome_per_vertex[key][b].weight);
            }
        });

        int[] triangles = new int[6 * xSize * zSize];

        int vert = 0;
        int tris = 0;

        for(int z = 0; z < zSize; z++)
        {
            for(int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        return new Tuple<Vector3[], int[], Dictionary<Vector2, List<biome_data>>>(vertices, triangles,biome_per_vertex);
        
    }

    public static List<float> list_sum_one(List<float> input_list)
    {
        float magnitude = 0;
        for(int i = 0; i < input_list.Count; i++)
        {
            magnitude += input_list[i];
        }

        for(int i = 0; i < input_list.Count; i++)
        {
            input_list[i] = input_list[i] / magnitude;
        }

        return input_list;
    }



    biome GetBiome(float x, float y)
    {
        int highest = 0;
        float highest_value = 0;
        for(int i =0; i < boimes.Length; i++)
        {
            float value_store = boimes[i].poll_biome_weight(x, y, perlin_scale_x, perlin_scale_z, seed);
            if (value_store > highest_value)
            {
                highest = i;
                highest_value = value_store;
            }
        }
        return boimes[highest];
    }

    void gradient_separation(Vector3[] vertices, int[] triangles, Dictionary<Vector2, List<biome_data>> biome_per_vertex, generate_command command)
    {
        Polygon[] polys = new Polygon[xSize * zSize * 2];
        float this_gradient;
        int tri_index = 0;
        Vector3 lowest_vert;
        Vector3 highest_vert;
        for (int i = 0; i < xSize * zSize * 2; i++)
        {
            highest_vert = vertices[
                triangles[tri_index]];

            lowest_vert = vertices[triangles[tri_index]];
            for (int t = 0; t < 3; t++)
            {
                if (vertices[triangles[tri_index + t]].y > highest_vert.y)
                {
                    highest_vert = vertices[triangles[tri_index + t]];
                }
                if (vertices[triangles[tri_index + t]].y < lowest_vert.y)
                {
                    lowest_vert = vertices[triangles[tri_index + t]];
                }
            }
            this_gradient = (highest_vert.y - lowest_vert.y) / 
                MathF.Sqrt(((highest_vert.x - lowest_vert.x) * (highest_vert.x - lowest_vert.x)) + ((highest_vert.z - lowest_vert.z) * (highest_vert.z - lowest_vert.z)));

            polys[i] = (new Polygon(new Vector3[] { vertices[triangles[tri_index]], vertices[triangles[tri_index + 1]], vertices[triangles[tri_index + 2]] }));
            polys[i].input_gradient(this_gradient);

            tri_index += 3;
        }

        for (int i = 0; i < polys.Length; i++)
        {
            if (i % 2 == 0)
            {
                polys[i].adjacent_polygons[0] = polys[i + 1];
                if ((i - (2 * xSize) - 1) >= 0 && (i - (2 * xSize) - 1) < polys.Length && shares_vertices_check(polys[i - (xSize * 2) - 1].vertices, polys[i].vertices))
                {
                    polys[i].adjacent_polygons[1] = polys[i - (2 * xSize) + 1];
                }
                else
                {
                    polys[i].adjacent_polygons[1] = null;
                }
                if ((i - 1) >= 0 && shares_vertices_check(polys[i - 1].vertices, polys[i].vertices))
                {
                    polys[i].adjacent_polygons[2] = polys[i - 1];
                }
                else
                {
                    polys[i].adjacent_polygons[2] = null;
                }
            }
            else
            {
                polys[i].adjacent_polygons[0] = polys[i - 1];
                if ((i + (xSize * 2) + 1) >= 0 && (i + (xSize * 2) + 1) < polys.Length && shares_vertices_check(polys[i + (xSize * 2) + 1].vertices, polys[i].vertices))
                {
                    polys[i].adjacent_polygons[1] = polys[i + (xSize * 2) - 1];
                }
                else
                {
                    polys[i].adjacent_polygons[1] = null;
                }
                if ((i + 1) < polys.Length && shares_vertices_check(polys[i + 1].vertices, polys[i].vertices))
                {
                    polys[i].adjacent_polygons[2] = polys[i + 1];
                }
                else
                {
                    polys[i].adjacent_polygons[2] = null;
                }

            }

        }
        List<List<Polygon>> polygon_frames = new List<List<Polygon>>();
        List<mesh_data> mesh_frames = new List<mesh_data>();

        bool valid_poly_done = false;
        for (int i = 0; i < polys.Length; i++)
        {
            valid_poly_done = false;
            if (!polys[i].been_done)
            {
                polys[i].fill_reccurs(0, max_sub_size);
                foreach (Polygon n in Polygon.to_return)
                {
                    if (valid_poly_done == false)
                    {
                        polygon_frames.Add(new List<Polygon>());
                        valid_poly_done = true;
                    }
                    polygon_frames[polygon_frames.Count - 1].Add(n);
                }
            }
            Polygon.to_return.Clear();
        }
          

        Dictionary<Vector3, int> vertices_for_duplicates = new Dictionary<Vector3, int>();
        int store_try_get;

        for (int i = 0; i < polygon_frames.Count; i++)
        {
            List<Vector3> verts = new List<Vector3>();
            vertices_for_duplicates.Clear();
            verts.Capacity = polygon_frames[i].Count * 3;
            int[] new_tris = new int[polygon_frames[i].Count * 3];
            int index = 0;
            List<Vector2> uvs1 = new List<Vector2>();

            for (int v = 0; v < polygon_frames[i].Count; v++)
            {
                for (int three = 0; three < 3; three++)
                {
                    if (vertices_for_duplicates.TryGetValue(polygon_frames[i][v].vertices[three], out store_try_get))
                    {
                        new_tris[index] = store_try_get;
                    }
                    else
                    {
                        verts.Add(polygon_frames[i][v].vertices[three]);
                        uvs1.Add(new Vector2(polygon_frames[i][v].vertices[three].x / texture_scaling, polygon_frames[i][v].vertices[three].z / texture_scaling));
                        vertices_for_duplicates.Add(polygon_frames[i][v].vertices[three], verts.Count - 1);
                        new_tris[index] = verts.Count - 1;
                    }
                    index += 1;
                }
            }

            UnityEngine.Color[] color_arr = new UnityEngine.Color[verts.Count];
            Vector3 color_average = Vector3.zero;
            for (int v = 0; v < verts.Count; v++)
            {
                color_average = Vector3.zero;
                for (int b = 0; b < biome_per_vertex[new Vector2(verts[v].x, verts[v].z)].Count; b++)
                {
                    color_average += new Vector3(biome_per_vertex[new Vector2(verts[v].x, verts[v].z)][b].biome.color.r * 
                        biome_per_vertex[new Vector2(verts[v].x, verts[v].z)][b].weight, biome_per_vertex[new Vector2(verts[v].x, verts[v].z)][b].biome.color.g * 
                        biome_per_vertex[new Vector2(verts[v].x, verts[v].z)][b].weight, biome_per_vertex[new Vector2(verts[v].x, verts[v].z)][b].biome.color.b * 
                        biome_per_vertex[new Vector2(verts[v].x, verts[v].z)][b].weight);
                }
                color_arr[v] = new UnityEngine.Color(color_average.x, color_average.y, color_average.z);
            }

            mesh_frames.Add(new mesh_data(verts.ToArray(), new_tris, polygon_frames[i][0].gradient_round, color_arr, uvs1.ToArray()));

        }


        UnityEngine.Debug.Log("queued");
        place_queue.Enqueue(new Tuple<List<mesh_data>, Vector3[], Dictionary<Vector2, List<biome_data>>, generate_command>(mesh_frames, vertices ,biome_per_vertex, command));
    }

    private IEnumerator place_results(List<mesh_data> mesh_frames,Vector3[] vertices, Dictionary<Vector2, List<biome_data>> biome_per_vertex, generate_command command)
    {
        place_in_progress = true;
        List<GameObject> chunk = new List<GameObject> (mesh_frames.Count);
        Mesh[] meshes = new Mesh[mesh_frames.Count];
        GameObject[] submeshes = new GameObject[mesh_frames.Count];
        Mesh mesh = new Mesh();

        for(int i =0; i < mesh_frames.Count; i++)
        {
            mesh = new Mesh();
            mesh.vertices = mesh_frames[i].vertices;
            mesh.triangles = mesh_frames[i].triangles;
            mesh.colors = mesh_frames[i].colours;
            mesh.uv = mesh_frames[i].uvs;
            mesh.name = "submesh" + i.ToSafeString();
            mesh.RecalculateNormals();
            submeshes[i] = (new GameObject("Submesh", new Type[] { typeof(MeshFilter), typeof(MeshCollider), typeof(NavMeshModifier), typeof(MeshRenderer) }));
            submeshes[i].transform.parent = this.gameObject.transform;
            meshes[i] = mesh;
        }

        for (int i = 0; i < mesh_frames.Count; i++)
        {
            if (i % 20 == 0)
            {
                yield return null;
            }
            submeshes[i].GetComponent<MeshFilter>().mesh = meshes[i];
            submeshes[i].GetComponent<MeshRenderer>().materials = new Material[1] { material };
            submeshes[i].GetComponent<MeshCollider>().sharedMesh = meshes[i];
            submeshes[i].GetComponent<NavMeshModifier>().overrideArea = true;
            submeshes[i].GetComponent<NavMeshModifier>().area = NavMesh.GetAreaFromName(mesh_frames[i].gradient_round.ToString());
            chunk.Add(submeshes[i]);
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            if(i % 600 == 0)
            {
                yield return null;
            }
            foreach (GameObject g in biome_per_vertex[new Vector2(vertices[i].x, vertices[i].z)][0].biome.placeable_objects)
            {
                object_script placing = g.GetComponent<object_script>();
                if (placing.weight_position(seed, vertices[i]))
                {
                    GameObject place = Instantiate(g, placing.decide_position(vertices[i], seed), Quaternion.identity);
                    place.transform.SetParent(transform, true);
                    place.GetComponent<object_script>().sort_nav_and_rotate();
                    chunk.Add(place);
                }
            }
        }

        chunks[command.position] = chunk.ToArray();
        yield return null;
        nav_surface.UpdateNavMesh(data);

        nav_surface.navMeshData = data;
        nav_surface.AddData();
        place_in_progress = false;
    }
}
