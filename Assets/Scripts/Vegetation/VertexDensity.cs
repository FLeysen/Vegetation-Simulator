using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexDensity : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Color32[] colours = new Color32[vertices.Length];

        float red = Random.Range(0, 256);

        for (int i = 0, length = vertices.Length; i < length; ++i)
        {
            byte value = (byte)(i * 255 / length);
            colours[i] = new Color32(0, 255, 0, 255); //new Color32((byte)red, value, 0, 255);
        }

        mesh.colors32 = colours;
    }
}
