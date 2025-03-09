using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate_sun : MonoBehaviour
{
    void FixedUpdate()
    {
        transform.Rotate(0.02f, 0, 0);
    }
}
