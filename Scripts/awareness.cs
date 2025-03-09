using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;


public class transformData
{
    public Vector3 position;
    public Vector3 previous_position;
    public Quaternion rotation;
    public GameObject host;
    public float last_updated;
    public bool objects_seen;
    public bool ghost_seen = false;
    public relevant_information info;

    public transformData(Vector3 position, Quaternion rotation, Vector3 eulerRotation, GameObject host)
    {
        this.position = position;
        this.rotation = rotation;
        this.previous_position = position;
        this.host = host;
        this.info = host.GetComponent<relevant_information>();
        this.last_updated = Time.time;
    }

    public void update_data()
    {
        this.previous_position = position;
        this.position = info.central_position;
        this.rotation = host.transform.rotation;
        this.last_updated = Time.time;
        objects_seen = true;
    }

    public void recieve_shared(transformData input)
    {
        this.previous_position = input.previous_position;
        this.rotation = input.rotation;
        this.position = input.position;
        this.last_updated = input.last_updated;
    }
}


public class awareness : MonoBehaviour
{
    public List<transformData> ghost_targets = new List<transformData>(); //list of known information about different targets
    public float field_of_view; // the sight field of view for which the enemies are able to see in
    public float seeing_range;
    public float spacial_awareness; //the distance at which (when already seen) a target is considered always seen
    private float update_delay_store; //processing variable for sight time intervals
    private float next_time_broadcast; //processing variable for information sharing intervals
    public float time_interval; //editor input variable for time between sight updates
    private relevant_information info; // reference to relevant_information component on this pbject
    private relevant_information[] store_tracker; // storage variable for the globalTracker knowables list
    private List<RaycastHit> hits = new List<RaycastHit>(); //variable used to hold raycast hits
    private Vector3 store_vect; // debug vector that probably should be removed
    public float forget_time; // editor input variable which decides how long after a target is last seen should it be forgotten
    public float retarget_time; //editor input variable for time between retarget updates
    private float last_time_target; //store variable for the time delay for retarget updates
    public transformData target; // the current combatant target for this NPC
    public NavMeshAgent self_agent; //reference to the NavMeshAgent component on this object

    private void Start()
    {
        info = GetComponent<relevant_information>();
        StartCoroutine(unload());
    }


    public IEnumerator unload()
    {
        self_agent = GetComponent<NavMeshAgent>();
        bool already_off = false;
        while (true)
        {
            yield return new WaitForSeconds(5f);
            if(self_agent.isOnNavMesh)
            {
                if ((!self_agent.CalculatePath(info.central_position, new NavMeshPath()) && already_off))
                {
                    GetComponent<Subtract_Health>().change_health(-info.health - 20);
                }
                else if (!self_agent.CalculatePath(info.central_position, new NavMeshPath()))
                {
                    already_off = true;
                }
                else
                {
                    already_off = false;
                }
            }
            else
            {
                if(already_off)
                {
                    GetComponent<Subtract_Health>().change_health(-info.health - 20);
                }
                else
                {
                    already_off = true;
                }
            }

        }
    }

