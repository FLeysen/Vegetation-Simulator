using UnityEngine;

namespace VegetationStates
{
    public class GenericSeedState : State
    {
        public bool WillGrow { get; private set; } = false;
        private float _spawnOddsPerDay = 0f;
        private int _survivalDaysLeft = 0;

        private GenericSeedState() { }
        public GenericSeedState(float spawnOddsPerDay, int maxSurvivalDays)
        {
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

    public class GenericGrowingState : State
    {
        public bool HasReachedTarget() { return _elapsedDays >= _daysToReachTarget; }

        //TODO: Remove this test segment, should respond to light etc.
        private int _daysToReachTarget = 30;
        private int _elapsedDays = 0;
        private Vector3 _initialScale = new Vector3(0.1f, 0.1f, 0.1f);
        private Vector3 _targetScaleMultiplier = new Vector3(2f, 4f, 2f);
        private GameObject _model = null;

        public override void Enter(MonoBehaviour origin)
        {
            _model = origin.gameObject.transform.parent.Find("GrowingModel").gameObject;
            _model.SetActive(true);
            _initialScale = _model.transform.parent.localScale;
            _targetScaleMultiplier = new Vector3(_initialScale.x * _targetScaleMultiplier.x, _initialScale.y * _targetScaleMultiplier.y, _initialScale.z * _targetScaleMultiplier.z);
        }

        public override void Exit(MonoBehaviour origin)
        {
        }

        public override void Update(MonoBehaviour origin)
        {
            _model.transform.parent.localScale = Vector3.Lerp(_initialScale, _targetScaleMultiplier, (float)(++_elapsedDays) / _daysToReachTarget);
        }
    }

    public class GenericFullyGrownState : State
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
}