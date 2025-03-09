using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class fern_placement1 : object_script
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
            NavMeshModifier mod = hit.collider.gameObject.GetComponent<NavMeshModifier>();
            if (mod != null)
            {
                NavMeshModifierVolume vol = GetComponent<NavMeshModifierVolume>();
                if(NavMesh.GetAreaCost(vol.area) < NavMesh.GetAreaCost(mod.area))
                {
                    vol.area = mod.area;
                }
            }
        }
    }
}
