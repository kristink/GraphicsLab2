using UnityEngine;
using System.Collections;

public class Underwater : MonoBehaviour
{
    public float waterHeight;

    private bool isUnderwater;
    public Color normalColor;
    public Color underwaterColor;

    public CharacterController characterController;
    public PlayerMovement playerMovement;
    // Use this for initialization
    void Start()
    {
        normalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        underwaterColor = new Color(0.22f, 0.65f, 0.77f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if ((transform.position.y < waterHeight) != isUnderwater)
        {
            isUnderwater = transform.position.y < waterHeight;
            if (isUnderwater)
            {
                SetUnderwater();
                playerMovement.isUnderWater = true;
            }
            if (!isUnderwater)
            {
                playerMovement.isUnderWater = false;
                SetNormal();
            }
        }

    }

    void SetNormal()
    {
        RenderSettings.fogColor = normalColor;
        RenderSettings.fogMode = FogMode.Linear;
    }

    void SetUnderwater()
    {
        RenderSettings.fogColor = underwaterColor;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.1f;

    }
}