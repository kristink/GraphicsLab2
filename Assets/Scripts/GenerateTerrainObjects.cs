using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTerrainObjects : MonoBehaviour
{
    public float refinement = 0f;
    public float multiplier = 0f;
    public int rowsandcolumns = 0;
    public Vector2 regionSize = Vector2.one * 30;

    public float chunkSize = 200;
    public int rejectionSamples = 30;
    public float radius = 1.0f;

    public GameObject Coconut;
    public GameObject Chest;
    public GameObject Shell;

    public GameObject Grass;
    public GameObject Tree;
    public GameObject rock;

    public LayerMask layerMask;


    public void GenerateObjects(Chunk chunk, float boundsSize)
    {
        GenerateTrees(chunk, boundsSize);
        GenerateGrass(chunk, boundsSize);
        GenerateStones(chunk, boundsSize);
        GenerateCoconut(chunk, boundsSize);
        GenerateTreasure(chunk, boundsSize);
    }

    public void RemoveObejcts(Chunk chunk) {
        foreach (Transform child in chunk.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    void GenerateCoconut(Chunk chunk, float boundsSize)
    {
        Vector2 regionSize = new Vector2(boundsSize, boundsSize);
        List<Vector2> points = PoissonDiscSampling.GeneratePoints(radius * 13, regionSize, rejectionSamples);
        RaycastHit hit;
        foreach (Vector2 vec in points)
        {
            Vector2 offsetIncluded = new Vector2(vec.x - boundsSize / 2 + (chunk.coord.x * boundsSize), vec.y - boundsSize / 2 + (chunk.coord.z * boundsSize));
            Physics.Raycast(new Vector3(offsetIncluded.x, 500, offsetIncluded.y), Vector3.down, out hit, Mathf.Infinity, layerMask);
            if (hit.point.y > 1)
            {
                GameObject newObj = Instantiate(Coconut, new Vector3(offsetIncluded.x, hit.point.y, offsetIncluded.y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                newObj.transform.parent = chunk.transform;
                newObj.name = "Coconut";
            }
        }
    }
    void GenerateTreasure(Chunk chunk, float boundsSize)
    {
        Vector2 regionSize = new Vector2(boundsSize, boundsSize);
        List<Vector2> points = PoissonDiscSampling.GeneratePoints(radius * 14, regionSize, rejectionSamples);
        RaycastHit hit;
        foreach (Vector2 vec in points)
        {
            Vector2 offsetIncluded = new Vector2(vec.x - boundsSize / 2 + (chunk.coord.x * boundsSize), vec.y - boundsSize / 2 + (chunk.coord.z * boundsSize));
            Physics.Raycast(new Vector3(offsetIncluded.x, 500, offsetIncluded.y), Vector3.down, out hit, Mathf.Infinity, layerMask);
            GameObject newObj = Instantiate(Chest, new Vector3(offsetIncluded.x, hit.point.y, offsetIncluded.y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
            newObj.transform.parent = chunk.transform;
            newObj.name = "Chest";
        }
    }

    void GenerateStones(Chunk chunk, float boundsSize)
    {
        Vector2 regionSize = new Vector2(boundsSize, boundsSize);
        List<Vector2> points = PoissonDiscSampling.GeneratePoints(radius * 14, regionSize, rejectionSamples);
        RaycastHit hit;
        foreach (Vector2 vec in points)
        {
            Vector2 offsetIncluded = new Vector2(vec.x - boundsSize / 2 + (chunk.coord.x * boundsSize), vec.y - boundsSize / 2 + (chunk.coord.z * boundsSize));
            Physics.Raycast(new Vector3(offsetIncluded.x, 500, offsetIncluded.y), Vector3.down, out hit, Mathf.Infinity, layerMask);
            GameObject newObj;
            if (hit.point.y < 0)
            {
                if (Random.Range(0f, 1f) < 0.5f) newObj = Instantiate(Shell, new Vector3(offsetIncluded.x, hit.point.y, offsetIncluded.y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                else newObj = Instantiate(rock, new Vector3(offsetIncluded.x, hit.point.y, offsetIncluded.y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
            }
            else
            {
                newObj = Instantiate(rock, new Vector3(offsetIncluded.x, hit.point.y, offsetIncluded.y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
            }
            newObj.transform.parent = chunk.transform;
        }
    }

    void GenerateGrass(Chunk chunk, float boundsSize)
    {
        Vector2 regionSize = new Vector2(boundsSize, boundsSize);
        List<Vector2> points = PoissonDiscSampling.GeneratePoints(radius * 2, regionSize, rejectionSamples);
        RaycastHit hit;
        foreach (Vector2 vec in points)
        {
            Vector2 offsetIncluded = new Vector2(vec.x - boundsSize / 2 + (chunk.coord.x * boundsSize), vec.y - boundsSize / 2 + (chunk.coord.z * boundsSize));
            Physics.Raycast(new Vector3(offsetIncluded.x, 500, offsetIncluded.y), Vector3.down, out hit, Mathf.Infinity, layerMask);
            if (hit.point.y > 5)
            {
                GameObject newObj = Instantiate(Grass, new Vector3(offsetIncluded.x, hit.point.y, offsetIncluded.y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                newObj.transform.parent = chunk.transform;
            }
        }
    }

    void GenerateTrees(Chunk chunk, float boundsSize)
    {
        Vector2 regionSize = new Vector2(boundsSize, boundsSize);
        List<Vector2> points = PoissonDiscSampling.GeneratePoints(radius * 10, regionSize, rejectionSamples);
        RaycastHit hit;
        foreach (Vector2 vec in points)
        {
            Vector2 offsetIncluded = new Vector2(vec.x - boundsSize / 2 + (chunk.coord.x * boundsSize), vec.y - boundsSize / 2 + (chunk.coord.z * boundsSize));
            Physics.Raycast(new Vector3(offsetIncluded.x, 500, offsetIncluded.y), Vector3.down, out hit, Mathf.Infinity, layerMask);
            if (hit.point.y > 0.5)
            {
                GameObject newObj = Instantiate(Tree, new Vector3(offsetIncluded.x, hit.point.y, offsetIncluded.y), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                newObj.transform.parent = chunk.transform;
            }
        }
    }

    /*void Test(Vector3Int coord, float boundsSize)
    {
        List<Vector2> islandObjectPoints = islandObjectPointsList[coord];
        Vector3 newCoord = new Vector3();
        newCoord = coord * (int)boundsSize;

        List<Vector2> coconutPoints = PoissonDiscSampling.GeneratePoints(radius, regionSize, rejectionSamples);
        List<Vector2> chestPoints = PoissonDiscSampling.GeneratePoints(radius * 3.0f, regionSize, rejectionSamples);
        List<Vector2> shellPoints = PoissonDiscSampling.GeneratePoints(radius * 2, regionSize, rejectionSamples);
        List<Vector2> rockPoints = PoissonDiscSampling.GeneratePoints(radius, regionSize, rejectionSamples);
        List<Vector2> grassPoints = PoissonDiscSampling.GeneratePoints(radius, regionSize, rejectionSamples);

        List<float> islandObjectYPos = new List<float>();
        for (int k = 0; k < islandObjectPoints.Count; k++)
        {
            RaycastHit hit;
            Physics.Raycast(new Vector3(islandObjectPoints[k].x + newCoord.x, 500, islandObjectPoints[k].y + newCoord.z), Vector3.down, out hit);
            float yPos = hit.point.y;
            islandObjectYPos.Add(yPos);
        }

        List<float> coconutYpos = new List<float>();
        for (int k = 0; k < coconutPoints.Count; k++)
        {
            RaycastHit hit;
            Physics.Raycast(new Vector3(coconutPoints[k].x + newCoord.x, 500, coconutPoints[k].y + newCoord.z), Vector3.down, out hit);
            float yPos = hit.point.y;
            coconutYpos.Add(yPos);
        }

        List<float> chestYpos = new List<float>();
        for (int k = 0; k < chestPoints.Count; k++)
        {
            RaycastHit hit;
            Physics.Raycast(new Vector3(chestPoints[k].x + newCoord.x, 500, chestPoints[k].y + newCoord.z), Vector3.down, out hit);
            float yPos = hit.point.y;
            chestYpos.Add(yPos);
        }

        List<float> shellYpos = new List<float>();
        for (int k = 0; k < shellPoints.Count; k++)
        {
            RaycastHit hit;
            Physics.Raycast(new Vector3(shellPoints[k].x + newCoord.x, 500, shellPoints[k].y + newCoord.z), Vector3.down, out hit);
            float yPos = hit.point.y;
            shellYpos.Add(yPos);
        }

        List<float> rockYpos = new List<float>();
        for (int k = 0; k < rockPoints.Count; k++)
        {
            RaycastHit hit;
            Physics.Raycast(new Vector3(rockPoints[k].x + newCoord.x, 500, rockPoints[k].y + newCoord.z), Vector3.down, out hit);
            float yPos = hit.point.y;
            rockYpos.Add(yPos);
        }

        List<float> grassYpos = new List<float>();
        for (int k = 0; k < grassPoints.Count; k++)
        {
            RaycastHit hit;
            Physics.Raycast(new Vector3(grassPoints[k].x + newCoord.x, 500, grassPoints[k].y + newCoord.z), Vector3.down, out hit);
            float yPos = hit.point.y;
            grassYpos.Add(yPos);
        }


        for (int k = 0; k < islandObjectPoints.Count; k++)
        {
            float yMark = 2.5f;
            float yMark2 = 8.5f;
            if (islandObjectYPos[k] > yMark && islandObjectYPos[k] < yMark2)
            {
                GameObject aStaticObject;
                float randomNum = Random.Range(0, 60);
                float treeOffset = 1.0f;
                float rockOffset = 1.0f;
                if (randomNum > 55)
                {
                    aStaticObject = Instantiate(static_object[0], new Vector3(islandObjectPoints[k].x + newCoord.x, islandObjectYPos[k] - treeOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if (randomNum <= 55 && randomNum > 50)
                {
                    aStaticObject = Instantiate(static_object[1], new Vector3(islandObjectPoints[k].x + newCoord.x * chunkSize, islandObjectYPos[k] - treeOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if (randomNum <= 50 && randomNum > 45)
                {
                    aStaticObject = Instantiate(static_object[2], new Vector3(islandObjectPoints[k].x + newCoord.x * chunkSize, islandObjectYPos[k] - treeOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if (randomNum <= 45 && randomNum > 40)
                {
                    aStaticObject = Instantiate(static_object[3], new Vector3(islandObjectPoints[k].x + newCoord.x * chunkSize, islandObjectYPos[k] - treeOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if (randomNum <= 40 && randomNum > 35)
                {
                    aStaticObject = Instantiate(static_object[4], new Vector3(islandObjectPoints[k].x + newCoord.x * chunkSize, islandObjectYPos[k] - treeOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if (randomNum <= 35 && randomNum > 30)
                {
                    aStaticObject = Instantiate(static_object[5], new Vector3(islandObjectPoints[k].x + newCoord.x * chunkSize, islandObjectYPos[k] - treeOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if (randomNum <= 30 && randomNum > 25)
                {
                    aStaticObject = Instantiate(static_object[6], new Vector3(islandObjectPoints[k].x + newCoord.x * chunkSize, islandObjectYPos[k] - treeOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if (randomNum <= 25 && randomNum > 20)
                {
                    aStaticObject = Instantiate(static_object[7], new Vector3(islandObjectPoints[k].x + newCoord.x * chunkSize, islandObjectYPos[k] - treeOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if (randomNum <= 20 && randomNum > 15)
                {
                    aStaticObject = Instantiate(static_object[8], new Vector3(islandObjectPoints[k].x + newCoord.x * chunkSize, islandObjectYPos[k] - treeOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if (randomNum <= 15 && randomNum > 10)
                {
                    aStaticObject = Instantiate(static_object[9], new Vector3(islandObjectPoints[k].x + newCoord.x * chunkSize, islandObjectYPos[k] - treeOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if (randomNum <= 10 && randomNum > 4)
                {
                    aStaticObject = Instantiate(static_object[10], new Vector3(islandObjectPoints[k].x + newCoord.x * chunkSize, islandObjectYPos[k] - treeOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if (randomNum <= 4 && randomNum > 3)
                {
                    aStaticObject = Instantiate(static_object[11], new Vector3(islandObjectPoints[k].x + newCoord.x * chunkSize, islandObjectYPos[k] - treeOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                }
                else if (randomNum <= 3 && randomNum > 2)
                {
                    aStaticObject = Instantiate(static_object[12], new Vector3(islandObjectPoints[k].x + newCoord.x * chunkSize, islandObjectYPos[k] - rockOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                    Rigidbody gameObjectsRigidBody = aStaticObject.AddComponent<Rigidbody>(); // Add the rigidbody.
                    gameObjectsRigidBody.useGravity = true;
                    gameObjectsRigidBody.mass = 100; // Set the GO's mass to 5 via the Rigidbody.
                    gameObjectsRigidBody.angularDrag = 200;
                }
                else if (randomNum <= 2 && randomNum > 1)
                {
                    aStaticObject = Instantiate(static_object[13], new Vector3(islandObjectPoints[k].x + newCoord.x * chunkSize, islandObjectYPos[k] - rockOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                    Rigidbody gameObjectsRigidBody = aStaticObject.AddComponent<Rigidbody>(); // Add the rigidbody.
                    gameObjectsRigidBody.useGravity = true;
                    gameObjectsRigidBody.mass = 100; // Set the GO's mass to 5 via the Rigidbody.
                    gameObjectsRigidBody.angularDrag = 200;
                }
                else if (randomNum <= 1 && randomNum > 0.5)
                {
                    aStaticObject = Instantiate(static_object[14], new Vector3(islandObjectPoints[k].x + newCoord.x * chunkSize, islandObjectYPos[k] - rockOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                    Rigidbody gameObjectsRigidBody = aStaticObject.AddComponent<Rigidbody>(); // Add the rigidbody.
                    gameObjectsRigidBody.useGravity = true;
                    gameObjectsRigidBody.mass = 100; // Set the GO's mass to 5 via the Rigidbody.
                    gameObjectsRigidBody.angularDrag = 200;
                }
                else
                {
                    aStaticObject = Instantiate(static_object[15], new Vector3(islandObjectPoints[k].x + newCoord.x, islandObjectYPos[k] - rockOffset, islandObjectPoints[k].y + newCoord.z), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                    Rigidbody gameObjectsRigidBody = aStaticObject.AddComponent<Rigidbody>(); // Add the rigidbody.
                    gameObjectsRigidBody.useGravity = true;
                    gameObjectsRigidBody.mass = 100; // Set the GO's mass to 5 via the Rigidbody.
                    gameObjectsRigidBody.angularDrag = 200;
                }
                MeshCollider mc = aStaticObject.AddComponent<MeshCollider>();
                mc.convex = true;
                //TODO get this working
                //existingIslandObjects[coord].Add(aStaticObject);

            }

        }

        for (int k = 0; k < coconutPoints.Count; k++)
        {
            float yMark = 1.5f;
            if (coconutYpos[k] > yMark)
            {
                GameObject aCoconut = Instantiate(Coconut, new Vector3(coconutPoints[k].x + newCoord.x * chunkSize, 2.1f, coconutPoints[k].y + newCoord.z * chunkSize), Quaternion.identity);
                aCoconut.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                Rigidbody gameObjectsRigidBody = aCoconut.AddComponent<Rigidbody>(); // Add the rigidbody.
                gameObjectsRigidBody.useGravity = true;
                MeshCollider mc = aCoconut.AddComponent<MeshCollider>();
                mc.convex = true;
                gameObjectsRigidBody.mass = 30; // Set the GO's mass to 5 via the Rigidbody.
                gameObjectsRigidBody.angularDrag = 80;
                aCoconut.name = "Coconut";
            }
        }

        for (int k = 0; k < chestPoints.Count; k++)
        {
            float yMark1 = 2.0f;
            float yMark2 = -1.0f;
            if (chestYpos[k] > yMark1 || chestYpos[k] < yMark2)
            {
                GameObject aChest = Instantiate(Chest, new Vector3(chestPoints[k].x + newCoord.x * chunkSize, 2.1f, chestPoints[k].y + newCoord.z * chunkSize), Quaternion.identity);
                aChest.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                GameObject top = aChest.transform.GetChild(1).gameObject;
                aChest.transform.Rotate(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
                top.transform.Rotate(Random.Range(-40, 0), 0, 0);
                Rigidbody gameObjectsRigidBody = aChest.AddComponent<Rigidbody>(); // Add the rigidbody.
                gameObjectsRigidBody.useGravity = true;
                MeshCollider mc = aChest.AddComponent<MeshCollider>();
                mc.convex = true;
                gameObjectsRigidBody.mass = 100; // Set the GO's mass to 5 via the Rigidbody.
                gameObjectsRigidBody.angularDrag = 200;
                aChest.name = "Chest";
                //coconutList.Add(aChest);
            }
        }

        for (int k = 0; k < shellPoints.Count; k++)
        {
            float yMark = 1.8f;
            if (shellYpos[k] < yMark)
            {
                GameObject aShell = Instantiate(Shell, new Vector3(shellPoints[k].x + newCoord.x * chunkSize, 1.9f, shellPoints[k].y + newCoord.z * chunkSize), Quaternion.identity);
                MeshCollider mc = aShell.AddComponent<MeshCollider>();
                mc.convex = true;
                aShell.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                aShell.transform.Rotate(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
                Rigidbody gameObjectsRigidBody = aShell.AddComponent<Rigidbody>(); // Add the rigidbody.
                gameObjectsRigidBody.useGravity = true;
                gameObjectsRigidBody.mass = 5; // Set the GO's mass to 5 via the Rigidbody.
                gameObjectsRigidBody.angularDrag = 50;
                aShell.name = "Shell";
                //coconutList.Add(aChest);
            }
        }

        for (int k = 0; k < rockPoints.Count; k++)
        {
            float yMark = 1.8f;
            int rockIndex = Random.Range(0, rocks.Length - 1);
            if (rockYpos[k] < yMark)
            {
                GameObject aRock = Instantiate(rocks[rockIndex], new Vector3(rockPoints[k].x + newCoord.x * chunkSize, 1.6f, rockPoints[k].y + newCoord.z * chunkSize), Quaternion.identity);
                MeshCollider mc = aRock.AddComponent<MeshCollider>();
                mc.convex = true;
                aRock.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                aRock.transform.Rotate(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
                Rigidbody gameObjectsRigidBody = aRock.AddComponent<Rigidbody>(); // Add the rigidbody.
                gameObjectsRigidBody.useGravity = true;
                gameObjectsRigidBody.mass = 5; // Set the GO's mass to 5 via the Rigidbody.
                gameObjectsRigidBody.angularDrag = 50;
                aRock.name = "Rock";
                //coconutList.Add(aChest);
            }
        }

        for (int k = 0; k < grassPoints.Count; k++)
        {
            float yMark = -2.3f;
            float yMark2 = 3.2f;
            float yMark3 = 8.0f;
            if ((grassYpos[k] < yMark || grassYpos[k] > yMark2) && grassYpos[k] < yMark3)
            {
                GameObject aGrass = Instantiate(Grass, new Vector3(grassPoints[k].x + newCoord.x * chunkSize, grassYpos[k], grassPoints[k].y + newCoord.z * chunkSize), Quaternion.identity);
                MeshCollider mc = aGrass.AddComponent<MeshCollider>();
                mc.convex = true;
                aGrass.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                aGrass.transform.Rotate(0, Random.Range(0, 360), 0);

                //coconutList.Add(aChest);
            }
        }

        //boat.transform.position = new Vector3(boat.transform.position.x, 3, boat.transform.position.z);
    }
    */
}
