using UnityEngine;
using VegetationStates;
using System.Collections.Generic;

public class TestPlant : MonoBehaviour, IActOnDayPassing
{
    [SerializeField] [Range(0.001f, 1f)] private float _dailySeedGrowthChance = 0.01f;
    [SerializeField] private uint _averageSeedSurvivalDays = 17;
    [SerializeField] private uint _maxSeedSurvivalVariation = 12;
    [SerializeField] private GameObject _leafModel = null;

    private List<TestPlantLeaf> _testPlantSystem = new List<TestPlantLeaf>();
    private List<TestPlantLeaf> _removables = new List<TestPlantLeaf>();
    private List<TestPlantLeaf> _removeBuffer = new List<TestPlantLeaf>();

    public int GetLeafCount() { return _testPlantSystem.Count; }
    public GameObject GetLeafModel() { return _leafModel; }

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
        _testPlantSystem.Add(new TestPlantLeaf(transform.position, _dailySeedGrowthChance, _averageSeedSurvivalDays, _maxSeedSurvivalVariation, this, true));
    }

    public void AddTestPlantToSystem(Vector3 pos, Vector3 creatorPos)
    {
        _testPlantSystem.Add(new TestPlantLeaf(pos, _dailySeedGrowthChance, _averageSeedSurvivalDays, _maxSeedSurvivalVariation, this, false));
    }

    public void OnDayPassed()
    {
        for (int i = 0, size = _testPlantSystem.Count; i < size; ++i)
            _testPlantSystem[i].Update();

        foreach(TestPlantLeaf leaf in _removables)
            _testPlantSystem.Remove(leaf);

        _removables = _removeBuffer;
        _removeBuffer.Clear();
    }

    public void DeregisterLeaf(TestPlantLeaf leaf, bool freePosition)
    {
        _removeBuffer.Add(leaf);

        Plant plant = GetComponent<Plant>();
        if(freePosition) plant.VegetationSys.RemoveOccupationAt(leaf.Position);

        if (_testPlantSystem.Count == _removeBuffer.Count)
        {
            plant.VegetationSys.RemoveOccupationsBy(plant);
            Destroy(transform.parent.gameObject);
        }
    }
}

public class TestPlantLeaf
{
    private TestPlantLeaf() { }

    public TestPlantLeaf(Vector3 position, float dailySeedGrowthChance, uint averageSeedSurvivalDays, uint maxSeedSurvivalVariation, TestPlant TestPlant, float shadowAtPos, bool firstLeaf)
    {
        ShadowAtLocation = shadowAtPos;
        Position = position;

        GameObject model = null;
        TestPlantSeedState seedState = new TestPlantSeedState(model, dailySeedGrowthChance, Random.Range((int)(averageSeedSurvivalDays - maxSeedSurvivalVariation), (int)(averageSeedSurvivalDays + maxSeedSurvivalVariation + 1)), this, firstLeaf);
        TestPlantGrowingState growingState = new TestPlantGrowingState(this);
        TestPlantFullyGrownState fullyGrownState = new TestPlantFullyGrownState(this);
        TestPlantDormantState dormantState = new TestPlantDormantState(this);

        StateTransition seedToGrowing = new StateTransition
            ((originObject, originState)
            =>
            {
                if (!(originState as TestPlantSeedState).WillGrow) return StateTransitionResult.NO_ACTION;
                return StateTransitionResult.STACK_SWAP;
            });
        seedToGrowing.TargetState = growingState;
        seedState.Transitions.Add(seedToGrowing);

        StateTransition growingToGrown = new StateTransition
            ((originObject, originState)
            =>
            {
                if (!(originState as TestPlantGrowingState).HasReachedTarget()) return StateTransitionResult.NO_ACTION;
                return StateTransitionResult.STACK_SWAP;
            });
        growingToGrown.TargetState = fullyGrownState;
        growingState.Transitions.Add(growingToGrown);

        StateTransition toDormant = new StateTransition
            ((originObject, originState)
            =>
            {
                if (SeasonChanger.Instance.GetSeason() == Season.Autumn) return StateTransitionResult.NO_ACTION;
                return StateTransitionResult.STACK_PUSH;
            });
        toDormant.TargetState = dormantState;
        growingState.Transitions.Add(toDormant);
        fullyGrownState.Transitions.Add(toDormant);
        seedState.Transitions.Add(toDormant);

        StateTransition leaveDormancy = new StateTransition
            ((originObject, originState)
            =>
            {
                if (SeasonChanger.Instance.GetSeason() != Season.Autumn) return StateTransitionResult.NO_ACTION;
                return StateTransitionResult.STACK_POP;
            });
        leaveDormancy.TargetState = null;
        dormantState.Transitions.Add(leaveDormancy);


        _stateMachine = new StateMachine(TestPlant, seedState);
    }

    public TestPlantLeaf(Vector3 position, float dailySeedGrowthChance, uint averageSeedSurvivalDays, uint maxSeedSurvivalVariation, TestPlant TestPlant, bool firstLeaf)
        : this(position, dailySeedGrowthChance, averageSeedSurvivalDays, maxSeedSurvivalVariation, TestPlant, TestPlant.gameObject.GetComponent<Plant>().ShadowFactor, firstLeaf)
    {}

    public void Update()
    {
        _stateMachine.Update();
    }

    public Vector3 Position { get; private set; } = Vector3.zero;
    public float ShadowAtLocation { get; private set; } = 0f;
    public GameObject CurrentModel { get; set; } = null;

    private StateMachine _stateMachine = null;
}

namespace VegetationStates
{
    public class TestPlantSeedState : State
    {
        public bool WillGrow { get; private set; } = false;
        private float _spawnOddsPerDay = 0f;
        private int _survivalDaysLeft = 0;
        private GameObject _model = null;
        private TestPlantLeaf _leaf = null;
        private bool _firstSeed = true;

