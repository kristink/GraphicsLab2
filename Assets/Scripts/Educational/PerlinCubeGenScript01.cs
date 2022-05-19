using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinCubeGenScript01 : MonoBehaviour
{
    public float perlinNoise = 0f;
    public float refinement = 0f;
    public float multiplier = 0f;
    public int cubes = 0;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < cubes; i++){
            for(int j = 0; j < cubes; j++){
                perlinNoise = Mathf.PerlinNoise(i* refinement, j * refinement);
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = new Vector3(
                    i, perlinNoise * multiplier, j
                );
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
