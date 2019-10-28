using UnityEngine;

public interface IActOnDayPassing
{
    void OnDayPassed();
}

public interface IActOnAttemptDestroy
{
    void OnAttemptDestroy(Vector3Int pos);
}

public class Plant : MonoBehaviour
{
    public VegetationSystem VegetationSys { get; set; } = null;
    public float ShadowFactor { get; set; } = 0f;
    public float LifeForce = 1f;

    private IActOnDayPassing[] _actOnDayPassingBehaviours = new IActOnDayPassing[] { };
    private IActOnAttemptDestroy[] _actOnAttemptDestroyBehaviours = new IActOnAttemptDestroy[] { };

    private void OnDestroy()
    {
        VegetationSys.DeregisterPlant(this);
    }

    private void Start()
    {
        _actOnDayPassingBehaviours = GetComponents<IActOnDayPassing>();
        _actOnAttemptDestroyBehaviours = GetComponents<IActOnAttemptDestroy>();
    }

    public void OnDayPassed()
    {
        foreach(IActOnDayPassing actingBehaviour in _actOnDayPassingBehaviours)
            actingBehaviour.OnDayPassed();
    }

    public void OnAttemptDestroy(Vector3Int pos)
    {
        foreach (IActOnAttemptDestroy actingBehaviour in _actOnAttemptDestroyBehaviours)
            actingBehaviour.OnAttemptDestroy(pos);
    }
}
