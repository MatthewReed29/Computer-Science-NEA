using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawn_player : MonoBehaviour
{
    public GameObject player;
    [SerializeField]
    GameObject g;
    public GameObject[] weapons;
    float spawn_delay = 5;
    float time_delay_store = 0f;
    void Update()
    {
        if(g == null && Time.time > time_delay_store)
        {
            time_delay_store = Time.time + 1f;
            if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y, transform.position.z), Vector3.down, 1000))
            {
                Invoke("spawn", spawn_delay);
                time_delay_store = Time.time + spawn_delay + 1f; ;
            }
        }
    }

    public void spawn()
    {
        g = Instantiate(player, this.gameObject.transform.position, Quaternion.identity);
        globalTracker.add_target(g);
        for (int i = 0; i < weapons.Length; i++)
        {
            Instantiate(weapons[i], transform.position, Quaternion.identity);
        }
    }
}
