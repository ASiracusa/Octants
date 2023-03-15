using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : MonoBehaviour
{
    private Vector3Int levelDims;
    private GameObject[,,] groundGos;
    private Vector3 zeroCorner;

    private Vector3Int playerSpace;
    private int playerColor;
    private int playerDir;

    private int sigilCount;
    private List<Vector3Int> sigilSpaces;
    private List<int> sigilColors;

    private int reflectorCount;
    private List<Vector3Int> reflectorSpaces;
    private List<Vector2Int> reflectorColors;
    private List<bool[]> reflectorDims;

    private float cameraCurrentRot = 0;
    private float cameraTargetRot = 0;

    private GameObject gsPrefab;
    private Material evenMat;
    private Material oddMat;
    private Transform groundParent;
    
    private GameObject cursorCube;
    private Vector3Int cursorPos;
    private bool selecting;
    private bool leftSelect;
    private Vector3Int selectPos;

    void Start()
    {
        cursorCube = GameObject.Find("CursorCube");

        // Creates a generic floor out of ground tiles
        levelDims = new Vector3Int(15, 9, 15);
        zeroCorner = -0.5f * (Vector3)(levelDims - new Vector3Int(1, 1, 1));
        groundGos = new GameObject[levelDims.x, levelDims.y, levelDims.z];

        // Spawns the floor
        gsPrefab = (GameObject)Resources.Load("Prefabs/LevelElements/GroundTile");
        evenMat = (Material)Resources.Load("Materials/EvenGround");
        oddMat = (Material)Resources.Load("Materials/OddGround");
        groundParent = GameObject.Find("LevelElements/GroundElements").transform;
        for (int x = 0; x < levelDims.x; x++) {
            for (int z = 0; z < levelDims.z; z++) {
                GameObject gs = Instantiate(gsPrefab, groundParent);
                groundGos[x,0,z] = gs;
                gs.name = "GroundElement";
                gs.transform.position = zeroCorner + new Vector3(x, 0, z);
                if ((x + z) % 2 == 1) {
                    gs.GetComponent<MeshRenderer>().material = oddMat;
                } else {
                    gs.GetComponent<MeshRenderer>().material = evenMat;
                }
            }
        }
    }

    void FixedUpdate()
    {
        // Camera rotation
        if (Input.GetKey(KeyCode.Q)) {
            cameraTargetRot += 0.05f;
        }
        if (Input.GetKey(KeyCode.E)) {
            cameraTargetRot -= 0.05f;
        }
        cameraCurrentRot = Mathf.Lerp(cameraCurrentRot, cameraTargetRot, 0.1f);
        Camera.main.transform.position = new Vector3(10 * Mathf.Sin(cameraCurrentRot), 5, 10 * Mathf.Cos(cameraCurrentRot));
        Camera.main.transform.eulerAngles = new Vector3(30, 180 + 360 * cameraCurrentRot / (2 * Mathf.PI), 0);

        // Cursor controls
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit cursorHit;
        if (Physics.Raycast(cameraRay, out cursorHit, 100)) {
            cursorCube.SetActive(true);
            if (cursorHit.transform.gameObject.layer == 0) {
                cursorPos = Vector3Int.FloorToInt(cursorHit.transform.position - zeroCorner);
                if (!Input.GetKey(KeyCode.LeftShift)) {
                    cursorPos += DirectionFromPosition(cursorHit.point, cursorHit.transform.position);
                }
                cursorCube.transform.position = cursorPos + zeroCorner;
            }
        } else {
            cursorCube.SetActive(false);
            cursorPos = -1 * Vector3Int.one;
        }
    }

    void Update() {

        // Adding and removing ground tiles
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && cursorCube.activeSelf) {
            selecting = true;
            leftSelect = Input.GetMouseButtonDown(0);
            selectPos = cursorPos;
        }
        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && cursorCube.activeSelf && selecting) {
            selecting = false;
            Tuple<Vector3Int, Vector3Int> corners = MinMaxVectors(selectPos, cursorPos);
            for (int x = corners.Item1.x; x <= corners.Item2.x; x++) {
                for (int y = corners.Item1.y; y <= corners.Item2.y; y++) {
                    for (int z = corners.Item1.z; z <= corners.Item2.z; z++) {
                        if (leftSelect && groundGos[x,y,z] == null) {
                            GameObject gs = Instantiate(gsPrefab, groundParent);
                            groundGos[x,y,z] = gs;
                            gs.name = "GroundElement";
                            gs.transform.position = zeroCorner + new Vector3(x, y, z);
                            if ((x + y + z) % 2 == 1) {
                                gs.GetComponent<MeshRenderer>().material = oddMat;
                            } else {
                                gs.GetComponent<MeshRenderer>().material = evenMat;
                            }
                        } else if (!leftSelect && groundGos[x,y,z] != null) {
                            Destroy(groundGos[x,y,z]);
                            groundGos[x,y,z] = null;
                        }
                    }
                }
            }
        }

    }

    private Vector3Int DirectionFromPosition (Vector3 point, Vector3 home) {
        Vector3 diff = point - home;
        Vector3 mags = new Vector3(Mathf.Abs(diff.x), Mathf.Abs(diff.y), Mathf.Abs(diff.z));
        if (mags.x > mags.y && mags.x > mags.z) {
            if (diff.x > 0) {
                return new Vector3Int(1, 0, 0);
            } else {
                return new Vector3Int(-1, 0, 0);
            }
        }
        if (mags.y > mags.x && mags.y > mags.z) {
            if (diff.y > 0) {
                return new Vector3Int(0, 1, 0);
            } else {
                return new Vector3Int(0, -1, 0);
            }
        }
        if (mags.z > mags.x && mags.z > mags.y) {
            if (diff.z > 0) {
                return new Vector3Int(0, 0, 1);
            } else {
                return new Vector3Int(0, 0, -1);
            }
        }
        return Vector3Int.zero;
    }

    private Tuple<Vector3Int, Vector3Int> MinMaxVectors (Vector3Int a, Vector3Int b) {
        return Tuple.Create(new Vector3Int(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z)),
                            new Vector3Int(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z))); 
    }
}