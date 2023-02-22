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
    [SerializeField] GameObject goal;

    private List<GameObject> centers = new List<GameObject>();

    private Vector3 startPosition;
    private Vector3 endPosition;

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
        position.y = position.y - (DistanceToGround(position) - 0.1f);

        GameObject start = Instantiate(simplePlatform, position, transform.rotation);
        startPosition = start.transform.position + new Vector3(0, 0.2f, 0);

        // Place end postion
        GameObject farthestMesh = GetFarthest(startPosition, centers);
        endPosition = farthestMesh.transform.position;
        float ceilingDistance = DistanceToCeiling(endPosition);
        if(ceilingDistance < 0.7f)
        {
            endPosition.y = endPosition.y - (0.7f - ceilingDistance);
        }

        Instantiate(simplePlatform, endPosition, transform.rotation);
        Instantiate(goal, endPosition + new Vector3(0, 0.1f, 0), transform.rotation);


        // Place platforms between start and end position
        // TODO
    }

    private float DistanceToGround(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.SphereCast(position, 0.1f, Vector3.down, out hit))
        {
            Debug.Log(hit.distance);
            Debug.Log(hit.collider.tag);
            return hit.distance;
        }
        return float.MaxValue;
    }

    private float DistanceToCeiling(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.SphereCast(position, 0.1f, Vector3.up, out hit))
        {
            Debug.Log(hit.distance);
            Debug.Log(hit.collider.tag);
            return hit.distance;
        }
        return float.MaxValue;
    }

    private GameObject GetFarthest(Vector3 startPoint, List<GameObject> gos)
    {
        GameObject farthest = gos[0];
        float maxDistance = 0;
        foreach(GameObject i in gos)
        {
            float distance = Vector3.Distance(startPoint, i.transform.position);
            if(distance > maxDistance)
            {
                maxDistance = distance;
                farthest = i;
            }
        }
        return farthest;
    }
}
