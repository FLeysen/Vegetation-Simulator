using System.Collections.Generic;
using UnityEngine;

public class VegetationSystem : MonoBehaviour
{
    [SerializeField] private GameObject[] _possiblePlantPrefabs = null;
    [SerializeField] private int[] _seedsBeforeStart = null;
    private List<Plant> _plants = new List<Plant>() { };
    private Grid _grid = null;
    private Dictionary<Vector3Int, Plant> _gridOccupations = new Dictionary<Vector3Int, Plant>();
    private DetectSurfaces _surfaceDetector = null;

    private int _plantPrefabLength = 0;
    private int _seedArrayLength = 0;

    private void Start()
    {
        Invoke("SpawnVegetation", 0.12f);
    }

    private void SpawnVegetation()
    {
        _grid = GetComponent<Grid>();
        GetComponent<TrackAndPassTime>().OnPassDay += OnDayPassed;

        _surfaceDetector = GetComponent<DetectSurfaces>();

        Bounds bounds = GetComponent<Collider>().bounds;
        float maxXDiff = bounds.extents.x;
        float maxYDiff = bounds.extents.y;
        float maxZDiff = bounds.extents.z;
        Vector3 center = bounds.center;
        Collider nearestObject = null;

        Vector3 spawnPos = center;
        for (int i = 0; i < _plantPrefabLength; ++i)
        {
            for (int j = 0; j < _seedsBeforeStart[i]; ++j)
            {
                spawnPos.x += Random.Range(-maxXDiff, maxXDiff);
                spawnPos.z += Random.Range(-maxZDiff, maxZDiff);
                spawnPos.y += Random.Range(-maxYDiff, maxZDiff);
                nearestObject = _surfaceDetector.GetNearestSurfaceTo(spawnPos, bounds.size.y);
                spawnPos.y = bounds.max.y;


                Vector3 originalPos = spawnPos;

                for (float progress = 0.1f; progress < 1.01f; progress += 0.1f)
                {
                    Vector3 dir = Vector3.down;
                    float wiggleRoom = 0.01f;

                    if (spawnPos.y < bounds.center.y)
                    {
                        dir = Vector3.up;
                        spawnPos.y -= wiggleRoom;
                    }
                    else
                        spawnPos.y += wiggleRoom;


                    if (nearestObject.Raycast(new Ray(spawnPos, dir), out RaycastHit raycastHit, bounds.size.y + wiggleRoom))
                    {
                        spawnPos = raycastHit.point;
                        break;
                    }
                    else
                    {
                        spawnPos = Vector3.Lerp(originalPos, bounds.center, progress);
                        spawnPos.y = originalPos.y;
                    }
                }

                Plant plant = Instantiate(_possiblePlantPrefabs[i], spawnPos, transform.rotation).GetComponentInChildren<Plant>();

                _plants.Add(plant);
                plant.VegetationSys = this;

                spawnPos.x = center.x;
                spawnPos.z = center.z;
            }
        }
    }

    /// <summary>
    /// Use this version when the new plant is allowed to take a spot inside the creator's already occupied area
    /// </summary>
    /// <param name="position"></param>
    /// <param name="plant"></param>
    /// <param name="creatorPosition"></param>
    /// <returns></returns>
    public bool AttemptOccupy(ref Vector3 position, Plant plant, Vector3 creatorPosition)
    {
        if (_surfaceDetector.IsNearSurface(ref position, 0.01f))
        {
            Vector3Int gridPos = _grid.WorldToCell(position);
            if (_gridOccupations.ContainsKey(gridPos))
            {
                if (_gridOccupations[gridPos] != null && _gridOccupations[gridPos] != plant)
                    return false;
                else
                {
                    _gridOccupations[gridPos] = plant;
                    return true;
                }
            }
            _gridOccupations.Add(gridPos, plant);
            return true;
        }
        return false;
    }

    public bool AttemptOccupy(Vector3 position, Plant plant)
    {
        if (_surfaceDetector.IsWithinBounds(position))
        {
            Vector3Int gridPos = _grid.WorldToCell(position);
            if (_gridOccupations.ContainsKey(gridPos))
            {
                if (_gridOccupations[gridPos] != null)
                    return false;
                else
                {
                    _gridOccupations[gridPos] = plant;
                    return true;
                }
            }
            _gridOccupations.Add(gridPos, plant);
            return true;
        }
        return false;
    }

    public void DeregisterPlant(Plant plant)
    {
        _plants.Remove(plant);
    }

    public void RemoveOccupationAt(Vector3 position)
    {
        Vector3Int gridPos = _grid.WorldToCell(position);
        if (_gridOccupations.ContainsKey(gridPos))
            _gridOccupations[gridPos] = null;
    }

    private void OnDayPassed()
    {
        foreach(Plant plant in _plants)
            plant.OnDayPassed();
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
