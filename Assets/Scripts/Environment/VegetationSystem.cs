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
                RaycastHit raycastHit = new RaycastHit();
                float shadowFactor = 0f;

                for (float progress = 0.1f; progress < 1.01f; progress += 0.1f)
                {
                    Vector3 dir = Vector3.down;
                    float wiggleRoom = 0.01f;
                    spawnPos.y = bounds.max.y;

                    if (nearestObject.Raycast(new Ray(spawnPos, dir), out raycastHit, bounds.size.y + wiggleRoom))
                    {
                        spawnPos = raycastHit.point;
                        shadowFactor = ShadowMaskSampler.Instance.CalculateShadowFromHit(raycastHit);
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
                plant.ShadowFactor = shadowFactor;

                spawnPos.x = center.x;
                spawnPos.z = center.z;
            }
        }
    }

    /// <summary>
    /// Use this version when the new plant is not allowed to take a spot inside the plant's already occupied area
    /// </summary>
    /// <returns></returns>
    public bool AttemptHardOccupy(ref Vector3 position, Vector3 creatorPos, Plant plant, out float shadowFactor)
    {
        Vector3Int gridPos = _grid.WorldToCell(position);
        if (gridPos == _grid.WorldToCell(creatorPos))
        {
            shadowFactor = 0f;
            return false;
        }

        if (_surfaceDetector.IsNearSurface(ref position, 0.01f, out shadowFactor))
        {
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

    public bool AttemptOccupy(ref Vector3 position, Plant plant, out float shadowFactor)
    {
        if (_surfaceDetector.IsNearSurface(ref position, 0.01f, out shadowFactor))
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

    public void RemoveOccupationsBy(Plant plant)
    {
        List<Vector3Int> _buffer = new List<Vector3Int>(); 

        foreach (KeyValuePair<Vector3Int, Plant> pair in _gridOccupations)
        {
            if (pair.Value != plant) continue;
            _buffer.Add(pair.Key);
        }

        foreach(Vector3Int key in _buffer)
            _gridOccupations[key] = null;
    }

    public Plant GetOccupationNear(Vector3 position, ref Vector3Int direction)
    {
        direction = _grid.WorldToCell(position) + direction;
        if (_gridOccupations.ContainsKey(direction))
            return _gridOccupations[direction];
        return null;
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
