using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : MonoBehaviour
{
    private Vector3Int levelDims;
    private Vector3Int[] groundSpaces;
    private Vector3 zeroCorner;

    private Vector3Int playerSpace;
    private int playerColor;
    private int playerDir;

    private int sigilCount;
    private Vector3Int[] sigilSpaces;
    private int[] sigilColors;

    private int reflectorCount;
    private Vector3Int[] reflectorSpaces;
    private Vector2Int[] reflectorColors;
    private bool[][] reflectorDims;

    private float cameraCurrentRot = 0;
    private float cameraTargetRot = 0;
    
    private GameObject cursorCube;

    void Start()
    {
        cursorCube = GameObject.Find("CursorCube");

        // Creates a generic floor out of ground tiles
        levelDims = new Vector3Int(15, 9, 15);
        zeroCorner = -0.5f * (Vector3)(levelDims - new Vector3Int(1, 1, 1));
        List<Vector3Int> grounds = new List<Vector3Int>();
        for (int x = 0; x < levelDims.x; x++) {
            for (int z = 0; z < levelDims.z; z++) {
                grounds.Add(new Vector3Int(x, 0, z));
            }
        }
        groundSpaces = grounds.ToArray();

        // Spawns the floor
        GameObject gsPrefab = (GameObject)Resources.Load("Prefabs/LevelElements/GroundTile");
        Material evenMat = (Material)Resources.Load("Materials/EvenGround");
        Material oddMat = (Material)Resources.Load("Materials/OddGround");
        Transform groundParent = GameObject.Find("LevelElements/GroundElements").transform;
        foreach (Vector3Int gsPos in groundSpaces) {
            GameObject gs = Instantiate(gsPrefab, groundParent);
            gs.name = "GroundElement";
            gs.transform.position = zeroCorner + gsPos;
            if ((gsPos.x + gsPos.y + gsPos.z) % 2 == 1) {
                gs.GetComponent<MeshRenderer>().material = oddMat;
            } else {
                gs.GetComponent<MeshRenderer>().material = evenMat;
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
                Vector3Int cursorPos = Vector3Int.FloorToInt(cursorHit.transform.position - zeroCorner);
                if (!Input.GetKey(KeyCode.LeftShift)) {
                    cursorPos += DirectionFromPosition(cursorHit.point, cursorHit.transform.position);
                }
                cursorCube.transform.position = cursorPos + zeroCorner;
            }
        } else {
            cursorCube.SetActive(false);
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
}
