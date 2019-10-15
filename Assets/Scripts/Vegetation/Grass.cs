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
        _grassSystem.Add(new GrassLeaf(transform.position, _dailySeedGrowthChance, _averageSeedSurvivalDays, _maxSeedSurvivalVariation, this));
    }

    public void AddGrassToSystem(Vector3 pos, Vector3 creatorPos)
    {
        Plant plant = GetComponent<Plant>();
        if(GetComponent<Plant>().VegetationSys.AttemptOccupy(ref pos, plant, creatorPos, out float shadowFactor))
        {
            _grassSystem.Add(new GrassLeaf(pos, _dailySeedGrowthChance, _averageSeedSurvivalDays, _maxSeedSurvivalVariation, this, shadowFactor));
        }
    }

    public void OnDayPassed()
    {
        for (int i = 0, size = _grassSystem.Count; i < size; ++i)
            _grassSystem[i].Update();
    }
}

public class GrassLeaf
{
    private GrassLeaf() { }

    public GrassLeaf(Vector3 position, float dailySeedGrowthChance, uint averageSeedSurvivalDays, uint maxSeedSurvivalVariation, Grass grass, float shadowAtPos)
    {
        ShadowAtLocation = shadowAtPos;
        Position = position;
        Obj = grass.gameObject;

        GameObject model = Object.Instantiate(grass.GetSeedModel(), position, grass.transform.rotation, grass.transform);
        GrassSeedState seedState = new GrassSeedState(model, dailySeedGrowthChance, Random.Range((int)(averageSeedSurvivalDays - maxSeedSurvivalVariation), (int)(averageSeedSurvivalDays + maxSeedSurvivalVariation + 1)));
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

    public GrassLeaf(Vector3 position, float dailySeedGrowthChance, uint averageSeedSurvivalDays, uint maxSeedSurvivalVariation, Grass grass)
        : this(position, dailySeedGrowthChance, averageSeedSurvivalDays, maxSeedSurvivalVariation, grass, grass.gameObject.GetComponent<Plant>().ShadowFactor)
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

        private GrassSeedState() { }
        public GrassSeedState(GameObject model, float spawnOddsPerDay, int maxSurvivalDays)
        {
            _model = model;
            _spawnOddsPerDay = spawnOddsPerDay;
            _survivalDaysLeft = maxSurvivalDays;
        }

        public override void Enter(MonoBehaviour origin)
        { }

        public override void Exit(MonoBehaviour origin)
        {
            Plant plant = origin.GetComponent<Plant>();
            if (!plant.VegetationSys.AttemptOccupy(origin.transform.position, plant))
            {
                if ((origin as Grass).GetLeafCount() == 1)
                    Object.Destroy(origin.transform.parent.gameObject);
                else
                    Object.Destroy(_model);
                return;
            }

            Object.Destroy(_model);
        }

        public override void Update(MonoBehaviour origin)
        {
            if (WillGrow = Random.Range(0f, 1f) <= _spawnOddsPerDay)
                return;

            if (--_survivalDaysLeft == 0)
            {
                if ((origin as Grass).GetLeafCount() == 1)
                    Object.Destroy(origin.transform.parent.gameObject);
                else
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
            //TODO: Replace later
            //Object.Destroy(_model);
        }

        public override void Update(MonoBehaviour origin)
        {
            _model.transform.localScale = Vector3.Lerp(_initialScale, _targetScaleMultiplier, (float)(++_elapsedDays) / _daysToReachTarget);
        }
    }

    public class GrassFullyGrownState : State
    {
        private float _distanceSpawned = 0.5f;
        private float _chanceToSpawn = 0.01f;
        private GrassLeaf _leaf = null;

        //TODO: Remove this test segment, should respond to light etc.
        private int _daysToLive = 50;
        private GameObject _model = null;


        private GrassFullyGrownState() { }
        public GrassFullyGrownState(GrassLeaf leaf)
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
            if (Random.Range(0f, 1f) <= _chanceToSpawn)
            {
                Vector3 targetPos = _leaf.Position;
                float randomAngle = Random.Range(0f, Mathf.PI * 2f);
                targetPos += new Vector3(Mathf.Cos(randomAngle) * _distanceSpawned, 0f, Mathf.Sin(randomAngle) * _distanceSpawned);
                (origin as Grass).AddGrassToSystem(targetPos, _leaf.Position);
            }

            if (--_daysToLive == 0)
            {
                if ((origin as Grass).GetLeafCount() == 1)
                    Object.Destroy(origin.transform.parent.gameObject);
                else
                    Object.Destroy(_model);
            }
        }
    }
}