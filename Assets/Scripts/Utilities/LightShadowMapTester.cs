using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightShadowMapTester : MonoBehaviour
{
    [SerializeField] private Transform _indicator = null;
    private Vector3 _fallbackPos = Vector3.zero;

    private void Start()
    {
        _fallbackPos = _indicator.position;
    }

    private void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mouseRay, out RaycastHit hitInfo, 1000.0f, LayerMask.GetMask("VegetationEnvironment")))
        {
            _indicator.position = hitInfo.point;
            Vector2 coord = hitInfo.lightmapCoord;
            int lightIdx = hitInfo.collider.GetComponent<MeshRenderer>().lightmapIndex;
            Color color = LightmapSettings.lightmaps[lightIdx].shadowMask.GetPixelBilinear(coord.x, coord.y);
            Debug.Log(color);
        }
        else
        {
            _indicator.position = _fallbackPos;
        }
    }
}
