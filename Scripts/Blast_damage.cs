using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Blast_damage : MonoBehaviour
{
    public float explosive_damage;
    public float min_explosion_radius;
    public float max_explosion_radius;
    public float explosion_impulse;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void explosion_damage()
    {
        //Debug.Log("explode");
        Collider[] nearby_object_colliders = Physics.OverlapSphere(transform.position, max_explosion_radius);
        List<GameObject> list_of_done = new List<GameObject>();
        foreach (Collider col in nearby_object_colliders)
        {
            Subtract_Health subtract = col.gameObject.GetComponent<Subtract_Health>();

            if (subtract != null && !list_of_done.Contains(subtract.gameObject))
            {
                list_of_done.Add(subtract.gameObject);
                Vector3 col_transform = col.bounds.center;
                Vector3 difference_between = transform.position - col_transform;
                float health_change = 0;
                if(difference_between.magnitude <= min_explosion_radius)
                {
                    health_change = explosive_damage;
                }
                else
                {
                    health_change = (float)(explosive_damage - (((difference_between.magnitude - min_explosion_radius) / (max_explosion_radius - min_explosion_radius)) * explosive_damage));
                }

                subtract.change_health(-health_change);
                Debug.Log(health_change);
                
            }
            //Debug.Log(list_of_done);
            Rigidbody rigid = GetComponent<Rigidbody>();
            try
            {
                if (col.isTrigger == false)
                {
                    col.attachedRigidbody.AddExplosionForce(explosion_impulse, transform.position, (max_explosion_radius + min_explosion_radius) / 2, 0f, ForceMode.Impulse);
                }
                else
                {
                    //Debug.Log("taken out of physics");
                }
                
            }
            catch
            {

            }
        }
    }
}
