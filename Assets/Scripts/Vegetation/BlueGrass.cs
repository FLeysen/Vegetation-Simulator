using UnityEngine;
using VegetationStates;
using System.Collections.Generic;

public class BlueGrass : MonoBehaviour, IActOnDayPassing
{
    [SerializeField] [Range(0.001f, 1f)] private float _dailySeedGrowthChance = 0.01f;
    [SerializeField] private uint _averageSeedSurvivalDays = 17;
    [SerializeField] private uint _maxSeedSurvivalVariation = 12;
    [SerializeField] private GameObject _leafModel = null;

    private List<BlueGrassLeaf> _BlueGrassSystem = new List<BlueGrassLeaf>();
    private List<BlueGrassLeaf> _removables = new List<BlueGrassLeaf>();
    private List<BlueGrassLeaf> _removeBuffer = new List<BlueGrassLeaf>();

    public int GetLeafCount() { return _BlueGrassSystem.Count; }
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
        _BlueGrassSystem.Add(new BlueGrassLeaf(transform.position, _dailySeedGrowthChance, _averageSeedSurvivalDays, _maxSeedSurvivalVariation, this, true));
    }

    public void AddBlueGrassToSystem(Vector3 pos, Vector3 creatorPos)
    {
        Plant plant = GetComponent<Plant>();
        if (plant.VegetationSys.AttemptHardOccupy(ref pos, plant, out float shadowFactor))
        {
            _BlueGrassSystem.Add(new BlueGrassLeaf(pos, _dailySeedGrowthChance, _averageSeedSurvivalDays, _maxSeedSurvivalVariation, this, shadowFactor, false));
        }
    }

    public void OnDayPassed()
    {
        for (int i = 0, size = _BlueGrassSystem.Count; i < size; ++i)
            _BlueGrassSystem[i].Update();

        foreach (BlueGrassLeaf leaf in _removables)
            _BlueGrassSystem.Remove(leaf);

        _removables = _removeBuffer;
        _removeBuffer.Clear();
    }

    public void DeregisterLeaf(BlueGrassLeaf leaf)
    {
        _removeBuffer.Add(leaf);

        Plant plant = GetComponent<Plant>();
        plant.VegetationSys.RemoveOccupationAt(leaf.Position);

        if (_BlueGrassSystem.Count == _removeBuffer.Count)
        {
            plant.VegetationSys.RemoveOccupationsBy(plant);
            Destroy(transform.parent.gameObject);
        }
    }
}

public class BlueGrassLeaf
{
    private BlueGrassLeaf() { }

    public BlueGrassLeaf(Vector3 position, float dailySeedGrowthChance, uint averageSeedSurvivalDays, uint maxSeedSurvivalVariation, BlueGrass BlueGrass, float shadowAtPos, bool firstLeaf)
    {
        ShadowAtLocation = shadowAtPos;
        Position = position;

        GameObject model = null;
        BlueGrassSeedState seedState = new BlueGrassSeedState(model, dailySeedGrowthChance, Random.Range((int)(averageSeedSurvivalDays - maxSeedSurvivalVariation), (int)(averageSeedSurvivalDays + maxSeedSurvivalVariation + 1)), this, firstLeaf);
        BlueGrassGrowingState growingState = new BlueGrassGrowingState(this);
        BlueGrassFullyGrownState fullyGrownState = new BlueGrassFullyGrownState(this);
        BlueGrassDormantState dormantState = new BlueGrassDormantState(this);

        StateTransition seedToGrowing = new StateTransition
            ((originObject, originState)
            =>
            {
                if (!(originState as BlueGrassSeedState).WillGrow) return StateTransitionResult.NO_ACTION;
                return StateTransitionResult.STACK_SWAP;
            });
        seedToGrowing.TargetState = growingState;
        seedState.Transitions.Add(seedToGrowing);

        StateTransition growingToGrown = new StateTransition
            ((originObject, originState)
            =>
            {
                if (!(originState as BlueGrassGrowingState).HasReachedTarget()) return StateTransitionResult.NO_ACTION;
                return StateTransitionResult.STACK_SWAP;
            });
        growingToGrown.TargetState = fullyGrownState;
        growingState.Transitions.Add(growingToGrown);

        StateTransition toDormant = new StateTransition
            ((originObject, originState)
            =>
            {
                if (SeasonChanger.Instance.GetSeason() != Season.Winter) return StateTransitionResult.NO_ACTION;
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
                if (SeasonChanger.Instance.GetSeason() == Season.Winter) return StateTransitionResult.NO_ACTION;
                return StateTransitionResult.STACK_POP;
            });
        leaveDormancy.TargetState = null;
        dormantState.Transitions.Add(leaveDormancy);


        _stateMachine = new StateMachine(BlueGrass, seedState);
    }

