using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class start_game : MonoBehaviour
{
    public int seed = 0;
    public int renderDistance = 200;
    public int targetFramerate = 60;
    public int chunk_size = 100;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void setSeed(string input)
    {
        seed = to_int(input);
    }

    public void setRenderDistance(string input)
    {
        renderDistance = to_int(input);
    }

    public void setTargetFramerate(string input)
    {
        targetFramerate = to_int(input);
    }

    public void setChunk_Size(string input)
    {
        chunk_size = to_int(input);
    }
    public void StartGame()
    {
        start_statics.seed = seed;
        start_statics.chunk_size = chunk_size;
        start_statics.renderDistance = renderDistance;
        start_statics.targetFramerate = targetFramerate;
        SceneManager.LoadScene(1);
    }

    public static int to_int(string s)
    {
        Char[] chars = s.ToCharArray();
        int result = 0;
        int negative;
        String[] numbers = new String[10];
        if (s.Length == 0)
        {
            return 0;
        }
        for (int i = 0; i < 10; i++)
        {
            numbers[i] = (i.ToString());
        }
        if (chars[0] == '-')
        {
            negative = 1;
        }
        else
        {
            negative = 0;
        }
        for (int c = negative; c < chars.Length; c++)
        {
            bool valid_num = false;
            for (int n = 0; n < numbers.Length; n++)
            {
                if (chars[c].ToString() == numbers[n])
                {
                    if (negative == 0)
                    {
                        result += (int)(Mathf.Pow(10f, (float)(chars.Length - c - 1)) * n);
                    }
                    else
                    {
                        result -= (int)(Mathf.Pow(10f, (float)(chars.Length - c - 1)) * n);
                    }

                    valid_num = true;
                }
            }
            if (!valid_num)
            {
                return 0;
            }
        }
        return result;
    }
}
