using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinGen01 : MonoBehaviour
{

    public Vector2 perlinPos;
    public float perlinNoise = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        perlinNoise = Mathf.PerlinNoise(perlinPos.x, perlinPos.y);
    }
}
