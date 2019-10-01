using UnityEngine;

public class TrackAndPassTime : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Text _daysPassedText = null;

    public int ElapsedDays { get; private set; } = 0;

    public delegate void OnPassDayDelegate();
    public OnPassDayDelegate OnPassDay;

    private void Start()
    {
        OnPassDay += () => { ++ElapsedDays; _daysPassedText.text = ElapsedDays.ToString(); };
    }

    public void PassDay()
    {
        OnPassDay();
    }
}
