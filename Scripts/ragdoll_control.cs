using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ragdoll_control : MonoBehaviour
{
    private CapsuleCollider main_col;
    // Start is called before the first frame update
    void Start()
    {
        main_col = GetComponent<CapsuleCollider>();
        change_from_ragdoll();
    }

    public void change_to_ragdoll()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach(Collider c in colliders)
        {
            if(c.gameObject != this.gameObject)
            {
                c.isTrigger = false;
            }
        }
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody r in rigidbodies)
        {
            if(r.gameObject != this.gameObject)
            {
                r.isKinematic = false;
                r.useGravity = true;
            }
        }
        main_col.enabled= false;
        if (GetComponent<NavMeshAgent>() != null)
        {
            GetComponent<NavMeshAgent>().enabled = false;
        }
    }
    public void change_from_ragdoll()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            if (c.gameObject != this.gameObject)
            {
                c.isTrigger = true;
            }
        }
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody r in rigidbodies)
        {
            if (r.gameObject != this.gameObject)
            {
                r.isKinematic = true;
                r.useGravity = false;
            }
        }
        main_col.enabled = true;
        if(GetComponent<NavMeshAgent>() != null)
        {
            GetComponent<NavMeshAgent>().enabled = true;
        }
        
        
    }
}
