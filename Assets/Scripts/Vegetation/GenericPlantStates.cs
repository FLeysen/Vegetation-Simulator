using UnityEngine;

public class GenericPlantStates : MonoBehaviour, IActOnDayPassing
{
    [SerializeField][Range(0.001f, 1f)] private float _dailySeedGrowthChance = 0.05f;
    [SerializeField] private uint _averageSeedSurvivalDays = 15;
    [SerializeField] private uint _maxSeedSurvivalVariation = 5;

    private StateMachine _stateMachine = null;

    private void OnValidate()
    {
        if (_maxSeedSurvivalVariation >= _averageSeedSurvivalDays)
        {
            Debug.LogWarning("Situations where seed would survive less than 1 day are not allowed.");
            _averageSeedSurvivalDays += _maxSeedSurvivalVariation - _averageSeedSurvivalDays + 1;
        }
    }

    private void Start()
    {
        Plant plant = GetComponent<Plant>();

        SeedState seedState = new SeedState(_dailySeedGrowthChance, Random.Range((int)(_averageSeedSurvivalDays - _maxSeedSurvivalVariation), (int)(_averageSeedSurvivalDays + _maxSeedSurvivalVariation + 1)));
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

        StateTransition growingToGrown = new StateTransition
            ((originObject, originState)
            =>
            {
                if (!(originState as GrowingState).HasReachedTarget()) return StateTransitionResult.NO_ACTION;
                return StateTransitionResult.STACK_SWAP;
            });
        growingToGrown.TargetState = fullyGrownState;
        growingState.Transitions.Add(growingToGrown);

        _stateMachine = new StateMachine(this, seedState);
    }

    public void OnDayPassed()
    {
        _stateMachine.Update(); //We don't need to update every frame, just every passing day
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
        Plant plant = origin.GetComponent<Plant>();
        if (!plant.VegetationSys.AttemptOccupy(origin.transform.position, plant))
        {
            Object.Destroy(origin.transform.parent.gameObject);
            return;
        }

        origin.gameObject.transform.parent.Find("SeedModel").gameObject.SetActive(false);
    }

    public override void Update(MonoBehaviour origin)
    {
        if (WillGrow = Random.Range(0f, 1f) <= _spawnOddsPerDay)
            return;

        if (--_survivalDaysLeft == 0)
            Object.Destroy(origin.transform.parent.gameObject);
    }
}

public class GrowingState : State
{
    public bool HasReachedTarget(){ return _elapsedDays >= _daysToReachTarget; }

    //TODO: Remove this test segment, should respond to light etc.
    private int _daysToReachTarget = 30;
    private int _elapsedDays = 0;
    private Vector3 _initialScale = new Vector3(1f, 0.25f, 1f);
    private Vector3 _targetScale = new Vector3(2f, 1f, 2f);
    private GameObject _model = null;

    public override void Enter(MonoBehaviour origin)
    {
        _model = origin.gameObject.transform.parent.Find("GrowingModel").gameObject;
        _model.SetActive(true);
    }

    public override void Exit(MonoBehaviour origin)
    {
    }

    public override void Update(MonoBehaviour origin)
    {
        _model.transform.localScale = Vector3.Lerp(_initialScale, _targetScale, (float)(++_elapsedDays) / _daysToReachTarget);
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