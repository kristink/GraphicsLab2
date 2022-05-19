using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshGeneratorCpy : MonoBehaviour {

    [SerializeField]
    GameObject WaterPlane;

    const int threadGroupSize = 8;

    [Header ("General Settings")]
    public DensityGenerator densityGenerator;

    public bool fixedMapSize;
    [ConditionalHide (nameof (fixedMapSize), true)]
    public Vector3Int numChunks = Vector3Int.one;
    [ConditionalHide (nameof (fixedMapSize), false)]
    public Transform viewer;
    [ConditionalHide (nameof (fixedMapSize), false)]
    public float viewDistance = 30;

    [Space ()]
    public bool autoUpdateInEditor = true;
    public bool autoUpdateInGame = true;
    public ComputeShader shader;
    public Material mat;
    public bool generateColliders;

    [Header ("Voxel Settings")]
    public float isoLevel;
    public float boundsSize = 1;

    public Vector3 offset = Vector3.zero;

    [Range (2, 100)]
    public int numPointsPerAxis = 30;

    [Header ("Gizmos")]
    public bool showBoundsGizmo = true;
    public Color boundsGizmoCol = Color.white;

    GameObject chunkHolder;
    const string chunkHolderName = "Chunks Holder";
    List<Chunk> chunks;
    Dictionary<Vector3Int, Chunk> existingChunks;
    Dictionary<Vector3Int, GameObject> existingWaterPlanes;
    Dictionary<Vector3Int, GameObject> existingWaterPlanes2;

    Queue<Chunk> recycleableChunks;

    // Buffers
    ComputeBuffer triangleBuffer;
    ComputeBuffer pointsBuffer;
    ComputeBuffer triCountBuffer;

    bool settingsUpdated;


    //object placements
    Dictionary<Vector3Int, List<GameObject>> existingIslandObjects;
    Dictionary<Vector3Int, List<Vector2>> islandObjectPointsList;

    public float refinement = 0f;
    public float multiplier = 0f;
    public int rowsandcolumns = 0;
    public Vector2 regionSize = Vector2.one * 200;

    public float chunkSize = 200;
    public int rejectionSamples = 30;
    public float radius  = 1.0f;
    Terrain terrain;
    List<Vector2> points;
    List<Vector2> points2;
    List<Vector2> points3;
    List<float> perlinList = new List<float> ();

    public GameObject Coconut;
    public GameObject Chest;
    public GameObject [] static_object;

    void Awake () {
        if (Application.isPlaying && !fixedMapSize) {
            InitVariableChunkStructures ();

            var oldChunks = FindObjectsOfType<Chunk> ();
            for (int i = oldChunks.Length - 1; i >= 0; i--) {
                Destroy (oldChunks[i].gameObject);
            }
        }
    }

    void Update () {
        // Update endless terrain
        if ((Application.isPlaying && !fixedMapSize)) {
            Run ();
        }

        if (settingsUpdated) {
            RequestMeshUpdate ();
            settingsUpdated = false;
        }
    }

    public void Run () {
        CreateBuffers ();

        if (fixedMapSize) {
            InitChunks ();
            UpdateAllChunks ();

        } else {
            if (Application.isPlaying) {
                InitVisibleChunks ();
            }
        }

        // Release buffers immediately in editor
        if (!Application.isPlaying) {
            ReleaseBuffers ();
        }

    }

    public void RequestMeshUpdate () {
        if ((Application.isPlaying && autoUpdateInGame) || (!Application.isPlaying && autoUpdateInEditor)) {
            Run ();
        }
    }

    void InitVariableChunkStructures () {
        recycleableChunks = new Queue<Chunk> ();
        chunks = new List<Chunk> ();
        existingChunks = new Dictionary<Vector3Int, Chunk> ();
        existingWaterPlanes = new Dictionary<Vector3Int, GameObject>();
        existingWaterPlanes2 = new Dictionary<Vector3Int, GameObject>();
        existingIslandObjects = new Dictionary<Vector3Int, List<GameObject>>();
        islandObjectPointsList = new Dictionary<Vector3Int, List<Vector2>>();
    }

    void InitVisibleChunks () {
        if (chunks==null) {
            return;
        }
        CreateChunkHolder ();

        Vector3 p = viewer.position;
        Vector3 ps = p / boundsSize;
        Vector3Int viewerCoord = new Vector3Int (Mathf.RoundToInt (ps.x), Mathf.RoundToInt (ps.y), Mathf.RoundToInt (ps.z));

        int maxChunksInView = Mathf.CeilToInt (viewDistance / boundsSize);
        float sqrViewDistance = viewDistance * viewDistance;

        // Go through all existing chunks and flag for recyling if outside of max view dst
        for (int i = chunks.Count - 1; i >= 0; i--) {
            Chunk chunk = chunks[i];
            Vector3 centre = CentreFromCoord (chunk.coord);
            Vector3 viewerOffset = p - centre;
            Vector3 o = new Vector3 (Mathf.Abs (viewerOffset.x), Mathf.Abs (viewerOffset.y), Mathf.Abs (viewerOffset.z)) - Vector3.one * boundsSize / 2;
            float sqrDst = new Vector3 (Mathf.Max (o.x, 0), Mathf.Max (o.y, 0), Mathf.Max (o.z, 0)).sqrMagnitude;
            if (sqrDst > sqrViewDistance) {
                existingChunks.Remove (chunk.coord);
                recycleableChunks.Enqueue (chunk);
                chunks.RemoveAt (i);

                if (chunk.coord.y == 0)
                {
                    GameObject waterPlane = existingWaterPlanes[chunk.coord];
                    existingWaterPlanes.Remove(chunk.coord);
                    Destroy(waterPlane);

                    GameObject waterPlane2 = existingWaterPlanes2[chunk.coord];
                    existingWaterPlanes2.Remove(chunk.coord);
                    Destroy(waterPlane2);
                    
                    // List<GameObject> toRecycleIslandObjects = existingIslandObjects[chunk.coord];
                    // for(int k = 0; k < toRecycleIslandObjects.Count; k++){
                    //     Destroy(toRecycleIslandObjects[k]);
                    // }
                    // existingIslandObjects.Remove(chunk.coord);
                }
                
            }
        }

        for (int x = -maxChunksInView; x <= maxChunksInView; x++) {
            for (int y = -maxChunksInView; y <= maxChunksInView; y++) {
                for (int z = -maxChunksInView; z <= maxChunksInView; z++) {
                    Vector3Int coord = new Vector3Int (x, y, z) + viewerCoord;

                    if (existingChunks.ContainsKey (coord)) {
                        continue;
                    }

                    Vector3 centre = CentreFromCoord (coord);
                    Vector3 viewerOffset = p - centre;
                    Vector3 o = new Vector3 (Mathf.Abs (viewerOffset.x), Mathf.Abs (viewerOffset.y), Mathf.Abs (viewerOffset.z)) - Vector3.one * boundsSize / 2;
                    float sqrDst = new Vector3 (Mathf.Max (o.x, 0), Mathf.Max (o.y, 0), Mathf.Max (o.z, 0)).sqrMagnitude;

                    // Chunk is within view distance and should be created (if it doesn't already exist)
                    if (sqrDst <= sqrViewDistance) {

                        Bounds bounds = new Bounds (CentreFromCoord (coord), Vector3.one * boundsSize);
                        if (IsVisibleFrom (bounds, Camera.main)) {
                            if (recycleableChunks.Count > 0) {
                                Chunk chunk = recycleableChunks.Dequeue ();
                                chunk.coord = coord;
                                existingChunks.Add (coord, chunk);
                                chunks.Add (chunk);
                                UpdateChunkMesh (chunk);
                                chunk.meshColliderLateUpdate(generateColliders);

                                if (coord.y == 0)
                                {
                                    GameObject waterPlane = Instantiate(WaterPlane, coord * (int)boundsSize, WaterPlane.transform.rotation, transform);
                                    existingWaterPlanes.Add(coord, waterPlane);

                                    Vector3 rot = WaterPlane.transform.rotation.eulerAngles;
                                    rot = new Vector3(rot.x + 180, rot.y, rot.z);
                                    GameObject waterPlane2 = Instantiate(WaterPlane, coord * (int)boundsSize, Quaternion.Euler(rot), transform);
                                    existingWaterPlanes2.Add(coord, waterPlane2);

                                    List<Vector2> islandObjectPoints = islandObjectPointsList[coord];
                                    Debug.Log(islandObjectPoints);

                                    for(int k = 0; k < islandObjectPoints.Count; k++){
                                        float yPos = coord.y - 10;
                                        float yMark = 1.0f;
                                        if(yPos > yMark){
                                            GameObject aStaticObject;
                                            float randomNum = Random.Range(0, 60);
                                            if(randomNum > 55){
                                                aStaticObject = Instantiate(static_object[0], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else if(randomNum <= 55 && randomNum > 50){
                                                aStaticObject = Instantiate(static_object[1], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else if(randomNum <= 50 && randomNum > 45){
                                                aStaticObject = Instantiate(static_object[2], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else if(randomNum <= 45 && randomNum > 40){
                                                aStaticObject = Instantiate(static_object[3], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else if(randomNum <= 40 && randomNum > 35){
                                                aStaticObject = Instantiate(static_object[4], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else if(randomNum <= 35 && randomNum > 30){
                                                aStaticObject = Instantiate(static_object[5], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else if(randomNum <= 30 && randomNum > 25){
                                                aStaticObject = Instantiate(static_object[6], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else if(randomNum <= 25 && randomNum > 20){
                                                aStaticObject = Instantiate(static_object[7], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else if(randomNum <= 20 && randomNum > 15){
                                                aStaticObject = Instantiate(static_object[8], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else if(randomNum <= 15 && randomNum > 10){
                                                aStaticObject = Instantiate(static_object[9], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else if(randomNum <= 10 && randomNum > 4){
                                                aStaticObject = Instantiate(static_object[10], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else if(randomNum <= 4 && randomNum > 3){
                                                aStaticObject = Instantiate(static_object[11], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else if(randomNum <= 3 && randomNum > 2){
                                                aStaticObject = Instantiate(static_object[12], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else if(randomNum <= 2 && randomNum > 1){
                                                aStaticObject = Instantiate(static_object[13], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-5,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else if(randomNum <= 1 && randomNum > 0.5){
                                                aStaticObject = Instantiate(static_object[14], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-5,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            else{
                                                aStaticObject = Instantiate(static_object[15], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-5,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                            }
                                            MeshCollider mc = aStaticObject.AddComponent<MeshCollider>();
                                            mc.convex = true;
                                            existingIslandObjects[coord].Add(aStaticObject);

                                        }

                                    }
                                }

                            } else {
                                Chunk chunk = CreateChunk (coord);
                                chunk.coord = coord;
                                chunk.SetUp (mat, generateColliders);
                                existingChunks.Add (coord, chunk);
                                chunks.Add (chunk);
                                UpdateChunkMesh (chunk);
                                chunk.meshColliderLateUpdate(generateColliders);


                                if (coord.y == 0){
                                    GameObject waterPlane = Instantiate(WaterPlane, coord * (int)boundsSize, WaterPlane.transform.rotation, transform);
                                    existingWaterPlanes.Add(coord, waterPlane);

                                    Vector3 rot = WaterPlane.transform.rotation.eulerAngles;
                                    rot = new Vector3(rot.x+180, rot.y, rot.z);
                                    GameObject waterPlane2 = Instantiate(WaterPlane, coord * (int)boundsSize, Quaternion.Euler(rot), transform);
                                    existingWaterPlanes2.Add(coord, waterPlane2);

                                    // List<Vector2> islandObjectPoints = islandObjectPointsList[coord];

                                    // for(int k = 0; k < islandObjectPoints.Count; k++){
                                    //     float yPos = coord.y - 10;
                                    //     float yMark = 1.0f;
                                    //     if(yPos > yMark){
                                    //         GameObject aStaticObject;
                                    //         float randomNum = Random.Range(0, 60);
                                    //         if(randomNum > 55){
                                    //             aStaticObject = Instantiate(static_object[0], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else if(randomNum <= 55 && randomNum > 50){
                                    //             aStaticObject = Instantiate(static_object[1], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else if(randomNum <= 50 && randomNum > 45){
                                    //             aStaticObject = Instantiate(static_object[2], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else if(randomNum <= 45 && randomNum > 40){
                                    //             aStaticObject = Instantiate(static_object[3], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else if(randomNum <= 40 && randomNum > 35){
                                    //             aStaticObject = Instantiate(static_object[4], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else if(randomNum <= 35 && randomNum > 30){
                                    //             aStaticObject = Instantiate(static_object[5], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else if(randomNum <= 30 && randomNum > 25){
                                    //             aStaticObject = Instantiate(static_object[6], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else if(randomNum <= 25 && randomNum > 20){
                                    //             aStaticObject = Instantiate(static_object[7], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else if(randomNum <= 20 && randomNum > 15){
                                    //             aStaticObject = Instantiate(static_object[8], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else if(randomNum <= 15 && randomNum > 10){
                                    //             aStaticObject = Instantiate(static_object[9], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else if(randomNum <= 10 && randomNum > 4){
                                    //             aStaticObject = Instantiate(static_object[10], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else if(randomNum <= 4 && randomNum > 3){
                                    //             aStaticObject = Instantiate(static_object[11], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else if(randomNum <= 3 && randomNum > 2){
                                    //             aStaticObject = Instantiate(static_object[12], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-1,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else if(randomNum <= 2 && randomNum > 1){
                                    //             aStaticObject = Instantiate(static_object[13], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-5,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else if(randomNum <= 1 && randomNum > 0.5){
                                    //             aStaticObject = Instantiate(static_object[14], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-5,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         else{
                                    //             aStaticObject = Instantiate(static_object[15], new Vector3(islandObjectPoints[k].x + coord.x * chunkSize ,yPos-5,islandObjectPoints[k].y + coord.z * chunkSize), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                                    //         }
                                    //         MeshCollider mc = aStaticObject.AddComponent<MeshCollider>();
                                    //         mc.convex = true;
                                    //         existingIslandObjects[coord].Add(aStaticObject);

                                    //     }
                                    //Debug.Log(islandObjectPoints);
                                    }
                                }
                            }
                    }
                }
            }
        }
    }
    

    public bool IsVisibleFrom (Bounds bounds, Camera camera) {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes (camera);
        return GeometryUtility.TestPlanesAABB (planes, bounds);
    }

    public void UpdateChunkMesh (Chunk chunk) {
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt (numVoxelsPerAxis / (float) threadGroupSize);
        float pointSpacing = boundsSize / (numPointsPerAxis - 1);

        Vector3Int coord = chunk.coord;
        Vector3 centre = CentreFromCoord (coord);

        Vector3 worldBounds = new Vector3 (numChunks.x, numChunks.y, numChunks.z) * boundsSize;

        densityGenerator.Generate (pointsBuffer, numPointsPerAxis, boundsSize, worldBounds, centre, offset, pointSpacing);

        triangleBuffer.SetCounterValue (0);
        shader.SetBuffer (0, "points", pointsBuffer);
        shader.SetBuffer (0, "triangles", triangleBuffer);
        shader.SetInt ("numPointsPerAxis", numPointsPerAxis);
        shader.SetFloat ("isoLevel", isoLevel);

        shader.Dispatch (0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount (triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData (triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData (tris, 0, 0, numTris);

        Mesh mesh = chunk.mesh;
        mesh.Clear ();

        var vertices = new Vector3[numTris * 3];
        var meshTriangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++) {
            for (int j = 0; j < 3; j++) {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        mesh.RecalculateNormals ();
    }

    public void UpdateAllChunks () {

        // Create mesh for each chunk
        foreach (Chunk chunk in chunks) {
            UpdateChunkMesh (chunk);
        }

    }

    void OnDestroy () {
        if (Application.isPlaying) {
            ReleaseBuffers ();
        }
    }

    void CreateBuffers () {
        int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        // Always create buffers in editor (since buffers are released immediately to prevent memory leak)
        // Otherwise, only create if null or if size has changed
        if (!Application.isPlaying || (pointsBuffer == null || numPoints != pointsBuffer.count)) {
            if (Application.isPlaying) {
                ReleaseBuffers ();
            }
            triangleBuffer = new ComputeBuffer (maxTriangleCount, sizeof (float) * 3 * 3, ComputeBufferType.Append);
            pointsBuffer = new ComputeBuffer (numPoints, sizeof (float) * 4);
            triCountBuffer = new ComputeBuffer (1, sizeof (int), ComputeBufferType.Raw);

        }
    }

    void ReleaseBuffers () {
        if (triangleBuffer != null) {
            triangleBuffer.Release ();
            pointsBuffer.Release ();
            triCountBuffer.Release ();
        }
    }

    Vector3 CentreFromCoord (Vector3Int coord) {
        // Centre entire map at origin
        if (fixedMapSize) {
            Vector3 totalBounds = (Vector3) numChunks * boundsSize;
            return -totalBounds / 2 + (Vector3) coord * boundsSize + Vector3.one * boundsSize / 2;
        }

        return new Vector3 (coord.x, coord.y, coord.z) * boundsSize;
    }

    void CreateChunkHolder () {
        // Create/find mesh holder object for organizing chunks under in the hierarchy
        if (chunkHolder == null) {
            if (GameObject.Find (chunkHolderName)) {
                chunkHolder = GameObject.Find (chunkHolderName);
            } else {
                chunkHolder = new GameObject (chunkHolderName);
            }
        }
    }

    // Create/get references to all chunks
    void InitChunks () {
        CreateChunkHolder ();
        chunks = new List<Chunk> ();
        List<Chunk> oldChunks = new List<Chunk> (FindObjectsOfType<Chunk> ());

        // Go through all coords and create a chunk there if one doesn't already exist
        for (int x = 0; x < numChunks.x; x++) {
            for (int y = 0; y < numChunks.y; y++) {
                for (int z = 0; z < numChunks.z; z++) {
                    Vector3Int coord = new Vector3Int (x, y, z);
                    bool chunkAlreadyExists = false;

                    // If chunk already exists, add it to the chunks list, and remove from the old list.
                    for (int i = 0; i < oldChunks.Count; i++) {
                        if (oldChunks[i].coord == coord) {
                            chunks.Add (oldChunks[i]);
                            oldChunks.RemoveAt (i);
                            chunkAlreadyExists = true;
                            break;
                        }
                    }

                    // Create new chunk
                    if (!chunkAlreadyExists) {
                        var newChunk = CreateChunk (coord);
                        chunks.Add (newChunk);
                    }

                    chunks[chunks.Count - 1].SetUp (mat, generateColliders);
                }
            }
        }

        // Delete all unused chunks
        for (int i = 0; i < oldChunks.Count; i++) {
            oldChunks[i].DestroyOrDisable ();
        }
    }

    Chunk CreateChunk (Vector3Int coord) {
        GameObject chunk = new GameObject ($"Chunk ({coord.x}, {coord.y}, {coord.z})");
        chunk.transform.parent = chunkHolder.transform;
        Chunk newChunk = chunk.AddComponent<Chunk> ();
        newChunk.coord = coord;
        if (!islandObjectPointsList.ContainsKey(coord))
            {
                islandObjectPointsList[coord] = PoissonDiscSampling.GeneratePoints(radius,regionSize,numPointsPerAxis);

            }
        return newChunk;
    }

    void OnValidate() {
        settingsUpdated = true;
    }

    struct Triangle {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this [int i] {
            get {
                switch (i) {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }

    void OnDrawGizmos () {
        if (showBoundsGizmo) {
            Gizmos.color = boundsGizmoCol;

            List<Chunk> chunks = (this.chunks == null) ? new List<Chunk> (FindObjectsOfType<Chunk> ()) : this.chunks;
            foreach (var chunk in chunks) {
                Bounds bounds = new Bounds (CentreFromCoord (chunk.coord), Vector3.one * boundsSize);
                Gizmos.color = boundsGizmoCol;
                Gizmos.DrawWireCube (CentreFromCoord (chunk.coord), Vector3.one * boundsSize);
            }
        }
    }

}