using UnityEngine;
using VegetationStates;
using System.Collections.Generic;

public class Grass : MonoBehaviour, IActOnDayPassing
{
    [SerializeField] [Range(0.001f, 1f)] private float _dailySeedGrowthChance = 0.01f;
    [SerializeField] private uint _averageSeedSurvivalDays = 17;
    [SerializeField] private uint _maxSeedSurvivalVariation = 12;
    [SerializeField] private GameObject _seedModel = null;
    [SerializeField] private GameObject _leafModel = null;

    private List<GrassLeaf> _grassSystem = new List<GrassLeaf>();
    private List<GrassLeaf> _removables = new List<GrassLeaf>();
    private List<GrassLeaf> _removeBuffer = new List<GrassLeaf>();

    public int GetLeafCount() { return _grassSystem.Count; }
    public GameObject GetSeedModel() { return _seedModel; }
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
        _grassSystem.Add(new GrassLeaf(transform.position, _dailySeedGrowthChance, _averageSeedSurvivalDays, _maxSeedSurvivalVariation, this, true));
    }

    public void AddGrassToSystem(Vector3 pos, Vector3 creatorPos)
    {
        Plant plant = GetComponent<Plant>();
        if(plant.VegetationSys.AttemptOccupy(ref pos, plant, creatorPos, out float shadowFactor))
        {
            _grassSystem.Add(new GrassLeaf(pos, _dailySeedGrowthChance, _averageSeedSurvivalDays, _maxSeedSurvivalVariation, this, shadowFactor, false));
        }
    }

    public void OnDayPassed()
    {
        for (int i = 0, size = _grassSystem.Count; i < size; ++i)
            _grassSystem[i].Update();

        foreach(GrassLeaf leaf in _removables)
            _grassSystem.Remove(leaf);

        _removables = _removeBuffer;
        _removeBuffer.Clear();
    }

    public void DeregisterLeaf(GrassLeaf leaf)
    {
        _removeBuffer.Add(leaf);

        if (_grassSystem.Count == _removeBuffer.Count)
        {
            Plant plant = GetComponent<Plant>();
            plant.VegetationSys.RemoveOccupationsBy(plant);
            Destroy(transform.parent.gameObject);
        }
    }
}

public class GrassLeaf
{
    private GrassLeaf() { }

    public GrassLeaf(Vector3 position, float dailySeedGrowthChance, uint averageSeedSurvivalDays, uint maxSeedSurvivalVariation, Grass grass, float shadowAtPos, bool firstLeaf)
    {
        ShadowAtLocation = shadowAtPos;
        Position = position;
        Obj = grass.gameObject;

        GameObject model = Object.Instantiate(grass.GetSeedModel(), position, grass.transform.rotation, grass.transform);
        GrassSeedState seedState = new GrassSeedState(model, dailySeedGrowthChance, Random.Range((int)(averageSeedSurvivalDays - maxSeedSurvivalVariation), (int)(averageSeedSurvivalDays + maxSeedSurvivalVariation + 1)), this, firstLeaf);
        GrassGrowingState growingState = new GrassGrowingState(this);
        GrassFullyGrownState fullyGrownState = new GrassFullyGrownState(this);

        StateTransition seedToGrowing = new StateTransition
            ((originObject, originState)
            =>
            {
                if (!(originState as GrassSeedState).WillGrow) return StateTransitionResult.NO_ACTION;
                return StateTransitionResult.STACK_SWAP;
            });
        seedToGrowing.TargetState = growingState;
        seedState.Transitions.Add(seedToGrowing);

        StateTransition growingToGrown = new StateTransition
            ((originObject, originState)
            =>
            {
                if (!(originState as GrassGrowingState).HasReachedTarget()) return StateTransitionResult.NO_ACTION;
                return StateTransitionResult.STACK_SWAP;
            });
        growingToGrown.TargetState = fullyGrownState;
        growingState.Transitions.Add(growingToGrown);

        _stateMachine = new StateMachine(grass, seedState);
    }

    public GrassLeaf(Vector3 position, float dailySeedGrowthChance, uint averageSeedSurvivalDays, uint maxSeedSurvivalVariation, Grass grass, bool firstLeaf)
        : this(position, dailySeedGrowthChance, averageSeedSurvivalDays, maxSeedSurvivalVariation, grass, grass.gameObject.GetComponent<Plant>().ShadowFactor, firstLeaf)
    {}

    private void Die()
    {
        //TODO: Wither animation?
        Obj.SetActive(false);
    }

    public void Update()
    {
        _stateMachine.Update();
    }
    
    public void AddConnection(GrassConnection connection)
    {
        _connections.Add(connection);
    }

    public Vector3 Position { get; private set; } = Vector3.zero;
    public float ShadowAtLocation { get; private set; } = 0f;
    public GameObject Obj { get; private set; } = null;

