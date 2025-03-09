using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class start_statics : MonoBehaviour
{
    static public int seed;
    static public int targetFramerate;
    static public int renderDistance;
    static public int chunk_size;

    private void Start()
    {
        AI_Store.start();
        if(targetFramerate < 20)
        {
            Application.targetFrameRate = 60;
        }
        else{
            Application.targetFrameRate = targetFramerate;
        }
        QualitySettings.vSyncCount = 0;
    }

    private void OnDestroy()
    {
        AI_Store.end();
        globalTracker.Clear();
    }
}
