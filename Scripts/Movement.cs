using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Movement : MonoBehaviour
{
    private bool ground = false;
    private float movement_speed;
    public float ground_drag;
    public float ground_movement;
    public float air_drag;
    public float air_movement;
    Rigidbody rigid;
    Animator animator;
    public float jump_impulse;
    private Vector3 velocity;
    private float mouse_up;
    private float mouse_right;
    public Camera cam;
    public float sensitivity;
    private float max_allowed_turn_down;
    private float max_allowed_turn_up;
    private relevant_information info;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        info = GetComponent<relevant_information>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            rigid.AddForce(transform.forward * movement_speed * Time.deltaTime * 50, ForceMode.Impulse);
        }
        if (Input.GetKey(KeyCode.S))
        {
            rigid.AddForce(-transform.forward * movement_speed * Time.deltaTime * 50, ForceMode.Impulse);
        }
        if(Input.GetKey(KeyCode.A))
        {
            rigid.AddForce(-transform.right * movement_speed * Time.deltaTime * 50, ForceMode.Impulse);
        }
        if(Input.GetKey(KeyCode.D))
        {
            rigid.AddForce(transform.right * movement_speed * Time.deltaTime * 50, ForceMode.Impulse);
        }
        if (Input.GetKey(KeyCode.Space) && ground == true)
        {
            animator.SetBool("Jump", true);
        }
        mouse_right = Input.GetAxis("Mouse X");
        mouse_up = Input.GetAxis("Mouse Y");
        transform.Rotate(0, mouse_right * sensitivity, 0);

        if(mouse_up > 0f) // divides scenario based on turning up or turning down
        {
            if (cam.transform.rotation.eulerAngles.x > 180) //decides if the rotation is looking up or down
            {
                max_allowed_turn_up = cam.transform.rotation.eulerAngles.x - 275; //sets max rotation upwards to 275
            }
            else
            {
                max_allowed_turn_up = cam.transform.rotation.eulerAngles.x + 85; //sets max rotation upwards to 275
            }

            if(mouse_up * sensitivity > max_allowed_turn_up) // if past maximum rotation
            {
                mouse_up = (max_allowed_turn_up) / sensitivity; // turn just below limit
            }
        }
        else if(mouse_up < 0f)
        {
            if(cam.transform.rotation.eulerAngles.x < 180)
            {
                max_allowed_turn_down = 85 - cam.transform.rotation.eulerAngles.x; //sets max rotation to 85
            }
            else
            {
                max_allowed_turn_down = 360 - cam.transform.rotation.eulerAngles.x + 85; //sets max rotation to 85 
            }
            
            if(-mouse_up * sensitivity > max_allowed_turn_down)
            {
                mouse_up = (-max_allowed_turn_down) / sensitivity;
            }
        }
        cam.transform.Rotate(-mouse_up * sensitivity, 0,0);
    }
    void FixedUpdate()
    {        
        velocity = rigid.velocity;

        animator.SetFloat("Walk_speed", Vector3.Dot(velocity, transform.forward) * 0.1f);

        if (new Vector2(velocity.x, velocity.z).magnitude > 0.1 && ground)
        {
            animator.SetBool("is_walking", true);

        }else
        {
            animator.SetBool("is_walking", false);
        }
        if (ground == false)
        {
            rigid.drag = air_drag;
            movement_speed = air_movement;
        }
        else
        {
            rigid.drag = ground_drag;
            movement_speed = ground_movement;
        }
        RaycastHit hit;
        if (Physics.Raycast(info.central_position, -transform.up, out hit, (info.height / 2f) + 0.25f ,-1, QueryTriggerInteraction.Ignore))
        {
            if(hit.collider.gameObject != this.gameObject)
            {
                ground = true;
                rigid.drag = ground_drag;
            }
        }
        else
        {
            ground = false;
        }
        if (velocity.y < -0.2 && !ground)
        {
            animator.SetBool("Falling", true);
        }else
        {
            animator.SetBool("Falling", false);
        }
    }
    void jump()
    {
        rigid.AddForce(transform.up * jump_impulse, ForceMode.Impulse);
        animator.SetBool("Jump", false);
    }
}
