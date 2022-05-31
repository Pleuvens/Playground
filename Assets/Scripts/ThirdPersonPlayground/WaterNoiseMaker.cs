using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterNoiseMaker : MonoBehaviour
{
    [SerializeField] float power;
    [SerializeField] float scale;
    [SerializeField] float timeScale;

    float xOffset;
    float yOffset;
    MeshFilter filter;
    // Start is called before the first frame update
    void Start()
    {
        filter = GetComponent<MeshFilter>();
        MakeNoise();
    }

    // Update is called once per frame
    void Update()
    {
        MakeNoise();
        xOffset += Time.deltaTime;
        yOffset += Time.deltaTime;
    }

    void MakeNoise()
    {
        Vector3[] verticies = filter.mesh.vertices;
        for (int i = 0; i < verticies.Length; i++)
        {
            verticies[i].y = CalculateHeight(verticies[i].x, verticies[i].z) * power;
        }
        filter.mesh.vertices = verticies;
        filter.mesh.RecalculateNormals();
    }

    float CalculateHeight(float x, float y)
    {
        float xCord = x * scale + xOffset;
        float yCord = y * scale + yOffset;
        return Mathf.PerlinNoise(xCord, yCord);
    }
}
