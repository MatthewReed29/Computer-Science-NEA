using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class coin_pickup : MonoBehaviour
{
    public float give_health;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "player")
        {

            other.gameObject.GetComponent<Subtract_Health>().change_health(give_health);
            if(other.gameObject.GetComponent<relevant_information>().max_health - give_health< other.gameObject.GetComponent<Subtract_Health>().health)
            {
                other.gameObject.GetComponent<Subtract_Health>().health = other.gameObject.GetComponent<relevant_information>().max_health;
            }
            other.gameObject.GetComponent<relevant_information>().score++;
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        transform.Rotate(1,0,0);
    }

}
