using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    Transform boat;

    [SerializeField]
    Transform player;

    [SerializeField]
    Transform player_boat_pos;

    BoatController boatController;
    PlayerMovement playerMovement;

    //public int coconutCount = 5;
    //public int goldCount = 0;

    //public Text goldText;

    //public Text coconutText;
    
    Camera m_MainCamera;
    CameraOrbit cameraOrbit;
    MouseLook mouseLook;

    public bool playerMovementIsActive = true;
    // Start is called before the first frame update

    void Start()
    {
        m_MainCamera = Camera.main;
        cameraOrbit = m_MainCamera.GetComponent<CameraOrbit>();
        mouseLook = m_MainCamera.GetComponent<MouseLook>();
        cameraOrbit.enabled = !playerMovementIsActive;
        mouseLook.enabled = playerMovementIsActive;

        boatController = boat.GetComponent<BoatController>();
        playerMovement = player.GetComponent<PlayerMovement>();
        boatController.enabled = !playerMovementIsActive;
        playerMovement.enabled = playerMovementIsActive;
    }

    // Update is called once per frame
    void Update()
    {

        //coconutCount = playerMovement.GetCoconutCount();
        //goldCount = playerMovement.GetGoldCount();

        if(Mathf.Abs(Vector3.Distance(boat.position, player.position)) < 10)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                playerMovementIsActive = !playerMovementIsActive;
                boatController.enabled = !playerMovementIsActive;
                playerMovement.enabled = playerMovementIsActive;

                cameraOrbit.enabled = !playerMovementIsActive;
                mouseLook.enabled = playerMovementIsActive;

                if (playerMovementIsActive)
                {
                    m_MainCamera.transform.localPosition = new Vector3(0, 0.8f, 0);
                    player.parent = null;
                    player.position = boat.position + new Vector3(4,0,0);
                    m_MainCamera.transform.parent = player;
                    m_MainCamera.transform.localPosition = new Vector3(0, 0.8f, 0);
                }
                else
                {
                    player.position = player_boat_pos.position;
                    player.parent = boat;
                    m_MainCamera.transform.parent = null;
                }
            }
        }

        
        
    }
}