    public BlueGrassLeaf(Vector3 position, float dailySeedGrowthChance, uint averageSeedSurvivalDays, uint maxSeedSurvivalVariation, BlueGrass BlueGrass, bool firstLeaf)
        : this(position, dailySeedGrowthChance, averageSeedSurvivalDays, maxSeedSurvivalVariation, BlueGrass, BlueGrass.gameObject.GetComponent<Plant>().ShadowFactor, firstLeaf)
    { }

    public void Update()
    {
        _stateMachine.Update();
    }

    public void AddConnection(BlueGrassConnection connection)
    {
        _connections.Add(connection);
    }

    public Vector3 Position { get; private set; } = Vector3.zero;
    public float ShadowAtLocation { get; private set; } = 0f;
    public GameObject CurrentModel { get; set; } = null;

    private List<BlueGrassConnection> _connections = new List<BlueGrassConnection>();
    private StateMachine _stateMachine = null;
}

public class BlueGrassConnection
{
    private BlueGrassConnection() { }
    public BlueGrassConnection(BlueGrassLeaf initiator, BlueGrassLeaf receiver)
    {
        initiator.AddConnection(this);
        receiver.AddConnection(this);
    }
}

namespace VegetationStates
{
    public class BlueGrassSeedState : State
    {
        public bool WillGrow { get; private set; } = false;
        private float _spawnOddsPerDay = 0f;
        private int _survivalDaysLeft = 0;
        private GameObject _model = null;
        private BlueGrassLeaf _leaf = null;
        private bool _firstSeed = true;

        private BlueGrassSeedState() { }
        public BlueGrassSeedState(GameObject model, float spawnOddsPerDay, int maxSurvivalDays, BlueGrassLeaf leaf, bool firstSeed)
        {
            _model = model;
            _spawnOddsPerDay = spawnOddsPerDay * Mathf.Clamp(leaf.ShadowAtLocation * 0.9f, 0.25f, 1f);
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
                if (!_firstSeed) return;

                Plant plant = origin.GetComponent<Plant>();
                if (!plant.VegetationSys.AttemptOccupy(origin.transform.position, plant))
                {
                    (origin as BlueGrass).DeregisterLeaf(_leaf);
                    Object.Destroy(_model);
                }
                return;
            }

