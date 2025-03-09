using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class display_values : MonoBehaviour
{
    public relevant_information health_info;
    TextMeshProUGUI health;
    public GameObject healthObj;
    public GameObject frameRateObj;
    public GameObject scoreObj;
    TextMeshProUGUI frameRate;
    TextMeshProUGUI score;
    int count = 0;
    // Start is called before the first frame update
    void Start()
    {
        frameRate = frameRateObj.GetComponent<TextMeshProUGUI>();
        health = healthObj.GetComponent<TextMeshProUGUI>();
        score = scoreObj.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if(count == 5)
        {
            frameRate.text = ((int)(1 / Time.deltaTime)).ToString();
            count = 0;
        }

        health.text = health_info.health.ToString();

        score.text = health_info.score.ToString();

        count++;
    }
}
