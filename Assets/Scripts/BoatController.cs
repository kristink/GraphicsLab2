using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    Rigidbody m_Rigidbody;
    public float m_Speed = 10.0f;
    public float torque = 10.0f;
    void Start()
    {
        //Fetch the Rigidbody component you attach from your GameObject
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        m_Rigidbody.MovePosition(transform.position + new Vector3(transform.forward.x, 0f, transform.forward.z) * z * m_Speed * Time.deltaTime);

        //transform.Rotate(transform.right * x * m_Speed * Time.deltaTime);
        
        m_Rigidbody.AddTorque(transform.up * torque * x);
    }
}
