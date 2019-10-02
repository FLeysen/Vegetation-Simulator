using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectSurfaces : MonoBehaviour
{
    private List<Bounds> _surfaces = new List<Bounds>();

    private List<Collider> _objectsInside = new List<Collider>();
    private Collider _collider = null;

    private void Start()
    {
        _collider = GetComponent<Collider>();
    }

    public float DebugCodeDeleteLater()
    {
        return _surfaces[0].max.y;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Bounds ownBounds = _collider.bounds;
        //Vector3 ownBoundsCenter = ot
        //Bounds otherBounds = other.bounds;
        //Vector3 otherBoundsCenter = otherBounds.center;
        //if (otherBounds.center > )
        //
        //Vector3 furthestPoint = Vector3.zero;
        //otherBounds.SqrDistance(otherBounds.center + otherBounds.extents);

        _objectsInside.Add(other);
        //other.bounds.
    }
}
