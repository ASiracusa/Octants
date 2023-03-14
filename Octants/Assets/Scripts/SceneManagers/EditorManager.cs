using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : MonoBehaviour
{
    private Vector3Int levelDims;
    private Vector3Int[] groundSpaces;

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

    void Start()
    {
        // Creates a generic floor out of ground tiles
        levelDims = new Vector3Int(15, 9, 15);
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
        Vector3 zeroCorner = -0.5f * (Vector3)(levelDims - new Vector3Int(1, 1, 1));
        foreach (Vector3Int gsPos in groundSpaces) {
            GameObject gs = Instantiate(gsPrefab);
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
        if (Input.GetKey(KeyCode.Q)) {
            cameraTargetRot += 0.05f;
        }
        if (Input.GetKey(KeyCode.E)) {
            cameraTargetRot -= 0.05f;
        }
        cameraCurrentRot = Mathf.Lerp(cameraCurrentRot, cameraTargetRot, 0.1f);
        Camera.main.transform.position = new Vector3(10 * Mathf.Sin(cameraCurrentRot), 5, 10 * Mathf.Cos(cameraCurrentRot));
        Camera.main.transform.eulerAngles = new Vector3(30, 180 + 360 * cameraCurrentRot / (2 * Mathf.PI), 0);
    }
}
