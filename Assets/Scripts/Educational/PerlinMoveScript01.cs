using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinMoveScript01 : MonoBehaviour
{
    public float elapsedTime = 0f;
    public float perlinNoise = 0f;
    public float multiplier = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime = Time.time;
        perlinNoise = Mathf.PerlinNoise(elapsedTime, 0);

        transform.position = new Vector3(
            transform.position.x,
            perlinNoise * multiplier,
            transform.position.z
        );
    }
}
