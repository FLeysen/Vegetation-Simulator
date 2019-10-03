using UnityEngine;

public interface IActOnDayPassing
{
    void OnDayPassed();
}

public class Plant : MonoBehaviour
{
    public VegetationSystem VegetationSys { get; set; } = null;
    private IActOnDayPassing[] _actOnDayPassingBehaviours = null;

    private void OnDestroy()
    {
        VegetationSys.DeregisterPlant(this);
    }

    private void Start()
    {
        _actOnDayPassingBehaviours = GetComponents<IActOnDayPassing>();
    }

    public void OnDayPassed()
    {
        foreach(IActOnDayPassing actingBehaviour in _actOnDayPassingBehaviours)
            actingBehaviour.OnDayPassed();
    }
}
