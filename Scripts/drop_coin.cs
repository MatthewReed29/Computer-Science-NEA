using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class drop_coin : MonoBehaviour
{
    public GameObject coin;
    Subtract_Health health;
    private bool placed = false;

    private void Start()
    {
        health = GetComponent<Subtract_Health>();
    }

    private void Update()
    {
        if(health.health <= 0 && !placed && health.health != -20)
        {
            StartCoroutine(place_coin(health.info.central_position, health.despawn_time));
            placed = true;
        }

    }

    private IEnumerator place_coin(Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay - 1f);
        Instantiate(coin, position, Quaternion.identity);
    }

}
