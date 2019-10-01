using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackAndPassTime : MonoBehaviour
{
    public int ElapsedDays { get; private set; } = 0;

    public void PassDay()
    {
        ++ElapsedDays;
    }
}
