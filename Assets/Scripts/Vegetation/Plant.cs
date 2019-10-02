using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ActOnDayPassing
{
    void OnDayPassed();
}

public class Plant : MonoBehaviour
{
    public VegetationSystem VegetationSys { get; set; } = null;
    private ActOnDayPassing[] _actOnDayPassingBehaviours = null;

    private void OnDestroy()
    {
        VegetationSys.DeregisterPlant(this);
    }

    private void Start()
    {
        _actOnDayPassingBehaviours = GetComponents<ActOnDayPassing>();
    }

    public void OnDayPassed()
    {
        foreach(ActOnDayPassing actingBehaviour in _actOnDayPassingBehaviours)
            actingBehaviour.OnDayPassed();
    }
}
