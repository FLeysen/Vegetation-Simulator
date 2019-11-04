using UnityEngine;

namespace VegetationGenerator
{

    public class AutomaticTimePassage : MonoBehaviour
    {
        [SerializeField] private KeyCode _pauseButton = KeyCode.Space;
        [SerializeField] private float _secondsPerDay = 3f;
        private float _elapsedToday = 0f;
        private bool _isPaused = true;

        private TrackAndPassTime _time = null;

        private void Start()
        {
            _time = GetComponent<TrackAndPassTime>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(_pauseButton))
                _isPaused = !_isPaused;

            if (!_isPaused)
            {
                _elapsedToday += Time.deltaTime;
                while (_elapsedToday > _secondsPerDay)
                {
                    _time.PassDay();
                    _elapsedToday -= _secondsPerDay;
                }
            }
        }

        public void ForcePause()
        {
            _isPaused = true;
        }
    }
}