            if (--_survivalDaysLeft == 0)
            {
                (origin as BlueGrass).DeregisterLeaf(_leaf);
                Object.Destroy(_model);
            }
        }
    }

    public class BlueGrassGrowingState : State
    {
        private BlueGrassGrowingState() { }
        public BlueGrassGrowingState(BlueGrassLeaf leaf)
        {
            _leaf = leaf;
            _chanceToSpawn *= leaf.ShadowAtLocation * 0.9f * 0.5f + 0.5f;
        }

        public bool HasReachedTarget() { return _leaf.CurrentModel.transform.localScale.y > _maxScale; }
        private float _chanceToSpawn = 0.12f;
        private float _maxScale = Random.Range(0.9f, 1.1f);
        private float _distanceSpawned = 0.5f;
        private Vector3 _initialScale = new Vector3(0.1f, 0.1f, 0.1f);
        private Vector3 _amountAddedPerDay = new Vector3(0.000f, 0.0025f, 0.000f);
        private BlueGrassLeaf _leaf = null;

        public override void Enter(MonoBehaviour origin)
        {
            BlueGrass BlueGrass = origin as BlueGrass;
            _leaf.CurrentModel = Object.Instantiate(BlueGrass.GetLeafModel(), _leaf.Position, BlueGrass.GetLeafModel().transform.rotation, BlueGrass.transform);
            _initialScale = _leaf.CurrentModel.transform.localScale;

            Mesh mesh = _leaf.CurrentModel.GetComponentInChildren<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            Color32[] colours = new Color32[vertices.Length];

            for (int i = 0, length = vertices.Length; i < length; i += 10)
            {
                byte value = (byte)((((float)i / length) > _leaf.ShadowAtLocation) ? 255 : 0);
                Color32 colour = new Color32(value, value, value, value);

                for (int j = 0; j < 10; ++j)
                    colours[i + j] = colour;
            }
            mesh.colors32 = colours;

            Vector3 rotation = _leaf.CurrentModel.transform.localEulerAngles;
            rotation.y = Random.Range(0f, 359.9999f);
            _leaf.CurrentModel.transform.localEulerAngles = rotation;
        }

        public override void Exit(MonoBehaviour origin)
        {
        }

        public override void Update(MonoBehaviour origin)
        {
            _leaf.CurrentModel.transform.localScale += _amountAddedPerDay;

            if (Random.Range(0f, 1f) <= _chanceToSpawn)
            {
                Vector3 targetPos = _leaf.Position;
                float randomAngle = Random.Range(0f, Mathf.PI * 2f);
                targetPos += new Vector3(Mathf.Cos(randomAngle) * _distanceSpawned, 0f, Mathf.Sin(randomAngle) * _distanceSpawned);
                targetPos.y += 2.5f * Random.Range(-1f, 1f);
                (origin as BlueGrass).AddBlueGrassToSystem(targetPos, _leaf.Position);
            }
        }
    }

    public class BlueGrassFullyGrownState : State
    {
        private BlueGrassLeaf _leaf = null;
        private float _chanceToSpawn = 0.12f;
        private float _distanceSpawned = 0.5f;

        public BlueGrassFullyGrownState(BlueGrassLeaf leaf) { _leaf = leaf; }

        public override void Enter(MonoBehaviour origin) { }

        public override void Exit(MonoBehaviour origin) { }

        public override void Update(MonoBehaviour origin)
        {
            if (Random.Range(0f, 1f) <= _chanceToSpawn)
            {
                Vector3 targetPos = _leaf.Position;
                float randomAngle = Random.Range(0f, Mathf.PI * 2f);
                targetPos += new Vector3(Mathf.Cos(randomAngle) * _distanceSpawned, 0f, Mathf.Sin(randomAngle) * _distanceSpawned);
                (origin as BlueGrass).AddBlueGrassToSystem(targetPos, _leaf.Position);
            }
        }
    }

    public class BlueGrassDormantState : State
    {
        private float _chanceToWither = 0.001f;
        private BlueGrassLeaf _leaf = null;

        private BlueGrassDormantState() { }
        public BlueGrassDormantState(BlueGrassLeaf leaf)
        {
            _leaf = leaf;
        }

        public override void Enter(MonoBehaviour origin)
        {
        }

        public override void Exit(MonoBehaviour origin)
        {

        }

        public override void Update(MonoBehaviour origin)
        {
            if (Random.Range(0f, 1f) <= _chanceToWither)
            {
                (origin as BlueGrass).DeregisterLeaf(_leaf);
                Object.Destroy(_leaf.CurrentModel);
            }
        }
    }
}