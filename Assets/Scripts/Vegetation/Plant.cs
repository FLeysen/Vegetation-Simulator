using UnityEngine;

public interface IActOnDayPassing
{
    void OnDayPassed();
}

public class Plant : MonoBehaviour
{
    public VegetationSystem VegetationSys { get; set; } = null;
    public float ShadowFactor { get; set; } = 0f;
    public float LifeForce = 1f;

    private IActOnDayPassing[] _actOnDayPassingBehaviours = new IActOnDayPassing[] { };

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
