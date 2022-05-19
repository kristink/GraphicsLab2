using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class MarchingCubeGrid : MonoBehaviour
{
    Mesh mesh;

    private MarchTables marchTable = new MarchTables();
    private Vector3[] cubePos = new Vector3[8] {
        new Vector3(0,0,0),
        new Vector3(1,0,0),
        new Vector3(1,0,1),
        new Vector3(0,0,1),
        new Vector3(0,1,0),
        new Vector3(1,1,0),
        new Vector3(1,1,1),
        new Vector3(0,1,1)
    };
    [SerializeField]
    private List<Vector3> vertices = new List<Vector3>();

    [SerializeField]
    private List<int> tri = new List<int>();

    [SerializeField]
    private Vector3 gridSize = new Vector3(10, 10, 10);

    private List<GameObject> pointObjects = new List<GameObject>();
    
    private Dictionary<Vector3, float> points = new Dictionary<Vector3, float>();
    
    public float surfaceLevel = 0;

    [SerializeField]
    public float offset = 0f;

    public Slider SurfaceLevelSlider;
    public Slider OffsetSlider;
    public Slider GridsizeXSlider;
    public Slider GridsizeYSlider;
    public Slider GridsizeZSlider;
    public Text timerText;

    public Transform CamTarget;
    public Transform GridArea;
    private float startTime = 0f;

    private Vector2 valueRange = new Vector2(-10, 10);
    private bool useInterpolation = false;

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CamTarget.position = new Vector3(gridSize.x / 2, gridSize.y / 2, gridSize.z / 2);
        GridArea.localScale = new Vector3(gridSize.x, gridSize.y, gridSize.z);

        GeneratePoints();
        StartCoroutine(CreateMeshInstant(0.02f));
    }    

    private void GeneratePoints()
    {
        vertices = new List<Vector3>();
        tri = new List<int>();
        points = new Dictionary<Vector3, float>();

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    float value = PerlinNoise3D((float)x / gridSize.x + offset, (float)y / gridSize.y + offset, (float)z / gridSize.z + offset);
                    points.Add(new Vector3(x, y, z), Remap(value, 0, 1, valueRange.x, valueRange.y));
                }
            }
        }
    }



    private IEnumerator CreateMeshInstant(float waitTime)
    {
        for (int x = 0; x < gridSize.x- 1; x++)
        {
            for (int y = 0; y < gridSize.y- 1; y++)
            {
                for (int z = 0; z < gridSize.z- 1; z++)
                {
                    CalculateTriangluationForGivenVoxelPos(new Vector3(x, y, z));
                }
            }
        }
        UpdateMesh();
        timerText.text = (Time.realtimeSinceStartup - startTime).ToString();
        yield return new WaitForSeconds(waitTime);
    }

    private void CalculateTriangluationForGivenVoxelPos(Vector3 pos)
    {
        int cubeIndex = GetCubeIndex(pos);
        for (int i = 0; i < 16; i += 3)
        {
            if (marchTable.triangulation[cubeIndex, i] < 0) break;
            int edgeIndex1 = marchTable.triangulation[cubeIndex, i];
            int edgeIndex2 = marchTable.triangulation[cubeIndex, i + 1];
            int edgeIndex3 = marchTable.triangulation[cubeIndex, i + 2];

            int a0 = marchTable.cornerIndexAFromEdge[edgeIndex1];
            int a1 = marchTable.cornerIndexBFromEdge[edgeIndex1];

            int b0 = marchTable.cornerIndexAFromEdge[edgeIndex2];
            int b1 = marchTable.cornerIndexBFromEdge[edgeIndex2];

            int c0 = marchTable.cornerIndexAFromEdge[edgeIndex3];
            int c1 = marchTable.cornerIndexBFromEdge[edgeIndex3];

            Vector3 vertexPos1;
            Vector3 vertexPos2;
            Vector3 vertexPos3;

            if (useInterpolation)
            {
                vertexPos1 = calculateVertexPosition(cubePos[a0] + pos, cubePos[a1] + pos);
                vertexPos2 = calculateVertexPosition(cubePos[b0] + pos, cubePos[b1] + pos);
                vertexPos3 = calculateVertexPosition(cubePos[c0] + pos, cubePos[c1] + pos);
            }
            else
            {
                vertexPos1 = (cubePos[a0] + cubePos[a1]) / 2 + pos;
                vertexPos2 = (cubePos[b0] + cubePos[b1]) / 2 + pos;
                vertexPos3 = (cubePos[c0] + cubePos[c1]) / 2 + pos;
            }

            vertices.Add(vertexPos1);
            vertices.Add(vertexPos2);
            vertices.Add(vertexPos3);
            tri.Add(vertices.Count - 3);
            tri.Add(vertices.Count - 2);
            tri.Add(vertices.Count - 1);
        }
    }

    private Vector3 calculateVertexPosition(Vector3 pos1, Vector3 pos2)
    {
        float valueA = points[pos1];
        float valueB = points[pos2];
        float t = (0 - valueA) / (valueB - valueA);
        print(valueA + " : " + valueB);
        return pos1 + t * (pos2 - pos1);
    }

    private int GetCubeIndex(Vector3 pos)
    {
        int cubeIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (points[pos + cubePos[i]] < surfaceLevel)
            {
                cubeIndex |= 1 << i;
            }
        }
        return cubeIndex;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = tri.ToArray();

        mesh.RecalculateNormals();
    }

    public static float PerlinNoise3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float xz = Mathf.PerlinNoise(x, z);
        float yz = Mathf.PerlinNoise(y, z);
        float yx = Mathf.PerlinNoise(y, x);
        float zx = Mathf.PerlinNoise(z, x);
        float zy = Mathf.PerlinNoise(z, y);
        return (xy + xz + yz + yx + zx + zy) / 6;
    }
    public void ValueChangeCheck()
    {
        surfaceLevel = SurfaceLevelSlider.value;
        startTime = Time.realtimeSinceStartup;
        GeneratePoints();
        StartCoroutine(CreateMeshInstant(0.002f));
    }

    public void ValueChangeCheck2()
    {
        offset = OffsetSlider.value;
        startTime = Time.realtimeSinceStartup;
        GeneratePoints();
        StartCoroutine(CreateMeshInstant(0.002f));
    }

    public void ValueChangeGridsizeX()
    {
        gridSize.x = GridsizeXSlider.value;
        CamTarget.position = new Vector3(gridSize.x / 2, gridSize.y / 2, gridSize.z / 2);
        GridArea.localScale = new Vector3(gridSize.x, gridSize.y, gridSize.z);
        
        startTime = Time.realtimeSinceStartup;
        GeneratePoints();
        StartCoroutine(CreateMeshInstant(0.002f));
    }

    public void ValueChangeGridsizeY()
    {
        gridSize.y = GridsizeYSlider.value;
        CamTarget.position = new Vector3(gridSize.x / 2, gridSize.y / 2, gridSize.z / 2);
        GridArea.localScale = new Vector3(gridSize.x, gridSize.y, gridSize.z);
        
        startTime = Time.realtimeSinceStartup;
        GeneratePoints();
        StartCoroutine(CreateMeshInstant(0.002f));
    }

    public void ValueChangeGridsizeZ()
    {
        gridSize.z = GridsizeZSlider.value;
        CamTarget.position = new Vector3(gridSize.x / 2, gridSize.y / 2, gridSize.z / 2);
        GridArea.localScale = new Vector3(gridSize.x, gridSize.y, gridSize.z);
        
        startTime = Time.realtimeSinceStartup;
        GeneratePoints();
        StartCoroutine(CreateMeshInstant(0.002f));
    }

    public void UpdateInterpolationValue()
    {
        useInterpolation = !useInterpolation;

        startTime = Time.realtimeSinceStartup;
        GeneratePoints();
        StartCoroutine(CreateMeshInstant(0.002f));
    }

    public void ReturnToMainMenu()
    {

        SceneManager.LoadScene("MainMenu");
    }
}
