using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnToReact : MonoBehaviour
{
    Animator animator;
    int wait_for;
    bool is_reacting;
    AudioSource sound;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        sound = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space) & (wait_for == 0))
        {
            animator.SetBool("return_to_walk", false);
            animator.SetBool("Reaction", true);
            wait_for = 300;
            is_reacting= true;

        }
        if ((wait_for == 0) & (is_reacting == true))
        {

        }
       // Debug.Log(wait_for);
       // Debug.Log(animator.GetBool("return_to_walk"));
      //  Debug.Log(animator.GetBool("Reaction"));
        //Debug.Log(is_reacting);
        //Debug.Log();
    }

    void FixedUpdate()
    {
        if (wait_for > 0)
        {
            wait_for = wait_for - 1;
        }
    }
    void rotate_to_normal()
    {
        animator.SetBool("Reaction", false);
        animator.SetBool("return_to_walk", true);
        is_reacting = false;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }
    void play_walkin_here()
    {
        sound.Play();
    }

}
