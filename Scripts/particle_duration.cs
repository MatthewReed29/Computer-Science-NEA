using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particle_duration : MonoBehaviour
{
    public float duration;
    void Start()
    {
        Invoke("destroy", duration);
    }

    void destroy()
    {
        Destroy(this.gameObject);
    }
}
