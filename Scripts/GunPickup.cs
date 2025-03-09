using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GunPickup : MonoBehaviour

{
    public spawn_bullet gunScript;
    public Rigidbody rb;
    private GameObject player;
    private Transform player_camera;
    private CapsuleCollider cap_collider;

    public float pick_up_range;
    public float drop_impulse;

    public Vector3 offset_rotation;
    public Vector3 offset_position;

    public bool equiped = false;
    static bool slot_full = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cap_collider = GetComponent<CapsuleCollider>();
        gunScript = GetComponent<spawn_bullet>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            player = GameObject.FindGameObjectWithTag("player");
            if(player != null)
            {
                if ((player.transform.position - transform.position).magnitude < pick_up_range && slot_full == false)
                {
                    player_camera = player.GetComponentInChildren<Camera>().transform;
                    Pick_up();
                }
            }


        }
        if (equiped && Input.GetKey(KeyCode.F))
        {
            Drop_weapon();
        }
    }

    void Pick_up()
    {
        equiped= true;
        slot_full = true;
        gunScript.enabled = true;
        rb.useGravity = false;
        cap_collider.enabled = false;

        transform.SetParent(player_camera, true);
        transform.localPosition = offset_position;
        transform.localRotation = Quaternion.Euler(offset_rotation);
        transform.localScale = Vector3.one;
        rb.isKinematic = true;
        
    }

    private void OnDestroy()
    {
        slot_full = false;
    }

    void Drop_weapon()
    {
        equiped= false;
        slot_full = false;
        gunScript.enabled = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        cap_collider.enabled = true;

        transform.SetParent(null);
        rb.velocity = player.GetComponent<Rigidbody>().velocity;
        rb.AddForce(player.transform.forward * drop_impulse, ForceMode.Impulse);
    }
}
    

