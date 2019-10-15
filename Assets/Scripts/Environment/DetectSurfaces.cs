using System.Collections.Generic;
using UnityEngine;

public class DetectSurfaces : MonoBehaviour
{
    private Collider _collider = null;
    private List<Bounds> _surfaces = new List<Bounds>();
    private List<Collider> _objectsInside = new List<Collider>();

    private void Start()
    {
        _collider = GetComponent<Collider>();
    }

    public bool IsNearSurface(ref Vector3 pos, float acceptableDist, out float shadowFactor)
    {
        for (int i = 0, length = _surfaces.Count; i < length; ++i)
        {
            Bounds bounds = _surfaces[i];
            bounds.extents = new Vector3(bounds.extents.x, bounds.extents.y + acceptableDist, bounds.extents.z);
            if (!bounds.Contains(pos)) continue;

            Vector3 dir = Vector3.down;
            pos.y = bounds.max.y;

            if (_objectsInside[i].Raycast(new Ray(pos, dir), out RaycastHit raycastHit, bounds.size.y + acceptableDist))
            {
                pos = raycastHit.point;
                shadowFactor = ShadowMaskSampler.Instance.CalculateShadowFromHit(raycastHit);
                return true;
            }
        }
        shadowFactor = 0f;
        return false;
    }

    public bool IsWithinBounds(Vector3 position)
    {
        return _collider.bounds.Contains(position);
    }

    public Collider GetNearestSurfaceTo(Vector3 pos, float maxYDifference)
    {
        float sqDist = 0f;
        Bounds bounds = _surfaces[0];
        float minDistSQ = (bounds.center - pos).sqrMagnitude;
        int closestIdx = 0;

        for (int i = 1, length = _surfaces.Count; i < length; ++i)
        {
            bounds = _surfaces[i];
            bounds.extents = new Vector3(bounds.extents.x, maxYDifference, bounds.extents.z);
            if (!bounds.Contains(pos)) continue;

            sqDist = (bounds.center - pos).sqrMagnitude;
            if (sqDist < minDistSQ)
            {
                closestIdx = i;
                minDistSQ = sqDist;
            }
        }

        return _objectsInside[closestIdx];
    }

    private void OnTriggerEnter(Collider other)
    {
        _objectsInside.Add(other);
        _surfaces.Add(other.bounds);
    }
}