    private List<GrassConnection> _connections = new List<GrassConnection>();
    private StateMachine _stateMachine = null;
}

public class GrassConnection
{
    private GrassConnection() { }
    public GrassConnection(GrassLeaf initiator, GrassLeaf receiver)
    {
        initiator.AddConnection(this);
        receiver.AddConnection(this);
    }
}

namespace VegetationStates
{
    public class GrassSeedState : State
    {
        public bool WillGrow { get; private set; } = false;
        private float _spawnOddsPerDay = 0f;
        private int _survivalDaysLeft = 0;
        private GameObject _model = null;
        private GrassLeaf _leaf = null;
        private bool _firstSeed = true;

        private GrassSeedState() { }
        public GrassSeedState(GameObject model, float spawnOddsPerDay, int maxSurvivalDays, GrassLeaf leaf, bool firstSeed)
        {
            _model = model;
            _spawnOddsPerDay = spawnOddsPerDay;
            _survivalDaysLeft = maxSurvivalDays;
            _leaf = leaf;
            _firstSeed = firstSeed;
        }

        public override void Enter(MonoBehaviour origin)
        { }

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
                    (origin as Grass).DeregisterLeaf(_leaf);
                    Object.Destroy(_model);
                }
                return;
            }

            if (--_survivalDaysLeft == 0)
            {
                (origin as Grass).DeregisterLeaf(_leaf);
                Object.Destroy(_model);
            }
        }
    }

    public class GrassGrowingState : State
    {
        private GrassGrowingState() { }
        public GrassGrowingState(GrassLeaf leaf)
        {
            _leaf = leaf;
        }

        public bool HasReachedTarget() { return _elapsedDays >= _daysToReachTarget; }
        private GrassLeaf _leaf = null;

        //TODO: Remove this test segment, should respond to light etc.
        private int _daysToReachTarget = 30;
        private int _elapsedDays = 0;
        private Vector3 _initialScale = new Vector3(0.1f, 0.1f, 0.1f);
        private Vector3 _targetScaleMultiplier = new Vector3(2f, 2f, 2f);
        private GameObject _model = null;


        public override void Enter(MonoBehaviour origin)
        {
            Grass grass = origin as Grass;
            _model = Object.Instantiate(grass.GetLeafModel(), _leaf.Position, grass.GetLeafModel().transform.rotation, grass.transform);
            _initialScale = _model.transform.localScale;
            _targetScaleMultiplier = new Vector3(_initialScale.x * _targetScaleMultiplier.x, _initialScale.y * _targetScaleMultiplier.y, _initialScale.z * _targetScaleMultiplier.z);

            Renderer renderer = _model.GetComponent<Renderer>();
            Color col = renderer.material.color;
            col.r = _leaf.ShadowAtLocation;
            renderer.material.color = col;
        }

        public override void Exit(MonoBehaviour origin)
        {
            Object.Destroy(_model);
        }

        public override void Update(MonoBehaviour origin)
        {
            _model.transform.localScale = Vector3.Lerp(_initialScale, _targetScaleMultiplier, (float)(++_elapsedDays) / _daysToReachTarget);
        }
    }

    public class GrassFullyGrownState : State
    {
        private float _distanceSpawned = 0.5f;
        private float _chanceToSpawn = 0.02f;
        private GrassLeaf _leaf = null;

        //TODO: Remove this test segment, should respond to light etc.
        private int _daysToLive = Random.Range(450, 650);
        private GameObject _model = null;


        private GrassFullyGrownState() { }
        public GrassFullyGrownState(GrassLeaf leaf)
        {
            _leaf = leaf;
        }

        public override void Enter(MonoBehaviour origin)
        {
            Grass grass = origin as Grass;
            _model = Object.Instantiate(grass.GetLeafModel(), _leaf.Position, grass.GetLeafModel().transform.rotation, grass.transform);
            _model.transform.localScale = new Vector3(1f, 1f, 1f);
           
            Renderer renderer = _model.GetComponent<Renderer>();
            Color col = renderer.material.color;
            col.r = _leaf.ShadowAtLocation;
            renderer.material.color = col;
        }

        public override void Exit(MonoBehaviour origin)
        {

        }

        public override void Update(MonoBehaviour origin)
        {
            if (Random.Range(0f, 1f) <= _chanceToSpawn)
            {
                Vector3 targetPos = _leaf.Position;
                float randomAngle = Random.Range(0f, Mathf.PI * 2f);
                targetPos += new Vector3(Mathf.Cos(randomAngle) * _distanceSpawned, 0f, Mathf.Sin(randomAngle) * _distanceSpawned);
                (origin as Grass).AddGrassToSystem(targetPos, _leaf.Position);
            }

            if (--_daysToLive == 0)
            {
                (origin as Grass).DeregisterLeaf(_leaf);
                Object.Destroy(_model);
            }
        }
    }
}