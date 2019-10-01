using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegetationSystem : MonoBehaviour
{
    private List<Plant> _vegetationSystem = new List<Plant>();

    private void Start()
    {
        GetComponent<TrackAndPassTime>().OnPassDay += OnDayPassed;
    }

    private void OnDayPassed()
    {

    }
}
