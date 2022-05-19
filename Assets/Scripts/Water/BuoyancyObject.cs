using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BuoyancyObject : MonoBehaviour
{
    public List<Transform> Floaters = new List<Transform>();
    public float UnderWaterDrag = 3f;
    public float UnderWaterAngularDrag = 1f;
    public float AirDrag = 0f;
    public float AirAngularDrag = 0.05f;
    public float FloatingPower = 15f;
    OceanManager oceanManager;

    Rigidbody Rb;
    bool Underwater;
    int FloatersUnderWater;

    public float floatersDistance = 0.5f;
    public Transform floatersParent;
    public GameObject objToSpawn;
    void CreateFloaters()
    {
        for(float x = -1f; x <= 1f; x += floatersDistance)
        {
            for (float z = -2.5f; z <= 2.5f; z++)
            {
                GameObject objToSpawn2 = Instantiate(objToSpawn);
                objToSpawn2.transform.parent = floatersParent;
                objToSpawn2.transform.localPosition = new Vector3(x, floatersParent.localPosition.y, z);
                Floaters.Add(objToSpawn2.transform);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CreateFloaters();
        Rb = this.GetComponent<Rigidbody>();
        oceanManager = FindObjectOfType<OceanManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        FloatersUnderWater = 0;
        for (int i = 0; i < Floaters.Count; i++)
        {
            float diff = Floaters[i].position.y - oceanManager.WaterHeightAtPosition(Floaters[i].position);
            if (diff < 0)
            {
                Rb.AddForceAtPosition(Vector3.up * FloatingPower * Mathf.Abs(diff), Floaters[i].position, ForceMode.Force);
                FloatersUnderWater += 1;
                if (!Underwater)
                {
                    Underwater = true;
                    SwitchState(true);
                }
            }
        }
        if (Underwater && FloatersUnderWater == 0)
        {
            Underwater = false;
            SwitchState(false);
        }
    }
    void SwitchState(bool isUnderwater)
    {
        if (isUnderwater)
        {
            Rb.drag = UnderWaterDrag;
            Rb.angularDrag = UnderWaterAngularDrag;
        }
        else
        {
            Rb.drag = AirDrag;
            Rb.angularDrag = AirAngularDrag;
        }
    }
}