    void Update()
    {
        if(Time.time > update_delay_store)
        {
            update_delay_store = Time.time + time_interval;
            store_tracker = globalTracker.knowables.ToArray();
            for (int target_index = 0; target_index < store_tracker.Length; target_index++)
            {
                if (store_tracker[target_index].alive && store_tracker[target_index] != info)
                {
                    if ((store_tracker[target_index].central_position - transform.position).magnitude <= seeing_range)
                    {   
                        if (store_tracker[target_index].allegiance != info.allegiance)
                        {
                            if ((360 / (2 * Mathf.PI)) * Mathf.Acos(Vector3.Dot(info.eye_level.forward,
                                (store_tracker[target_index].eye_level.position - info.eye_level.position).normalized)) < (field_of_view / 2f))
                            {
                                bool unknown = true;
                                int index_equal = 0;
                                if (ghost_targets.Count > 0)
                                {
                                    for (int g = 0; g < ghost_targets.Count; g++)
                                    {
                                        if (store_tracker[target_index].gameObject == ghost_targets[g].host)
                                        {
                                            unknown = false;
                                            index_equal = g;
                                        }
                                    }
                                }

                                if (unknown)
                                {
                                    hits = new List<RaycastHit>();
                                    foreach (Transform t in store_tracker[target_index].view_points)
                                    {
                                        RaycastHit hit;
                                        Vector3 relative_vector = new Vector3(t.position.x - info.eye_level.transform.position.x,
                                            t.position.y - info.eye_level.transform.position.y, t.position.z - info.eye_level.transform.position.z);
                                        store_vect = relative_vector;
                                        if (Physics.Raycast(info.eye_level.position, relative_vector, out hit, seeing_range))
                                        {
                                            hits.Add(hit);
                                            if (hit.collider.gameObject == store_tracker[target_index].gameObject && hit.collider.gameObject 
                                                 != this.gameObject)
                                            {
                                                ghost_targets.Add(new transformData(hit.collider.gameObject.GetComponent<relevant_information>().central_position,
                                                    hit.collider.gameObject.transform.rotation, hit.collider.gameObject.transform.rotation.eulerAngles, hit.collider.gameObject));
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    hits = new List<RaycastHit>();
                                    foreach (Transform t in store_tracker[target_index].view_points)
                                    {
                                        RaycastHit hit;
                                        Vector3 relative_vector = new Vector3(t.position.x - info.eye_level.transform.position.x, 
                                            t.position.y - info.eye_level.transform.position.y, t.position.z - info.eye_level.transform.position.z);
                                        store_vect = relative_vector;
                                        if (Physics.Raycast(info.eye_level.position, relative_vector, out hit, seeing_range))
                                        {
                                            hits.Add(hit);
                                            if (hit.collider.gameObject == store_tracker[target_index].gameObject && hit.collider.gameObject != this.gameObject)
                                            {
                                                ghost_targets[index_equal].update_data();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (store_tracker[target_index].gameObject != this.gameObject)
                        {
                            bool unknown = true;
                            int index_equal = 0;
                            if (info.allies.Count > 0)
                            {
                                for (int g = 0; g < info.allies.Count; g++)
                                {
                                    if (store_tracker[target_index] == info.allies[g])
                                    {
                                        unknown = false;
                                        index_equal = g;
                                    }
                                }
                            }
                            if (unknown)
                            {
                                info.allies.Add(store_tracker[target_index]);
                            }
                        }
                    }
                }
            }
            for(int ghost_index = 0; ghost_index < ghost_targets.Count; ghost_index++)
            {
                if (!ghost_targets[ghost_index].objects_seen)
                {
                    RaycastHit hit = new RaycastHit();
                    Physics.Raycast(info.eye_level.position, ghost_targets[ghost_index].position - info.eye_level.position, out hit, seeing_range);
                    if ((hit.point - info.central_position).magnitude < (hit.point - ghost_targets[ghost_index].position).magnitude)
                    {
                        if (hit.collider == null)
                        {
                            ghost_targets[ghost_index].ghost_seen = true;
                        }
                        else if(hit.collider.gameObject == ghost_targets[ghost_index].host)
                        {
                            ghost_targets[ghost_index].ghost_seen = true;
                        }else if((ghost_targets[ghost_index].position - info.eye_level.transform.position).magnitude <= spacial_awareness)
                        {
                            ghost_targets[ghost_index].ghost_seen = true;
                        }
                        else
                        {
                            ghost_targets[ghost_index].ghost_seen = false;
                        }
                    }
                    else
                    {
                        ghost_targets[ghost_index].ghost_seen = true;
                    }
                }
                else
                {
                    ghost_targets[ghost_index].ghost_seen = true;
                }
            }
            for (int i = 0; i < ghost_targets.Count; i++)
            {
                if (ghost_targets[i].last_updated + forget_time < Time.time || !ghost_targets[i].info.alive)
                {

                    ghost_targets.RemoveAt(i);

                    if (ghost_targets.Count == 0)
                    {
                        target = null;
                    }
                    else
                    {
                        target = null;
                        select_target();
                    }
                }
            }
            foreach(transformData t in ghost_targets)
            {
                if(Time.time - t.last_updated > time_interval)
                {
                    t.objects_seen = false;
                }
            }
            if(Time.time - last_time_target >= retarget_time)
            {
                select_target();
                last_time_target = Time.time;
            }
            
        }

        if(Time.time > next_time_broadcast)
        {
            share_information();
            next_time_broadcast = Time.time + 5f;
        }

    }

    public void select_target()
    {
        if(target != null)
        {
            if (ghost_targets.Count > 0)
            {
                float lowest = float.MaxValue;
                int lowest_index = -1;
                for (int i = 0; i < ghost_targets.Count; i++)
                {
                    if (ghost_targets[i].objects_seen)
                    {
                        if ((ghost_targets[i].position - transform.position).magnitude * Vector3.Angle(info.eye_level.forward, (ghost_targets[i].position - transform.position).normalized) < lowest)
                        {
                            lowest = (ghost_targets[i].position - transform.position).magnitude * Vector3.Angle(info.eye_level.forward, (ghost_targets[i].position - transform.position).normalized);
                            lowest_index = i;
                        }
                        
                    }
                }
                if(lowest_index != -1)
                {
                    target = ghost_targets[lowest_index];
                }
            }else if(ghost_targets.Count == 0)
            {
                target = null;
            }
        }else
        {
            if (ghost_targets.Count > 0)
            {
                float closest = (ghost_targets[0].position - info.central_position).magnitude;
                int index = 0;
                for(int i = 0; i < ghost_targets.Count; i++)
                {
                    if((ghost_targets[i].position - info.central_position).magnitude < closest)
                    {
                        closest = (ghost_targets[i].position - info.central_position).magnitude;
                        index = i;
                    }
                }

                target = ghost_targets[index];
            }
        }
    }

    public void share_information()
    {
        for(int i = 0; i < info.allies.Count; i++)
        {
            info.allies[i].recieve_teamate_info(this);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = 
            new Color(info.allegiance * 255, info.allegiance * 255, 0);
        if(hits.Count > 0)
        {
            for(int i = 0; i < hits.Count; i++)
            {
                Gizmos.DrawSphere(hits[i].point, 0.5f);
                Gizmos.DrawLine(this.gameObject.transform.position + store_vect.normalized, hits[i].point);
            }
        }
        
        for(int i = 0; i < ghost_targets.Count; i++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(ghost_targets[i].position, new Vector3(1,3,1));
        }
    }
}