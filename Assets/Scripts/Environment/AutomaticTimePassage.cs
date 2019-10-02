using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticTimePassage : MonoBehaviour
{
    [SerializeField] private KeyCode _pauseButton = KeyCode.Space;
    [SerializeField] private float _secondsPerDay = 3f;
    private float _elapsedToday = 0f;
    private bool _isPaused = false;

    private TrackAndPassTime _time = null;

    void Start()
    {
        _time = GetComponent<TrackAndPassTime>(); 
    }

    void Update()
    {
        if (Input.GetKeyDown(_pauseButton))
            _isPaused = !_isPaused;

        if (!_isPaused)
        {
            if ((_elapsedToday += Time.deltaTime) > _secondsPerDay)
            {
                _time.PassDay();
                _elapsedToday -= _secondsPerDay;
            }
        }
    }
}
