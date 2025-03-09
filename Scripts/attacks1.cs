using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attacks1 : attacks
{
    private void Awake()
    {
        gun = GetComponentInChildren<spawn_bullet>().gameObject;
        gun_script = gun.GetComponent<spawn_bullet>();
        gun_cooldown = gun_script.cooldown;
    }

    public override void shoot()
    {
        gun_script.create_bullet();
    }
}
