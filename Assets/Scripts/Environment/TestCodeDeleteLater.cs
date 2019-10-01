using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCodeDeleteLater : MonoBehaviour
{
    private TrackAndPassTime _time = null;

    void Start()
    {
        _time = GetComponent<TrackAndPassTime>(); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) _time.PassDay();
        if (Input.GetKeyDown(KeyCode.W))
        {
            for (int i = 0; i < 5; ++i)
                _time.PassDay();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            for (int i = 0; i < 10; ++i)
                _time.PassDay();
        }
    }
}
