using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class LevelGenerator : MonoBehaviour
{
    private GameObject arCam;
    private ARMeshManager meshManager;
    [SerializeField] GameObject simplePlatform;
    [SerializeField] GameObject debugObj;

    private List<GameObject> centers = new List<GameObject>();

    private Vector3 startPosition;

    public Vector3 StartPosition { get => startPosition; }

    public void Init(GameObject cam, ARMeshManager manager)
    {
        arCam = cam;
        meshManager = manager;
    }

    public void GenerateLevel()
    {
        foreach (MeshFilter i in meshManager.meshes)
        {
            GameObject instObj = Instantiate(debugObj, i.mesh.bounds.center, transform.rotation);
            centers.Add(instObj);
            if (!GameManager.Instance.GetDebug()) instObj.GetComponent<MeshRenderer>().enabled = false;
        }

        Bounds bounds = new Bounds(centers[0].transform.position, Vector3.zero);
        for (int i = 1; i < centers.Count; i++)
        {
            bounds.Encapsulate(centers[i].transform.position);
        }
        //centers.Add(Instantiate(level[Random.Range(0, level.Count-1)], new Vector3(bounds.center.x, bounds.min.y, bounds.center.z), new Quaternion(0, arCam.transform.rotation.y, 0, arCam.transform.rotation.w)));

        // Place start position
        Vector3 position = arCam.transform.position + arCam.transform.forward;
        RaycastHit hit;
        if(Physics.SphereCast(position, 0.1f, Vector3.down, out hit))
        {
            Debug.Log(hit.distance);
            Debug.Log(hit.collider.tag);
            position.y = position.y - (hit.distance - 0.1f);
        }

        GameObject start = Instantiate(simplePlatform, position, transform.rotation);
        startPosition = start.transform.position + new Vector3(0, 0.2f, 0);

        // Place end postion
        // TODO

        // Place platforms between start and end position
        // TODO
    }
}
