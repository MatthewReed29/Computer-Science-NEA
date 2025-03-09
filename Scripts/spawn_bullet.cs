using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class spawn_bullet : MonoBehaviour
{
    public GameObject bullet;
    public bool player;
    public Vector3 launch_direction;
    public float cooldown;
    private GameObject new_bullet;
    public float launch_velocity;
    public Vector3 shoot_offset;
    Vector3 relative_position;
    private animate_barrel animate_barrel;
    private float next_time_fire;

    private void OnEnable()
    {
        animate_barrel = GetComponent<animate_barrel>();
    }

    void Update()
    {
        if (player && Input.GetKey(KeyCode.Mouse0) & next_time_fire <= Time.time)
        {
            create_bullet();
            next_time_fire = Time.time + cooldown;
        }
    }

    public void create_bullet()
    {
        relative_position = Vector3.zero;
        relative_position += (transform.right * shoot_offset.x);
        relative_position += (transform.up * shoot_offset.y);
        relative_position += (transform.forward * shoot_offset.z);
        new_bullet = Instantiate(bullet, transform.position + relative_position, transform.rotation);
        new_bullet.GetComponent<Rigidbody>().AddForce
           ((transform.right * launch_direction.x + transform.up * launch_direction.y + transform.forward * launch_direction.z) * launch_velocity, ForceMode.Impulse);
        if(animate_barrel != null)
        {
            animate_barrel.muzzel_effect();
        }
    }
}
