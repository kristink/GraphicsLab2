using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public int coconutCount = 5;
    public int goldCount = 0;
    public Text goldText;

    public Text coconutText;


    Vector3 velocity;
    bool isGrounded;
    public bool isUnderWater;
    public Slider underWaterSlider;
    public GameObject GameOverPanel;
    float drownSpeed = 0.05f;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        

        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        if (isUnderWater)
        {
            underWaterSlider.gameObject.SetActive(true);
            velocity.y *= 0.8f;
            move *= 0.3f;
            if(underWaterSlider.value <= 0) GameOver();
            underWaterSlider.value = underWaterSlider.value - drownSpeed * Time.deltaTime;
        }
        else if(underWaterSlider.value < 1)
        {
            underWaterSlider.value = underWaterSlider.value + drownSpeed * Time.deltaTime;
        }
        else
        {
            underWaterSlider.gameObject.SetActive(false);
        }

        controller.Move(move * speed * Time.deltaTime);

        controller.Move(velocity * Time.deltaTime);

        if( Input.GetMouseButtonDown(0) )
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
                RaycastHit hit;
                if( Physics.Raycast(ray, out hit, 5.0f) && hit.transform.gameObject != null )
                {
                // here you need to insert a check if the object is really a tree
                // for example by tagging all trees with "Tree" and checking hit.transform.tag
                if((hit.transform.gameObject.name == "Coconut") || (hit.transform.gameObject.name == "Chest")){
                    if(hit.transform.gameObject.name == "Coconut"){
                        coconutCount += 1;
                    }
                    if(hit.transform.gameObject.name == "Chest"){
                        goldCount += 100;
                    }
                    GameObject.Destroy(hit.transform.gameObject);
                    }
                }
            }

        goldText.text = "Gold: " + goldCount.ToString();
        coconutText.text = "Coconuts: " + coconutCount.ToString();
    }

    public int GetCoconutCount(){
        return coconutCount;
    }

    public int GetGoldCount(){
        return goldCount;
    }

    void GameOver()
    {
        GameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
