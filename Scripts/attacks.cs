using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class attacks : MonoBehaviour
{
    public bool has_melee;
    public bool has_gun;
    public bool has_grenade;
    public float gun_cooldown;
    public GameObject gun;
    public spawn_bullet gun_script;

    public virtual void melee()
    {

    }

    public virtual void shoot()
    {

    }

    public virtual void grenade()
    {

    }
}