        private TestPlantSeedState() { }
        public TestPlantSeedState(GameObject model, float spawnOddsPerDay, int maxSurvivalDays, TestPlantLeaf leaf, bool firstSeed)
        {
            _model = model;
            _spawnOddsPerDay = spawnOddsPerDay;
            _survivalDaysLeft = maxSurvivalDays;
            _leaf = leaf;
            _firstSeed = firstSeed;
        }

        public override void Enter(MonoBehaviour origin)
        {
            _leaf.CurrentModel = _model;
        }

        public override void Exit(MonoBehaviour origin)
        {
            Object.Destroy(_model);
        }

        public override void Update(MonoBehaviour origin)
        {
            WillGrow = WillGrow || (Random.value <= _spawnOddsPerDay);
            if (WillGrow)
            {
                Vector3Int dir = Vector3Int.zero;
                TestPlant originPlant = (origin as TestPlant);
                Plant plant = originPlant.GetComponent<Plant>();
                Plant hit = plant.VegetationSys.GetOccupationNear(_leaf.Position, ref dir);

                if (hit)
                {
                    if (hit.GetComponent<TestPlant>() != null)
                    {
                        originPlant.DeregisterLeaf(_leaf, false);
                        Object.Destroy(_model);
                        return;
                    }
                    hit.OnAttemptDestroy(dir);
                }                
                if (!plant.VegetationSys.AttemptOccupy(origin.transform.position, plant))
                {
                    originPlant.DeregisterLeaf(_leaf, false);
                    Object.Destroy(_model);
                }
                return;
            }

            if (--_survivalDaysLeft == 0)
            {
                (origin as TestPlant).DeregisterLeaf(_leaf, false);
                Object.Destroy(_model);
            }
        }
    }

    public class TestPlantGrowingState : State
    {
        private TestPlantGrowingState() { }
        public TestPlantGrowingState(TestPlantLeaf leaf)
        {
            _leaf = leaf;
        }

        public bool HasReachedTarget() { return _absorbsUntilFullyGrown == 0; }
        private int _daysUntilAbsorb = Random.Range(0, 92);
        private int _absorbsUntilFullyGrown = 4;
        private TestPlantLeaf _leaf = null;
        private Mesh _mesh = null;
        private Color32[] _colours = null;

        public override void Enter(MonoBehaviour origin)
        {
            TestPlant TestPlant = origin as TestPlant;
            _leaf.CurrentModel = Object.Instantiate(TestPlant.GetLeafModel(), _leaf.Position, TestPlant.GetLeafModel().transform.rotation, TestPlant.transform);
            _mesh = _leaf.CurrentModel.GetComponentInChildren<MeshFilter>().mesh;
            Vector3[] vertices = _mesh.vertices;
            _colours = new Color32[vertices.Length];

            Color32 colour = new Color32(255, 255, 255, 255);
            for (int i = 0, length = vertices.Length; i < length; ++i)
                _colours[i] = colour;
            _mesh.colors32 = _colours;

            Vector3 rotation = _leaf.CurrentModel.transform.localEulerAngles;
            rotation.y = Random.Range(0f, 359.9999f);
            _leaf.CurrentModel.transform.localEulerAngles = rotation;
        }

        public override void Exit(MonoBehaviour origin)
        {
        }

        public override void Update(MonoBehaviour origin)
        {
            if (--_daysUntilAbsorb == 0)
            {
                _daysUntilAbsorb = Random.Range(0, 92);
                Vector3Int dir = new Vector3Int(Random.Range(-1, 2), 0, Random.Range(-1, 2));
                Plant hit = (origin as TestPlant).GetComponent<Plant>().VegetationSys.GetOccupationNear(_leaf.Position, ref dir);

                if (hit)
                {
                    if (hit.GetComponent<TestPlant>() != null) return;
                    hit.OnAttemptDestroy(dir);
                    --_absorbsUntilFullyGrown;

                    Vector3[] vertices = _mesh.vertices;
                    Color32 colour = new Color32(0, 0, 0, 0);

                    for (int i = 0, length = vertices.Length / 4, startPos = (3 - _absorbsUntilFullyGrown) * vertices.Length / 4; i < length; ++i)
                        _colours[i + startPos] = colour;
                    _mesh.colors32 = _colours;
                }
            }
        }
    }

    public class TestPlantFullyGrownState : State
    {
        private TestPlantLeaf _leaf = null;
        private float _chanceToSpawn = 0.10f;
        private float _distanceSpawned = 0.5f;
        private int _amountSpawned = 100;

        public TestPlantFullyGrownState(TestPlantLeaf leaf) { _leaf = leaf; }

        public override void Enter(MonoBehaviour origin) { }

        public override void Exit(MonoBehaviour origin) { }

        public override void Update(MonoBehaviour origin)
        {
            if (Random.Range(0f, 1f) <= _chanceToSpawn)
            {
                Vector3 targetPos = _leaf.Position;
                float randomAngle = Random.Range(0f, Mathf.PI * 2f);
                targetPos += new Vector3(Mathf.Cos(randomAngle) * _distanceSpawned, 0f, Mathf.Sin(randomAngle) * _distanceSpawned);
                TestPlant plant = origin as TestPlant;
                plant.AddTestPlantToSystem(targetPos, _leaf.Position);

                if (--_amountSpawned == 0)
                    plant.DeregisterLeaf(_leaf, true);
            }
        }
    }

    public class TestPlantDormantState : State
    {
        private TestPlantDormantState() { }
        public TestPlantDormantState(TestPlantLeaf leaf)
        {
        }

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
}