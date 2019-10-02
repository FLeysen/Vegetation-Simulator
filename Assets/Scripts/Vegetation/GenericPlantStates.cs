using UnityEngine;

public class GenericPlantStates : MonoBehaviour, IActOnDayPassing
{
    private StateMachine _stateMachine = null;

    private void Start()
    {
        SeedState seedState = new SeedState(0.01f, 5);
        GrowingState growingState = new GrowingState();
        FullyGrownState fullyGrownState = new FullyGrownState();

        StateTransition seedToGrowing = new StateTransition
            ((originObject, originState)
            =>
            {
                if (!(originState as SeedState).WillGrow) return StateTransitionResult.NO_ACTION;
                return StateTransitionResult.STACK_SWAP;
            });
        seedToGrowing.TargetState = growingState;
        seedState.Transitions.Add(seedToGrowing);

        _stateMachine = new StateMachine(this, seedState);
    }

    public void OnDayPassed()
    {
        _stateMachine.Update(); //We don't need to update every frame, just every day
    }
}

public class SeedState : State
{
    public bool WillGrow { get; private set; } = false;
    private float _spawnOddsPerDay = 0f;
    private int _survivalDaysLeft = 0;

    private SeedState() { }
    public SeedState(float spawnOddsPerDay, int maxSurvivalDays)
    {
        _spawnOddsPerDay = spawnOddsPerDay;
        _survivalDaysLeft = maxSurvivalDays;
    }

    public override void Enter(MonoBehaviour origin)
    {}

    public override void Exit(MonoBehaviour origin)
    {
        origin.gameObject.transform.parent.Find("SeedModel").gameObject.SetActive(false);
    }

    public override void Update(MonoBehaviour origin)
    {
        if (WillGrow = Random.Range(0f, 1f) <= _spawnOddsPerDay)
            return;

        if (--_survivalDaysLeft == 0)
            Object.Destroy(origin.gameObject);
    }
}

public class GrowingState : State
{
    public override void Enter(MonoBehaviour origin)
    {
        origin.gameObject.transform.parent.Find("GrowingModel").gameObject.SetActive(true);
    }

    public override void Exit(MonoBehaviour origin)
    {
    }

    public override void Update(MonoBehaviour origin)
    {
    }
}

public class FullyGrownState : State
{
    public override void Enter(MonoBehaviour origin)
    {
    }

    public override void Exit(MonoBehaviour origin)
    {
    }

    public override void Update(MonoBehaviour origin)
    {
    }
}