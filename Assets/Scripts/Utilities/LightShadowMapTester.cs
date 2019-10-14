using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightShadowMapTester : MonoBehaviour
{
    [SerializeField] private Transform _indicator = null;
    private Vector3 _fallbackPos = Vector3.zero;
    private List<int> _lightCounts = new List<int>(); //This is NOT 0-4, but a bitmask (0 - 15)! It is assumed that lights will always only be inserted from r-a
    private float _oneThird = 1f / 3f;

    private void Start()
    {
        int value = 0;
        foreach(LightmapData lightmap in LightmapSettings.lightmaps)
        {
            
            foreach (Color32 pixel in lightmap.shadowMask.GetPixels32())
            {
                if (pixel.a == 0) { }
                else value = value | 8;

                if (pixel.r == 0) { }
                else value = value | 1;

                if (pixel.g == 0) { }
                else value = value | 2;

                if (pixel.b == 0) { }
                else value = value | 4;

                if (value != 15) continue;
                break;
            }
            _lightCounts.Add(value);
            value = 0;
        }
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
            Texture2D tex = LightmapSettings.lightmaps[lightIdx].shadowMask;
            coord *= tex.height;
            Color color = tex.GetPixel((int)coord.x, (int)coord.y);
            Debug.Log(1 - CalculateAverageShadow(color, lightIdx));
        }
        else
        {
            _indicator.position = _fallbackPos;
        }
    }

    private float CalculateAverageShadow(Color color, int idx)
    {
        int val = _lightCounts[idx];
        if (val < 2)
            return color.r;
        
        else if (val < 4)
            return (color.g + color.r) * 0.5f;
        
        else if (val < 8)
            return (color.b + color.g + color.r) * _oneThird;
        
        else
            return (color.b + color.g + color.r + color.a) * 0.25f;
    }
}
