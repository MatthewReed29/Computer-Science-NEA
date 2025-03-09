using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class die_when_hit : MonoBehaviour
{
    public float despawn_time;
    public int impact_damage;
    public GameObject partical_system;
    public bool explode_on_impact;
    public float bullet_impulse;
    void Start()
    {
        Invoke("Destroy_self", despawn_time);
    }
    private void OnTriggerEnter(Collider col)
    {
        Blast_damage blast_Damage = GetComponent<Blast_damage>();
        Subtract_Health health_script;
        health_script = col.GetComponent<Subtract_Health>();
        if (health_script != null)
        {
            health_script.change_health(-impact_damage);
        }
        if (explode_on_impact && blast_Damage != null)
        {
            blast_Damage.explosion_damage();
        }
        try
        {
            col.GetComponent<Rigidbody>().AddForce(GetComponent<Rigidbody>().velocity.normalized * bullet_impulse, ForceMode.Impulse);
        }
        catch
        {

        }
        if (partical_system != null)
        {
            Instantiate(partical_system, transform.position, Quaternion.identity);
        }
        Destroy(this.gameObject);
    }
    void Destroy_self()
    {
        Destroy(this.gameObject);
    }
}
