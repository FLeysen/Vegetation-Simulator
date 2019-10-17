using UnityEngine;

public enum Season
{
    Winter,
    Spring,
    Summer,
    Autumn
}

public class SeasonChanger : Singleton<SeasonChanger>
{
    [SerializeField] private UnityEngine.UI.Text _seasonText = null;
    [SerializeField] private Season _season = Season.Spring;
    [SerializeField] private uint _daysInSpring = 91;
    [SerializeField] private uint _daysInSummer = 91;
    [SerializeField] private uint _daysInAutumn = 91;
    [SerializeField] private uint _daysInWinter = 91;
    private uint _elapsedDays = 0;
    private uint _daysInSeason = 91;

    public Season GetSeason() { return _season; }

    private void Start()
    {
        GetComponent<TrackAndPassTime>().OnPassDay += OnDayPassed;
        OnSeasonChange();
    }

    private void OnDayPassed()
    {
        if (++_elapsedDays == _daysInSeason)
        { 
            if (_season != (Season)3) //Always 4 seasons, but order might change in the future
                ++_season;
            else
                _season = 0;
            OnSeasonChange();
        }
    }

    private void OnSeasonChange()
    {
        _elapsedDays = 0;
        switch(_season)
        {
            case Season.Winter:
                _daysInSeason = _daysInWinter;
                break;
            case Season.Spring:
                _daysInSeason = _daysInSpring;
                break;
            case Season.Autumn:
                _daysInSeason = _daysInAutumn;
                break;
            case Season.Summer:
                _daysInSeason = _daysInSummer;
                break;
        }
        _seasonText.text = _season.ToString();
    }
}
