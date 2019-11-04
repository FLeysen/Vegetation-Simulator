using UnityEngine;

namespace VegetationGenerator
{
    public class AnnualPlant : MonoBehaviour, IActOnDayPassing
    {
        [SerializeField] [Range(0.001f, 1f)] private float _dailySeedGrowthChance = 0.05f;
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

            GenericSeedState seedState = new GenericSeedState(_dailySeedGrowthChance, Random.Range((int)(_averageSeedSurvivalDays - _maxSeedSurvivalVariation), (int)(_averageSeedSurvivalDays + _maxSeedSurvivalVariation + 1)));
            GenericGrowingState growingState = new GenericGrowingState();
            GenericFullyGrownState fullyGrownState = new GenericFullyGrownState();

            StateTransition seedToGrowing = new StateTransition
                ((originObject, originState)
                =>
                {
                    if (!(originState as GenericSeedState).WillGrow) return StateTransitionResult.NO_ACTION;
                    return StateTransitionResult.STACK_SWAP;
                });
            seedToGrowing.TargetState = growingState;
            seedState.Transitions.Add(seedToGrowing);

            StateTransition growingToGrown = new StateTransition
                ((originObject, originState)
                =>
                {
                    if (!(originState as GenericGrowingState).HasReachedTarget()) return StateTransitionResult.NO_ACTION;
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
}

