using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class rock_placement2 : object_script
{
    public float gradient_required;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override bool weight_position(int seed, Vector3 terrain_point)
    {
        bool p = base.weight_position(seed, terrain_point);
        if(p)
        {
            RaycastHit hit = new RaycastHit();
            Physics.Raycast(terrain_point + new Vector3(0, 1, 0), Vector3.down, out hit, 5);
            if(Vector3.Angle(Vector3.up, hit.normal) > gradient_required)
            {
                return true;
            }
        }
        return false;
    }

    

    public override void sort_nav_and_rotate()
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z), -Vector3.up, out hit))
        {
            if (hit.normal.z != 0)
            {
                transform.rotation = Quaternion.LookRotation(new Vector3(0, 1, -((-hit.normal.y) / (-hit.normal.z))), hit.normal);
            }

        }


        Random.seed = (int)(transform.position.magnitude * transform.position.x);
        transform.Rotate(new Vector3(0, Random.Range(-180, 180), 0));
    }

}
