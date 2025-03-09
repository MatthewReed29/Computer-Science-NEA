using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Subtract_Health : MonoBehaviour
{
    public ragdoll_control ragdoll;
    public float health;
    public float despawn_time;
    public relevant_information info;
    private void Awake()
    {
        info = GetComponent<relevant_information>();
        health = info.max_health;
        info.health = health;
    }
    public void change_health(float change)
    {
        health += change;
        info.health = health;
        if (health <= 0)
        {
            if (ragdoll != null)
            {
                ragdoll.change_to_ragdoll();
            }
            if(!info.player)
            {
                Invoke("destroy_self", despawn_time);
                info.alive = false;
                globalTracker.remove_target(this.gameObject);
                AI_Store.remove_target(this.gameObject);
                GetComponent<relevant_information>().enabled = false;
                GetComponent<awareness>().enabled = false;
                GetComponent<decisions>().dead();
            }
            else
            {
                globalTracker.remove_target(gameObject);
                Destroy(gameObject);
            }
        }
    }
    private void destroy_self()
    {
        Destroy(this.gameObject);
    }
}
