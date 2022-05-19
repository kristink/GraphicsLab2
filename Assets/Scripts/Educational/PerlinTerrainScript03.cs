using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinTerrainScript03 : MonoBehaviour
{
    float perlinNoise = 0f;
    public float refinement = 0f;
    public float multiplier = 0f;
    public float [,] terrainHeightData;
    public int rowsandcolumns = 0;
    public Vector2 regionSize = Vector2.one * 200;
    public int rejectionSamples = 30;
    public float radius  = 1.0f;
    Terrain terrain;
    List<Vector2> points;
    List<Vector2> points2;
    List<Vector2> points3;
    List<float> perlinList = new List<float> ();

    public GameObject Coconut;
    public GameObject Chest;

    public GameObject Branch;
    //public GameObject Tree1;
    //public GameObject Tree2;

    public GameObject [] static_object;

    List<GameObject> coconutList = new List<GameObject>();

    public Camera[] cameras;
    private int currentCameraIndex;

    // Start is called before the first frame update
    void Start()
    {

        points = PoissonDiscSampling.GeneratePoints(radius*2, regionSize, rejectionSamples);
        points2 = PoissonDiscSampling.GeneratePoints(radius*3, regionSize, rejectionSamples);
        points3 = PoissonDiscSampling.GeneratePoints(radius, regionSize, rejectionSamples);
        terrainHeightData = new float[rowsandcolumns, rowsandcolumns];
        terrain = GetComponent<Terrain>();

        for(int i = 0; i < rowsandcolumns; i++)
        {
            for(int j = 0; j < rowsandcolumns; j++)
            {
                perlinNoise = Mathf.PerlinNoise(i* refinement, j * refinement);
                terrainHeightData[i,j] = perlinNoise * multiplier; 
                //GameObject aCoconut = Instantiate(Coconut, new Vector3(i,perlinNoise,j), Quaternion.identity);
                //coconutList.Add(aCoconut);
            }
        }

        terrain.terrainData.SetHeights(0, 0, terrainHeightData);


        for(int i = 0; i < points.Count; i++){
            GameObject aCoconut = Instantiate(Coconut, new Vector3(points[i].x,10,points[i].y), Quaternion.identity);
            Rigidbody gameObjectsRigidBody = aCoconut.AddComponent<Rigidbody>(); // Add the rigidbody.
            gameObjectsRigidBody.useGravity = true;
            MeshCollider mc = aCoconut.AddComponent<MeshCollider>();
            mc.convex = true;
            gameObjectsRigidBody.mass = 30; // Set the GO's mass to 5 via the Rigidbody.
            gameObjectsRigidBody.angularDrag = 80;
            aCoconut.name = "Coconut"; 
            coconutList.Add(aCoconut);
        }
        //GameObject aCoconut = Instantiate(Coconut, new Vector3(1,1,1), Quaternion.identity);

        for(int i = 0; i < points2.Count; i++){
            GameObject aChest = Instantiate(Chest, new Vector3(points2[i].x,10,points2[i].y), Quaternion.identity);
            GameObject top = aChest.transform.GetChild(1).gameObject;
            top.transform.Rotate(Random.Range(-40,0), 0, 0);
            Rigidbody gameObjectsRigidBody = aChest.AddComponent<Rigidbody>(); // Add the rigidbody.
            gameObjectsRigidBody.useGravity = true;
            MeshCollider mc = aChest.AddComponent<MeshCollider>();
            mc.convex = true;
            gameObjectsRigidBody.mass = 100; // Set the GO's mass to 5 via the Rigidbody.
            gameObjectsRigidBody.angularDrag = 200;
            aChest.name = "Chest";
            coconutList.Add(aChest);
        }

        for(int i = 0; i < points3.Count; i++){
            float yPos = Terrain.activeTerrain.SampleHeight(new Vector3(points3[i].x, -10, points3[i].y)) - 10;
            Debug.Log(yPos);
            float yMark = 1.0f;
            if(yPos > yMark){
                GameObject aStaticObject;
                float randomNum = Random.Range(0, 60);
                if(randomNum > 55){
                    aStaticObject = Instantiate(static_object[0], new Vector3(points3[i].x,yPos-1,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if(randomNum <= 55 && randomNum > 50){
                    aStaticObject = Instantiate(static_object[1], new Vector3(points3[i].x,yPos-1,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if(randomNum <= 50 && randomNum > 45){
                    aStaticObject = Instantiate(static_object[2], new Vector3(points3[i].x,yPos-1,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if(randomNum <= 45 && randomNum > 40){
                    aStaticObject = Instantiate(static_object[3], new Vector3(points3[i].x,yPos-1,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if(randomNum <= 40 && randomNum > 35){
                    aStaticObject = Instantiate(static_object[4], new Vector3(points3[i].x,yPos-1,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if(randomNum <= 35 && randomNum > 30){
                    aStaticObject = Instantiate(static_object[5], new Vector3(points3[i].x,yPos-1,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if(randomNum <= 30 && randomNum > 25){
                    aStaticObject = Instantiate(static_object[6], new Vector3(points3[i].x,yPos-1,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if(randomNum <= 25 && randomNum > 20){
                    aStaticObject = Instantiate(static_object[7], new Vector3(points3[i].x,yPos-1,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if(randomNum <= 20 && randomNum > 15){
                    aStaticObject = Instantiate(static_object[8], new Vector3(points3[i].x,yPos-1,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if(randomNum <= 15 && randomNum > 10){
                    aStaticObject = Instantiate(static_object[9], new Vector3(points3[i].x,yPos-1,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if(randomNum <= 10 && randomNum > 4){
                    aStaticObject = Instantiate(static_object[10], new Vector3(points3[i].x,yPos-1,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if(randomNum <= 4 && randomNum > 3){
                    aStaticObject = Instantiate(static_object[11], new Vector3(points3[i].x,yPos-5,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if(randomNum <= 3 && randomNum > 2){
                    aStaticObject = Instantiate(static_object[12], new Vector3(points3[i].x,yPos-5,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if(randomNum <= 2 && randomNum > 1){
                    aStaticObject = Instantiate(static_object[13], new Vector3(points3[i].x,yPos-5,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if(randomNum <= 1 && randomNum > 0.5){
                    aStaticObject = Instantiate(static_object[14], new Vector3(points3[i].x,yPos-5,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else{
                    aStaticObject = Instantiate(static_object[15], new Vector3(points3[i].x,yPos-5,points3[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                MeshCollider mc = aStaticObject.AddComponent<MeshCollider>();
                mc.convex = true;
                //aStaticObject.name = "StaticObject";
            }

        }



        currentCameraIndex = 0;
         
         //Turn all cameras off, except the first default one
         for (int i=1; i<cameras.Length; i++) 
         {
             cameras[i].gameObject.SetActive(false);
         }
         
         //If any cameras were added to the controller, enable the first one
         if (cameras.Length>0)
         {
             cameras [0].gameObject.SetActive (true);
             Debug.Log ("Camera with name: " + cameras [0].GetComponent<Camera>().name + ", is now enabled");
         }

    }
        
    // Update is called once per frame
    void Update()
    {

        //If the c button is pressed, switch to the next camera
         //Set the camera at the current index to inactive, and set the next one in the array to active
         //When we reach the end of the camera array, move back to the beginning or the array.
         if (Input.GetKeyDown(KeyCode.C))
         {
             currentCameraIndex ++;
             //Debug.Log ("C button has been pressed. Switching to the next camera");
             if (currentCameraIndex < cameras.Length)
             {
                 cameras[currentCameraIndex-1].gameObject.SetActive(false);
                 cameras[currentCameraIndex].gameObject.SetActive(true);
                 //Debug.Log ("Camera with name: " + cameras [currentCameraIndex].GetComponent<Camera>().name + ", is now enabled");
             }
             else
             {
                 cameras[currentCameraIndex-1].gameObject.SetActive(false);
                 currentCameraIndex = 0;
                 cameras[currentCameraIndex].gameObject.SetActive(true);
                 //Debug.Log ("Camera with name: " + cameras [currentCameraIndex].GetComponent<Camera>().name + ", is now enabled");
             }
         }
        
    }
}
