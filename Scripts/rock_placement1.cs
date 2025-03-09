using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class rock_placement1 : object_script
{
    public override void sort_nav_and_rotate()
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z), -Vector3.up, out hit))
        {
            if(hit.normal.z != 0)
            {
                transform.rotation = Quaternion.LookRotation(new Vector3(0, 1, -((-hit.normal.y) / (-hit.normal.z))), hit.normal);
            }
            
        }
    }
}
