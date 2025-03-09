using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class spawn_on_ground : MonoBehaviour
{
    public GameObject spawn_object;
    private GameObject spawned;
    public int allegiance;
    public int spawn_number = 3;
    public float spawn_delay = 10f;
    public float later_spawn_delay = 2.3f;

    bool spawn(Vector3 relative_position)
    {
        Ray ray = new Ray(transform.position + new Vector3(relative_position.x, 100, relative_position.z), -Vector3.up);
        RaycastHit ground;
        NavMeshHit h;
        if ((Physics.Raycast(ray, out ground)) && spawn_object != null && NavMesh.SamplePosition(ground.point, out h, 0.5f, NavMesh.AllAreas))
        {
            ground.point.Set(ground.point.x, ground.point.y + 1, ground.point.z);
            spawned = Instantiate(spawn_object, new Vector3(ground.point.x ,ground.point.y + spawn_object.GetComponent<relevant_information>().height + 1, ground.point.z), transform.rotation);
            globalTracker.add_target(spawned);
            spawned.GetComponent<relevant_information>().SetAllegiance(allegiance);
            return true;
        }
        else
        {
            Debug.Log("No ground");
            return false;
        }
    }

    IEnumerator innitial_spawn()
    {
        yield return new WaitForSeconds(spawn_delay);
        for(int i = 0; i < spawn_number; i++)
        {
            if(!spawn(new Vector3(Random.Range(-2f, 2f),0,  Random.Range(-2f, 2f))))
            {
                i--;
            }
            yield return new WaitForSeconds(later_spawn_delay);
        }
    }

    private void Awake()
    {
        Random.seed = (int)(transform.position.x * transform.position.z);
        allegiance = UnityEngine.Random.Range(0, 1000);
        StartCoroutine(innitial_spawn());
    }
}
