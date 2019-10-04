using System.Collections.Generic;
using UnityEngine;

public class DetectSurfaces : MonoBehaviour
{
    private List<Bounds> _surfaces = new List<Bounds>();

    private List<Collider> _objectsInside = new List<Collider>();

    private float CalculateSurface(Mesh mesh, Vector3 direction)
    {
        direction = direction.normalized;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        double sum = 0.0;

        for (int i = 0, length = triangles.Length; i < length; i += 3)
        {
            Vector3 corner = vertices[triangles[i]];
            Vector3 a = vertices[triangles[i + 1]] - corner;
            Vector3 b = vertices[triangles[i + 2]] - corner;

            float projection = Vector3.Dot(Vector3.Cross(b, a), direction);
            if (projection > 0f)
                sum += projection;
        }

        return (float)(sum / 2.0);
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
