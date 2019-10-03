using UnityEngine;

public class ManualTimePassing : MonoBehaviour
{
    [SerializeField] private KeyCode _passSingleDayButton = KeyCode.Alpha1;
    [SerializeField] private KeyCode _passDayCountButton = KeyCode.Alpha2;
    [SerializeField] private int _passAmount = 5;
    [SerializeField] private KeyCode _passAltDayCountButton = KeyCode.Alpha3;
    [SerializeField] private int _altPassAmount = 10;

    private TrackAndPassTime _time = null;

    void Start()
    {
        _time = GetComponent<TrackAndPassTime>();
    }

    void Update()
    {
        if (Input.GetKeyDown(_passSingleDayButton)) _time.PassDay();
        if (Input.GetKeyDown(_passDayCountButton))
        {
            for (int i = 0; i < _passAmount; ++i)
                _time.PassDay();
        }
        if (Input.GetKeyDown(_passAltDayCountButton))
        {
            for (int i = 0; i < _altPassAmount; ++i)
                _time.PassDay();
        }
    }
}
