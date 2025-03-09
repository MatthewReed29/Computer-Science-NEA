using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class decisions : MonoBehaviour
{
    public float time_delta;
    public float safe_distance;
    public float mid_range;
    public float outnumbered_weighting;
    public float idle_range;
    public float team_stay_range_max;
    public float team_stay_range_natural;
    public int look_for_cover_range;
    public float defend_point_radius;
    public float cover_scale;
    public float cover_consideration_distance;
    public float teammate_number_confidence;
    [HideInInspector]
    public Vector3 team_center = new Vector3();
    [HideInInspector]
    public Vector3 local_team_center = new Vector3();
    [HideInInspector]
    public Vector3 local_enemy_center = new Vector3();
    [SerializeField]
    behaviourTree tree;
    public string state = "none";
    string previous_state = "none";
    Coroutine active_coroutine;
    relevant_information rel;
    awareness known;
    decisions self;
    NavMeshAgent self_agent;
    attacks attacks;
    public List<Tuple<Vector3, float, int>> cover_spots = new List<Tuple<Vector3, float, int>>();
    public Dictionary<string, float> impulses = new Dictionary<string, float>();
    public Dictionary<string, float> impulse_weights = new Dictionary<string, float>();
    public string[] impulse_names;
    public float[] impulse_weight_inputs;
    public float[] impulse_weight_outputs;
    public List<string> tree_path = new List<string>();
    public List<string> previous_tree_path = new List<string>();

    private void Start()
    {
        attacks = GetComponent<attacks>();
        known = GetComponent<awareness>();
        rel = GetComponent <relevant_information>();
        self = GetComponent<decisions>();
        self_agent = GetComponent<NavMeshAgent>();
        active_coroutine = null;
        StartCoroutine(find_cover());
        for (int i = 0; i < impulse_names.Length; i++)
        {
            impulses.Add(impulse_names[i], 0f);
            impulse_weights.Add(impulse_names[i], impulse_weight_inputs[i]);
        }
        AI_Store.add_target(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        impulse_weight_outputs = impulses.Values.ToArray();
        if (state != previous_state && state != "dead")
        {
            if(active_coroutine != null)
            {
                StopAllCoroutines();
                impulses[previous_state] = 0f;
                StartCoroutine(find_cover());
                
            }
            active_coroutine = StartCoroutine(state);
            previous_state = state;
        }else if(state == "dead")
        {
            StopAllCoroutines();
        }
            
    }

    private void get_values()
    {
        //______________________________________________________
        transformData target = known.target;
        Vector3 temp_team_center = rel.central_position;
        for (int i = 0; i < rel.allies.Count; i++)
        {
            temp_team_center += rel.allies[i].central_position;
        }

        team_center = new Vector3(temp_team_center.x / (rel.allies.Count + 1), temp_team_center.y / (rel.allies.Count + 1), temp_team_center.z / (rel.allies.Count + 1));

        //______________________________________________________________________
        Vector3 temp_local_team_center = 2 * rel.central_position;
        int average_counter = 2;
        for (int i = 0; i < rel.allies.Count; i++)
        {
            if ((rel.allies[i].central_position - rel.central_position).magnitude < ( (2f/3f) * known.seeing_range))
            {
                temp_local_team_center += rel.allies[i].central_position;
                average_counter++;
            }

        }
        if(average_counter > 0)
        {
            local_team_center = temp_local_team_center / average_counter;
        }
        //______________________________________________________________________
        average_counter = 1;
        
        if(target != null)
        {
            Vector3 temp_local_enemy_center = target.position * 1;
            for (int i = 0; i < known.ghost_targets.Count; i++)
            {
                if ((target.position - known.ghost_targets[i].position).magnitude < (known.seeing_range * (2f / 3f)))
                {
                    temp_local_enemy_center += known.ghost_targets[i].position;
                    average_counter++;
                }
            }
            local_enemy_center = temp_local_enemy_center / average_counter;
        }
        else
        {
            if(known.ghost_targets.Count > 0)
            {
                local_enemy_center = known.ghost_targets[0].position;
            }
            else
            {
                local_enemy_center = Vector3.zero;
            }
        }
    }

    public void get_decision()
    {
        get_values();
        tree.decision_calc(known, rel, ref self);
    }


    public void dead()
    {
        state = "dead";

    }

    public void simplePath(Vector3 position, float sampleRange)
    {
        NavMeshPath path = new NavMeshPath();
        if (self_agent.CalculatePath(position, path))
        {
            self_agent.SetPath(path);
        }
        else
        {
            NavMeshHit hit;
            NavMesh.SamplePosition(position, out hit, sampleRange, NavMesh.AllAreas);
            if (hit.hit)
            {
                self_agent.CalculatePath(hit.position, path);
                self_agent.SetPath(path);
            }
            else
            {
                self_agent.SetDestination(position);
            }
        }
    }

    private IEnumerator rushDown()
    {
        self_agent.updateRotation = false;
        float next_time_path = 0f;
        StartCoroutine(shoot_on_see());
        StartCoroutine(aim_gun());
        
        while (true)
        {
            if (known.target != null)
            {
                if (known.target.objects_seen)
                {
                    self_agent.speed = 5;
                }
                else
                {
                    self_agent.speed = 7;
                }
                if (next_time_path < Time.time && rel.alive)
                {
                    simplePath(known.target.position, known.target.info.height * 5);
                    next_time_path = Time.time +  0.5f;
                }
            }
            yield return null;
        }
    }

    private IEnumerator fleeDirect() 
    {
        self_agent.speed = 9;
        self_agent.updateRotation = true;
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            Vector3 away_from = known.target.position - transform.position;
            Vector3 away_to = transform.position - (away_from.normalized * 20);

            simplePath(away_to, away_from.magnitude * 5);

        }
    }

    private IEnumerator retreatDirect()
    {
        self_agent.speed = 5;
        self_agent.updateRotation = false;
        StartCoroutine(aim_gun());
        StartCoroutine(shoot_on_see());
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            Vector3 away_from = known.target.position - transform.position;
            Vector3 away_to = transform.position - (away_from.normalized * 20);

            simplePath(away_to, away_from.magnitude * 5);
        }
    }

    private IEnumerator retreatAllies()
    {
        float update_time = Time.time;
        self_agent.updateRotation = false;
        self_agent.speed = 4.5f;
        StartCoroutine(aim_gun());
        StartCoroutine(shoot_on_see());
        while (true)
        {
            if (update_time <= Time.time)
            {
                simplePath(local_team_center, (local_team_center - rel.central_position).magnitude);
                update_time = Time.time + 0.5f;
            }
            yield return null;
        }
    }

    private IEnumerator idle()
    {
        self_agent.speed = 3;
        float randomx;
        float randomz;
        float next_time_idle = 0;
        self_agent.updateRotation = true;
        while (true)
        {
            if(next_time_idle < Time.time)
            {
                UnityEngine.Random.seed = (int)(Time.timeAsDouble * transform.position.magnitude);
                randomx = UnityEngine.Random.Range(-idle_range , idle_range);
                UnityEngine.Random.seed += 1;
                randomz = UnityEngine.Random.Range(-idle_range , idle_range);
                if(((new Vector3(transform.position.x + randomx, transform.position.y, transform.position.z + randomz)) - transform.position).magnitude > rel.height)
                {
                    simplePath(new Vector3(transform.position.x + randomx, transform.position.y, transform.position.z + randomz), idle_range);
                    UnityEngine.Random.seed += 2;
                    next_time_idle = Time.time + 3f;
                }

            }
            yield return null;
        }
    }

    private IEnumerator idleTeam()
    {
        self_agent.speed = 3;
        float randomx;
        float randomz;
        float next_time_idle = 0;
        
        self_agent.updateRotation = true;
        while (true)
        {
            if (next_time_idle < Time.time)
            {
                if((team_center - rel.central_position).magnitude > team_stay_range_max)
                {
                    simplePath(team_center, (team_center - rel.central_position).magnitude);
                    next_time_idle = Time.time + 5f;
                }
                else if((team_center - rel.central_position).magnitude > team_stay_range_natural)
                {
                    float randomx2;
                    float randomz2;
                    UnityEngine.Random.seed = (int)(Time.timeAsDouble * transform.position.magnitude);
                    randomx = UnityEngine.Random.Range(-idle_range, idle_range);
                    UnityEngine.Random.seed += 1;
                    randomz = UnityEngine.Random.Range(-idle_range, idle_range);
                    UnityEngine.Random.seed += 1;
                    randomx2 = UnityEngine.Random.Range(-idle_range, idle_range);
                    UnityEngine.Random.seed += 1;
                    randomz2 = UnityEngine.Random.Range(-idle_range, idle_range);

                    Vector3 dest1 = new Vector3(transform.position.x + randomx, team_center.y, transform.position.z + randomz);
                    Vector3 dest2 = new Vector3(transform.position.x + randomx2, team_center.y, transform.position.z + randomz2);

                    if((dest1 - team_center).magnitude < (dest2 - team_center).magnitude)
                    {
                        simplePath(dest1, 5f);
                    }
                    else
                    {
                        simplePath(dest2, 5f);
                    }
                    next_time_idle = Time.time + 5f;
                }
                else
                {
                    UnityEngine.Random.seed = (int)(Time.timeAsDouble * transform.position.magnitude);
                    randomx = UnityEngine.Random.Range(-idle_range, idle_range);
                    UnityEngine.Random.seed += 1;
                    randomz = UnityEngine.Random.Range(-idle_range, idle_range);
                    if (((new Vector3(transform.position.x + randomx, transform.position.y, transform.position.z + randomz)) - transform.position).magnitude > rel.height)
                    {
                        simplePath(new Vector3(transform.position.x + randomx, transform.position.y, transform.position.z + randomz), rel.height * 4);
                        next_time_idle = Time.time + 5f;
                    }
                }
            }
            yield return null;
        }
    }

    private IEnumerator fleeAllies()
    {
        float update_time = Time.time;
        self_agent.updateRotation = true;
        self_agent.speed = 8.2f;
        while(true)
        {
            if(update_time <= Time.time)
            {
                simplePath(local_team_center, (local_team_center - rel.central_position).magnitude);
                update_time = Time.time + 0.5f;
            }
            yield return null;
        }
    }

    private IEnumerator aim_gun()
    {
        while(true)
        {
            if(known.target != null)
            {
                transform.Rotate((Quaternion.LookRotation(new Vector3(known.target.position.x - rel.eye_level.position.x, 0, known.target.position.z - rel.eye_level.position.z)).eulerAngles
                    - transform.rotation.eulerAngles) * Mathf.Clamp(60f * 0.5f * Time.deltaTime , 0f, 1f), Space.World);
                attacks.gun.transform.rotation = Quaternion.LookRotation(new 
                    Vector3(known.target.position.x - attacks.gun.transform.position.x, known.target.position.y - attacks.gun.transform.position.y, known.target.position.z - attacks.gun.transform.position.z));
            }
            yield return null;
        }
    }

    private IEnumerator find_cover()
    {
        float enter_percentage = 0f;
        NavMeshHit mesh_hit;
        RaycastHit[] hits;
        while (true)
        {
            Tuple<Vector3, float>[,] cover_points = new Tuple<Vector3, float>[((int)look_for_cover_range * 2) + 1, ((int)look_for_cover_range * 2) + 1];
            int[,] cover_arr = new int[((int)look_for_cover_range * 2) + 1, ((int)look_for_cover_range * 2) + 1];
            for(int x = -(int)look_for_cover_range; x < look_for_cover_range + 1; x++)
            {
                for(int y = -(int)look_for_cover_range; y < look_for_cover_range + 1; y++)
                {
                    hits = Physics.RaycastAll(transform.position + new Vector3(x * cover_scale, 0, y * cover_scale), -Vector3.up);
                    if (hits.Length != 0 && known.ghost_targets.Count > 0)
                    {
                        if (NavMesh.SamplePosition(hits[0].point, out mesh_hit, 0.5f, NavMesh.AllAreas))
                        {
                            if (test(hits[0].point + new Vector3(0, (self_agent.height * 3f / 4f), 0), out enter_percentage))
                            {
                                cover_points[x + look_for_cover_range, y + look_for_cover_range] = 
                                    new Tuple<Vector3, float>(hits[0].point + new Vector3(0, (self_agent.height * 3 / 4), 0), enter_percentage);
                                cover_arr[x + look_for_cover_range, y + look_for_cover_range] = 1;
                            }
                        }
                        else
                        {
                            if(test(hits[0].point + new Vector3(0, (self_agent.height * 3f / 4f), 0), out enter_percentage))
                            {
                                cover_arr[x + look_for_cover_range, y + look_for_cover_range] = 1;
                            }
                        }
                    }
                }
            }
            yield return null;
            bool something_done = true;
            int threshold = 0;
            bool is_surrounded = true;
            while(something_done)
            {
                something_done = false;

                for(int x = 1; x < (int)Mathf.Sqrt(cover_arr.Length) - 1; x++)
                {
                    for (int y = 1; y < (int)Mathf.Sqrt(cover_arr.Length) - 1; y++)
                    {
                        if (cover_arr[x, y] > threshold)
                        {
                            is_surrounded = true;
                            if (cover_arr[x - 1, y] > threshold)
                            {

                            }
                            else
                            {
                                is_surrounded = false;
                            }

                            if (cover_arr[x, y - 1] > threshold)
                            {

                            }
                            else
                            {
                                is_surrounded = false;
                            }

                            if (cover_arr[x + 1, y] > threshold)
                            {

                            }
                            else
                            {
                                is_surrounded = false;
                            }

                            if (cover_arr[x, y + 1] > threshold)
                            {

                            }
                            else
                            {
                                is_surrounded = false;
                            }


                            if (is_surrounded)
                            {
                                cover_arr[x, y]++;
                                something_done = true;
                            }

                        }
                    }
                    
                }
                threshold++;
            }

            List<Tuple<Vector3, float, int>> new_covers = new List<Tuple<Vector3, float, int>>();

            for(int x = 0; x < (((int)look_for_cover_range * 2) + 1); x++)
            {
                for (int y = 0; y < ((int)look_for_cover_range * 2) + 1; y++ )
                {
                    if (cover_points[x,y] != null)
                    {
                        new_covers.Add(new Tuple<Vector3, float, int>(cover_points[x,y].Item1, cover_points[x,y].Item2, cover_arr[x,y]));
                    }
                }
            }
            cover_spots = new_covers;
            yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 1.5f));
        }
    }

    bool test(Vector3 point, out float percentage_cover)
    {
        int count = 0;
        RaycastHit[] hits;
        bool blocked_by_character = true;
        int known_ghosts = 0;
        for (int i = 0; i < known.ghost_targets.Count; i++)
        {
            if (known.ghost_targets[i].objects_seen)
            {
                if (known.ghost_targets[i].last_updated - Time.time < 5f)
                {
                    known_ghosts++;
                    hits = Physics.RaycastAll(known.ghost_targets[i].info.eye_level.position, point - known.ghost_targets[i].info.eye_level.position,
                        (point - known.ghost_targets[i].info.eye_level.position).magnitude, -1, QueryTriggerInteraction.Ignore);
                    if (hits.Length > 0)
                    {
                        blocked_by_character = true;
                        foreach (RaycastHit ray_hit in hits)
                        {
                            if (ray_hit.collider.gameObject.GetComponent<relevant_information>() == null)
                            {
                                blocked_by_character = false;
                            }
                        }
                        if (!blocked_by_character)
                        {
                            count++;
                        }
                    }
                }
            }
        }
        percentage_cover = (float)count / (float)known_ghosts;
        if (percentage_cover > 0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    bool test(Vector3 point, Vector3 target)
    {
        RaycastHit[] hits;
        bool blocked_by_character = true;
        hits = Physics.RaycastAll(target, point - target, (point - target).magnitude, -1, QueryTriggerInteraction.Ignore);
        if (hits.Length > 0)
        {
            blocked_by_character = true;
            foreach (RaycastHit ray_hit in hits)
            {
                if (ray_hit.collider.gameObject.GetComponent<relevant_information>() == null)
                {
                    blocked_by_character = false;
                }
            }
            if (!blocked_by_character)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private IEnumerator shoot_on_see()
    {
        float next_time_shoot = Time.time;
        while(true)
        {
            if(known.target != null)
            {
                if (Time.time >= next_time_shoot && known.target.objects_seen)
                {
                    RaycastHit h = new RaycastHit();
                    
                    Physics.SphereCast(attacks.gun.transform.position, 0.5f, known.target.position - attacks.gun.transform.position, out h, known.seeing_range, -1, QueryTriggerInteraction.Ignore);
                    if (h.collider != null &&(h.collider.gameObject.GetComponent<relevant_information>() != null))
                    {
                        if (h.collider.gameObject.GetComponent<relevant_information>().allegiance != rel.allegiance)
                        {
                            attacks.shoot();
                            next_time_shoot = Time.time + attacks.gun_cooldown;
                        }
                    }
                    else
                    {
                        attacks.shoot();
                        next_time_shoot = Time.time + attacks.gun_cooldown;
                    }
                }
            }
            yield return null;
        }
    }

    IEnumerator fleeCover()
    {
        self_agent.speed = 9;
        self_agent.updateRotation = true;
        Tuple<Vector3, float, int> best_spot;
        float weight1 = 0f;
        float weight2 = 0f;
        float update_time = 0;
        float depth1 = 0;
        float depth2 = 0;
        NavMeshPath path = new NavMeshPath();
        while (true)
        {
            if (update_time <= Time.time && cover_spots.Count > 0)
            {
                best_spot = cover_spots[0];
                foreach (Tuple<Vector3, float, int> spot in cover_spots)
                {
                    if((best_spot.Item3 - 2) <= 0)
                    {
                        depth1 = 1f / (4f - best_spot.Item3);
                    }
                    else
                    {
                        depth1 = best_spot.Item3 - 2;
                    }

                    if ((spot.Item3 - 2) <= 0)
                    {
                        depth2 = 1f / (4f - spot.Item3);
                    }
                    else
                    {
                        depth2 = spot.Item3 - 2;
                    }
                    weight1 = ((Mathf.Pow(best_spot.Item2, 5)) * (Mathf.Pow(depth1, 0.5f))) / Mathf.Pow((best_spot.Item1 - rel.central_position).magnitude, 1f / 2f);
                    weight2 = ((Mathf.Pow(spot.Item2, 5)) * (Mathf.Pow(depth2, 0.5f))) / Mathf.Pow((spot.Item1 - rel.central_position).magnitude, 1f / 2f);
                    if (weight2 > weight1)
                    {
                        best_spot = spot;
                    }
                }

                self_agent.SetDestination(best_spot.Item1);
                
                update_time = Time.time + 1f;
            }
            yield return null;
        }
    }

    IEnumerator coverCombat()
    {
        self_agent.updateRotation = false;
        self_agent.speed = 4;
        StartCoroutine(shoot_on_see());
        StartCoroutine(aim_gun());
        float next_time_peek = 0;
        bool peek_in = true;
        Tuple<Vector3, float, int> best_spot;
        Vector3 previous_in_cover_position = rel.central_position;

        while (true)
        {
            if (Time.time > next_time_peek && cover_spots.Count > 0 && known.target != null)
            {
                if (peek_in)
                {
                    best_spot = cover_spots[0];
                    for (int i = 1; i < cover_spots.Count; i++)
                    {
                        if ((cover_spots[i].Item1 - rel.central_position).magnitude * (2 - cover_spots[i].Item2) * (cover_spots[i].Item1 - previous_in_cover_position).magnitude 
                            < (best_spot.Item1 - rel.central_position).magnitude * (2 - best_spot.Item2) * (best_spot.Item1 - previous_in_cover_position).magnitude  
                            && cover_spots[i].Item3 <= 2 && test(cover_spots[i].Item1, known.target.position))
                        {
                            best_spot = cover_spots[i];
                        }
                    }
                    self_agent.SetDestination(best_spot.Item1);
                    previous_in_cover_position = best_spot.Item1;
                    peek_in = false;
                }
                else
                {
                    bool spot_found = false; 
                    for(int x = -4; x <= 4; x++)
                    {
                        for(int y = -4; y <= 4; y++)
                        {
                            NavMeshHit hit;
                            if(NavMesh.SamplePosition(rel.central_position + new Vector3(x, 0, y), out hit, 1.5f, NavMesh.AllAreas))
                            {
                                if (!test(hit.position + new Vector3(0, (3f/4f) * rel.height,0 ), known.target.position) && !spot_found)
                                {
                                    self_agent.SetDestination(rel.central_position + new Vector3(x, 0, y));
                                    spot_found = true;
                                }
                            }
                        }
                    }
                    if(!spot_found)
                    {
                        self_agent.SetDestination(rel.central_position + new Vector3(3f, 0, 0));
                    }
                    peek_in = true;
                }

                next_time_peek = Time.time + 3f + UnityEngine.Random.Range(0f, 1f);
            }
            yield return null;
        }
    }


    IEnumerator retreatCover()
    {
        self_agent.speed = 5;
        self_agent.updateRotation = false;
        StartCoroutine(aim_gun());
        StartCoroutine(shoot_on_see());
        Tuple<Vector3, float, int> best_spot;
        float weight1 = 0f;
        float weight2 = 0f;
        float update_time = 0;
        float depth1 = 0;
        float depth2 = 0;
        NavMeshPath path = new NavMeshPath();
        while (true)
        {
            if (update_time <= Time.time && cover_spots.Count > 0)
            {
                best_spot = cover_spots[0];
                foreach (Tuple<Vector3, float, int> spot in cover_spots)
                {
                    if ((best_spot.Item3 - 2) <= 0)
                    {
                        depth1 = 1f / (4f - best_spot.Item3);
                    }
                    else
                    {
                        depth1 = best_spot.Item3 - 2;
                    }

                    if ((spot.Item3 - 2) <= 0)
                    {
                        depth2 = 1f / (4f - spot.Item3);
                    }
                    else
                    {
                        depth2 = spot.Item3 - 2;
                    }
                    weight1 = ((Mathf.Pow(best_spot.Item2, 5)) * (Mathf.Pow(depth1, 0.3f))) / Mathf.Pow((best_spot.Item1 - rel.central_position).magnitude, 1f / 2f);
                    weight2 = ((Mathf.Pow(spot.Item2, 5)) * (Mathf.Pow(depth2, 0.3f))) / Mathf.Pow((spot.Item1 - rel.central_position).magnitude, 1f / 2f);
                    if (weight2 > weight1)
                    {
                        best_spot = spot;
                    }
                }

                self_agent.SetDestination(best_spot.Item1);

                update_time = Time.time + 1f;
            }
            yield return null;
        }
    }


    private IEnumerator strafeShoot()
    {
        self_agent.speed = 5;
        StartCoroutine(shoot_on_see());
        StartCoroutine(aim_gun());
        self_agent.updateRotation = false;
        float next_time_update = 0;
        while (true)
        {
            if (Time.time > next_time_update && known.target != null)
            {
                Vector3 go_position = Vector3.zero;
                List<NavMeshHit> nav_hits = new List<NavMeshHit>();
                for (int t = 0; t < 15; t++)
                {
                    float x = UnityEngine.Random.Range(-6f, 6f);
                    float z = UnityEngine.Random.Range(-6f, 6f);
                    if (new Vector2(x, z).magnitude > 2.5)
                    {
                        RaycastHit[] hits = Physics.RaycastAll(rel.central_position + new Vector3(x, 100, z), Vector3.down, 200, -1);
                        
                        foreach(RaycastHit hit in hits)
                        {
                            NavMeshHit Nav_hit;
                            if (NavMesh.SamplePosition(hit.point, out Nav_hit, 0.5f, NavMesh.AllAreas))
                            {
                                nav_hits.Add(Nav_hit);
                            }
                        }
                        int closest = 0;
                        for (int i = 0; i < nav_hits.Count; i++)
                        {
                            if ((nav_hits[i].position - rel.central_position).magnitude < (nav_hits[closest].position - rel.central_position).magnitude)
                            {
                                closest = i;
                            }
                        }
                        if (nav_hits.Count > 0)
                        {
                            if (MathF.Abs(Vector3.Dot((known.target.position - rel.central_position).normalized, nav_hits[closest].position - rel.central_position)) < 2)
                            {
                                go_position = nav_hits[closest].position;
                                break;
                            }
                        }
                    }
                }
                if(go_position != Vector3.zero)
                {
                    self_agent.SetDestination(go_position);
                    next_time_update = Time.time + 1.5f + UnityEngine.Random.Range(0, 1f);
                }

            }
            yield return null;
        }
    }


    private IEnumerator defendPoint()
    {
        self_agent.speed = 4;
        self_agent.updateRotation = false;
        StartCoroutine(shoot_on_see());
        StartCoroutine(aim_gun());
        float randomx;
        float randomz;
        float next_time_path = 0;
        Vector3 defend_point = new Vector3();
        defend_point = rel.central_position * 8;
        for (int i = 0; i < rel.allies.Count; i++)
        {
            defend_point += rel.allies[i].central_position;
        }
        defend_point = defend_point / (8 + rel.allies.Count);

        while (true)
        {
            if (next_time_path < Time.time)
            {
                if ((defend_point - rel.central_position).magnitude > team_stay_range_max)
                {
                    simplePath(defend_point, (defend_point - rel.central_position).magnitude);
                    next_time_path = Time.time + 5f;
                }
                else if ((defend_point - rel.central_position).magnitude > defend_point_radius)
                {
                    float randomx2;
                    float randomz2;
                    UnityEngine.Random.seed = (int)(Time.timeAsDouble * transform.position.magnitude);
                    randomx = UnityEngine.Random.Range(-idle_range, idle_range);
                    randomz = UnityEngine.Random.Range(-idle_range, idle_range);
                    randomx2 = UnityEngine.Random.Range(-idle_range, idle_range);
                    randomz2 = UnityEngine.Random.Range(-idle_range , idle_range);

                    Vector3 dest1 = new Vector3(transform.position.x + randomx, defend_point.y, transform.position.z + randomz);
                    Vector3 dest2 = new Vector3(transform.position.x + randomx2, defend_point.y, transform.position.z + randomz2);

                    if ((dest1 - defend_point).magnitude < (dest2 - defend_point).magnitude)
                    {
                        self_agent.SetDestination(dest1);
                    }
                    else
                    {
                        self_agent.SetDestination(dest2);
                    }
                    next_time_path = Time.time + 5f;
                }
                else
                {
                    UnityEngine.Random.seed = (int)(Time.timeAsDouble * transform.position.magnitude);
                    randomx = UnityEngine.Random.Range(-idle_range, idle_range);
                    randomz = UnityEngine.Random.Range(-idle_range, idle_range);
                    if (((new Vector3(transform.position.x + randomx, transform.position.y, transform.position.z + randomz)) - transform.position).magnitude > rel.height)
                    {
                        self_agent.SetDestination(new Vector3(transform.position.x + randomx, transform.position.y, transform.position.z + randomz));
                        next_time_path = Time.time + 5f;
                    }
                }
            }
            yield return null;
        }
    }

    private IEnumerator defendAllies()
    {
        float next_time_path = 0f;

        self_agent.speed = 4;
        self_agent.updateRotation = false;


        Vector3 defend_point = rel.central_position * 2;
        int average_count = 8;
        for (int i = 0; i < rel.allies.Count; i++)
        {
            if (rel.allies[i].decision_script.state.Substring(0, Mathf.Min(rel.allies[i].decision_script.state.Length, 7)).ToLower() == "retreat")
            {
                defend_point += (rel.allies[i].central_position * 2);
                average_count += 2;
            }
            else if (rel.allies[i].decision_script.state.Substring(0, Mathf.Min(rel.allies[i].decision_script.state.Length, 4)).ToLower() == "flee")
            {
                defend_point += (rel.allies[i].central_position * 3);
                average_count += 3;
            }
            else
            {
                defend_point += rel.allies[i].central_position;
                average_count += 1;
            }
        }
        defend_point = defend_point / (average_count);

        float randomx;
        float randomz;
        while (true)
        {
            if (next_time_path < Time.time)
            {
                if ((defend_point - rel.central_position).magnitude > team_stay_range_max)
                {
                    simplePath(defend_point, (defend_point - rel.central_position).magnitude);
                    next_time_path = Time.time + 5f;
                }
                else if ((defend_point - rel.central_position).magnitude > defend_point_radius)
                {
                    float randomx2;
                    float randomz2;
                    UnityEngine.Random.seed = (int)(Time.timeAsDouble * transform.position.magnitude);
                    randomx = UnityEngine.Random.Range(-idle_range, idle_range);
                    randomz = UnityEngine.Random.Range(-idle_range, idle_range);
                    randomx2 = UnityEngine.Random.Range(-idle_range, idle_range);
                    randomz2 = UnityEngine.Random.Range(-idle_range, idle_range);

                    Vector3 dest1 = new Vector3(transform.position.x + randomx, defend_point.y, transform.position.z + randomz);
                    Vector3 dest2 = new Vector3(transform.position.x + randomx2, defend_point.y, transform.position.z + randomz2);

                    if ((dest1 - defend_point).magnitude < (dest2 - defend_point).magnitude)
                    {
                        self_agent.SetDestination(dest1);
                    }
                    else
                    {
                        self_agent.SetDestination(dest2);
                    }
                    next_time_path = Time.time + 5f;
                }
                else
                {
                    UnityEngine.Random.seed = (int)(Time.timeAsDouble * transform.position.magnitude);
                    randomx = UnityEngine.Random.Range(-idle_range, idle_range);
                    randomz = UnityEngine.Random.Range(-idle_range, idle_range);
                    if (((new Vector3(transform.position.x + randomx, transform.position.y, transform.position.z + randomz)) - transform.position).magnitude > rel.height)
                    {
                        self_agent.SetDestination(new Vector3(transform.position.x + randomx, transform.position.y, transform.position.z + randomz));
                        next_time_path = Time.time + 5f;
                    }
                }
            }
            yield return null;
        }
    }

    private IEnumerator pursueReady()
    {
        self_agent.speed = 6f;
        self_agent.updateRotation = false;
        StartCoroutine(aim_gun());
        StartCoroutine(shoot_on_see());
        float next_time_path = Time.time;
        NavMeshHit hit;
        NavMeshPath path = new NavMeshPath();
        while (true)
        {
            if (Time.time > next_time_path && known.target != null)
            {
                if (NavMesh.SamplePosition(known.target.position, out hit, known.target.info.height, NavMesh.AllAreas))
                {
                    if (self_agent.CalculatePath(hit.position, path))
                    {
                        if (path.corners.Length > 3)
                        {
                            self_agent.CalculatePath(path.corners[path.corners.Length - 2], path);
                        }
                        self_agent.SetPath(path);
                        next_time_path = Time.time + 10;
                    }
                }
            }
            if (known.target !=null)
            {
                if ((self_agent.pathEndPosition - rel.central_position).magnitude < rel.height)
                {
                    if (NavMesh.SamplePosition((((known.target.position - known.target.previous_position) / known.time_interval) * 1f) + known.target.position, out hit, known.target.info.height * 8, NavMesh.AllAreas))
                    {
                        self_agent.CalculatePath(hit.position, path);
                        self_agent.SetPath(path);
                    }
                }
            }
            yield return null;
        }
    }

    private IEnumerator pursueRush()
    {
        self_agent.speed = 8f;
        self_agent.updateRotation = true;
        float next_time_path = Time.time;
        NavMeshHit hit;
        NavMeshPath path = new NavMeshPath();
        while (true)
        {
            if (Time.time > next_time_path && known.target != null)
            {
                if (NavMesh.SamplePosition(known.target.position, out hit, known.target.info.height, NavMesh.AllAreas))
                {
                    if (self_agent.CalculatePath(hit.position, path))
                    {
                        if (path.corners.Length > 3)
                        {
                            self_agent.CalculatePath(path.corners[path.corners.Length - 2], path);
                        }
                        self_agent.SetPath(path);
                        next_time_path = Time.time + 10;
                    }
                }
            }
            if (known.target != null)
            {
                if ((self_agent.pathEndPosition - rel.central_position).magnitude < rel.height)
                {
                    if (NavMesh.SamplePosition((((known.target.position - known.target.previous_position) / known.time_interval) * 1f) + known.target.position, out hit, known.target.info.height * 8, NavMesh.AllAreas))
                    {
                        self_agent.CalculatePath(hit.position, path);
                        self_agent.SetPath(path);
                    }
                }
            }
            yield return null;
        }
    }


    private IEnumerator pursueReadyTeam()
    {
        self_agent.speed = 6f;
        self_agent.updateRotation = false;
        StartCoroutine(aim_gun());
        StartCoroutine(shoot_on_see());
        float next_time_path = Time.time;
        NavMeshHit hit;
        NavMeshPath path = new NavMeshPath();
        while (true)
        {
            if (Time.time > next_time_path && known.target != null)
            {
                if (NavMesh.SamplePosition(known.target.position + new Vector3(UnityEngine.Random.Range(-2f, 2f), 0, UnityEngine.Random.Range(-2f, 2f)), out hit, known.target.info.height, NavMesh.AllAreas))
                {
                    if (self_agent.CalculatePath(hit.position, path))
                    {
                        if (path.corners.Length > 3)
                        {
                            self_agent.CalculatePath(path.corners[path.corners.Length - 2], path);
                        }
                        self_agent.SetPath(path);

                        next_time_path = Time.time + 10;
                    }
                }
            }
            if (known.target != null)
            {
                if ((self_agent.pathEndPosition - rel.central_position).magnitude < rel.height)
                {
                    if (NavMesh.SamplePosition((((known.target.position - known.target.previous_position) / known.time_interval) * 1f) + known.target.position + 
                        new Vector3(UnityEngine.Random.Range(-2f, 2f), 0, UnityEngine.Random.Range(-2f, 2f)), out hit, known.target.info.height * 8, NavMesh.AllAreas))
                    {
                        self_agent.CalculatePath(hit.position, path);
                        self_agent.SetPath(path);
                    }
                }
            }
            yield return null;
        }
    }


    private IEnumerator pursueRushTeam()
    {
        self_agent.speed = 8f;
        self_agent.updateRotation = true;
        float next_time_path = Time.time;
        NavMeshHit hit;
        NavMeshPath path = new NavMeshPath();
        while (true)
        {
            if (Time.time > next_time_path && known.target != null)
            {
                if (NavMesh.SamplePosition(known.target.position + new Vector3(UnityEngine.Random.Range(-2f, 2f), 0, UnityEngine.Random.Range(-2f, 2f)), out hit, known.target.info.height, NavMesh.AllAreas))
                {
                    if (self_agent.CalculatePath(hit.position, path))
                    {
                        if (path.corners.Length > 3)
                        {
                            self_agent.CalculatePath(path.corners[path.corners.Length - 2], path);
                        }
                        self_agent.SetPath(path);
                        next_time_path = Time.time + 10;
                    }
                }
            }
            if (known.target != null)
            {
                if ((self_agent.pathEndPosition - rel.central_position).magnitude < rel.height)
                {
                    if (NavMesh.SamplePosition((((known.target.position - known.target.previous_position) / known.time_interval) * 1f) + known.target.position + 
                        new Vector3(UnityEngine.Random.Range(-2f, 2f), 0, UnityEngine.Random.Range(-2f, 2f)), out hit, known.target.info.height * 8, NavMesh.AllAreas))
                    {
                        self_agent.CalculatePath(hit.position, path);
                        self_agent.SetPath(path);
                    }
                }
            }
            yield return null;
        }
    }

    private IEnumerator runForward()
    {
        self_agent.updateRotation = false;
        self_agent.speed = 5f;
        float next_time_update = 0f;
        StartCoroutine(aim_gun());
        StartCoroutine(shoot_on_see());
        while(true)
        {
            if(Time.time > next_time_update && known.target != null)
            {
                simplePath(rel.central_position + (transform.forward.normalized * UnityEngine.Random.Range(0, 10f) 
                    + transform.right * UnityEngine.Random.Range(-5f, 5f)), rel.height * 2);
                next_time_update = Time.time + 2.5f;
            }
            yield return null;
        }
    }


    private IEnumerator flank()
    {
        self_agent.speed = 8;
        self_agent.updateRotation = true;
        bool first_flank = true;
        Vector3 goal_position = new Vector3();
        float next_time_update = 0f;
        while(true)
        {
            if(Time.time > next_time_update)
            {
                if(!first_flank)
                {
                    StartCoroutine(shoot_on_see());
                    StartCoroutine(aim_gun());
                    self_agent.updateRotation = false;
                }
                int mean_counter = 0;
                NavMeshHit hit = new NavMeshHit();
                NavMeshPath path1 = new NavMeshPath();
                float cover_average1 = 0;
                bool point_here = false;
                RaycastHit[] hits = Physics.RaycastAll(new Vector3(local_enemy_center.x + ((local_enemy_center - rel.central_position).normalized.z * 25),
                    local_enemy_center.y + 100, local_enemy_center.z - ((local_enemy_center - rel.central_position).normalized.x * 25)), Vector3.down, 200f, -1);
                for(int i = 0; i < hits.Length; i++)
                {
                    if(NavMesh.SamplePosition(hits[i].point, out hit, 0.5f, NavMesh.AllAreas))
                    {
                        point_here = true;
                        break;
                    }
                }
                if(point_here)
                {
                    if(self_agent.CalculatePath(hit.position, path1))
                    {
                        foreach(Vector3 v in path1.corners)
                        {
                            float f = 0;
                            test(v, out f);
                            cover_average1 += f;
                            mean_counter++;
                        }
                    }
                }

                cover_average1 /= mean_counter;
                float cover_average2 = 0;
                NavMeshPath path2 = new NavMeshPath();
                point_here = false;
                hits = Physics.RaycastAll(new Vector3(local_enemy_center.x - ((local_enemy_center - rel.central_position).normalized.z * 25)
                    , local_enemy_center.y + 100, local_enemy_center.z + ((local_enemy_center - rel.central_position).normalized.x * 25)), Vector3.down, 200f, -1);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (NavMesh.SamplePosition(hits[i].point, out hit, 0.5f, NavMesh.AllAreas))
                    {
                        point_here = true;
                        break;
                    }
                }
                if (point_here)
                {
                    if (self_agent.CalculatePath(hit.position, path2))
                    {
                        foreach (Vector3 v in path2.corners)
                        {
                            float f = 0;
                            test(v, out f);
                            cover_average2 += f;
                            mean_counter++;
                        }
                    }
                }
                
                cover_average2 /= mean_counter;

                if(cover_average1 > cover_average2)
                {
                    self_agent.SetPath(path1);
                    if(path1.corners.Length > 1)
                    {
                        goal_position = path1.corners[path1.corners.Length - 2];
                    }
                    else if(path1.corners.Length == 1)
                    {
                        goal_position = path1.corners[path1.corners.Length - 1];
                    }
                }
                else
                {
                    self_agent.SetPath(path2);
                    if (path2.corners.Length > 1)
                    {
                        goal_position = path2.corners[path2.corners.Length - 2];
                    }
                    else if(path2.corners.Length == 1)
                    {
                        goal_position = path2.corners[path2.corners.Length - 1];
                    }
                }
                next_time_update = Time.time + 8;
            }
            if((rel.central_position - goal_position).magnitude < rel.height && goal_position != Vector3.zero)
            {
                next_time_update -= 60 * Time.deltaTime;
            }
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        foreach(Tuple<Vector3, float, int> point in cover_spots)
        {
            Gizmos.color = new UnityEngine.Color((float)point.Item3 / (float)5, (float)point.Item3 / (float)5, 1);
            Gizmos.DrawSphere(point.Item1, 0.5f);
        }
        Gizmos.color = UnityEngine.Color.green;
        for(int i = 0; i < self_agent.path.corners.Length; i++)
        {
            Gizmos.DrawCube(self_agent.path.corners[i], Vector3.one);
        }
        Gizmos.color = UnityEngine.Color.red;
        Gizmos.DrawSphere(local_enemy_center, 1.5f);
    }
}
