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

        Bounds bounds = GetComponent<Collider>().bounds;
        float maxXDiff = bounds.extents.x;
        float maxZDiff = bounds.extents.z;
        float yPosition = -0.2f; //GetComponent<DetectSurfaces>().DebugCodeDeleteLater();
        Vector3 center = bounds.center;
        center.y = yPosition;

        Vector3 spawnPos = center;
        for (int i = 0; i < _plantPrefabLength; ++i)
        {
            for (int j = 0; j < _seedsBeforeStart[i]; ++j)
            {
                spawnPos.x += Random.Range(-maxXDiff, maxXDiff);
                spawnPos.z += Random.Range(-maxZDiff, maxZDiff);
                Plant plant = Instantiate(_possiblePlantPrefabs[i], spawnPos, transform.rotation).GetComponentInChildren<Plant>();
                _plants.Add(plant);
                plant.VegetationSys = this;

                spawnPos.x = center.x;
                spawnPos.z = center.z;
            }
        }
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
