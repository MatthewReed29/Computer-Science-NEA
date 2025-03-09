using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animate_barrel : MonoBehaviour
{
    public ParticleSystem particle_system;
    public void muzzel_effect()
    {
        particle_system.Emit(6);
    }
}
