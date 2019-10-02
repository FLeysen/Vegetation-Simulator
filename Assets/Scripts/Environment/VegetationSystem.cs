using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegetationSystem : MonoBehaviour
{
    [SerializeField] private GameObject[] _possiblePlantPrefabs = null;
    [SerializeField] private int[] _seedsBeforeStart = null;
    private List<Plant> _plants = new List<Plant>();

    private int _plantPrefabLength = 0;
    private int _seedArrayLength = 0;

    private void Start()
    {
        GetComponent<TrackAndPassTime>().OnPassDay += OnDayPassed;


    }

    public void DeregisterPlant(Plant plant)
    {
        _plants.Remove(plant);
    }

    private void OnDayPassed()
    {
        foreach(Plant plant in _plants)
        {
            plant.OnDayPassed();
        }
    }

    private void OnValidate()
    {
        if (_plantPrefabLength != _possiblePlantPrefabs.Length)
        {
            _plantPrefabLength = _possiblePlantPrefabs.Length;
            _seedArrayLength = _plantPrefabLength;
            System.Array.Resize(ref _seedsBeforeStart, _seedArrayLength);
        }
        else if (_seedArrayLength != _seedsBeforeStart.Length)
        {
            _seedArrayLength = _seedsBeforeStart.Length;
            _plantPrefabLength = _seedArrayLength;
            System.Array.Resize(ref _possiblePlantPrefabs, _plantPrefabLength);
        }
    }
}
