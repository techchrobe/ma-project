using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject obj;
    [SerializeField] GameObject player;
    [SerializeField] GameObject arCam;
    [SerializeField] ARMeshManager meshManager;
    [SerializeField] GameObject playUI;
    [SerializeField] GameObject scanUI;
    [SerializeField] List<GameObject> level;
    [SerializeField] bool debug = false;

    private List<GameObject> centers = new List<GameObject>();

    private void Start()
    {
        playUI.SetActive(false);
    }

    public void ResetPlayer()
    {
        player.GetComponent<PlayerControlls>().Reset();
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = arCam.transform.position + arCam.transform.forward;
        Debug.Log("Player reset");
        player.GetComponent<CharacterController>().enabled = true;
    }

    public void ToggleMesh()
    {
        meshManager.DestroyAllMeshes();
        meshManager.enabled = !meshManager.enabled;
    }

    public void SaveMesh()
    {
        foreach (GameObject i in centers)
        {
            Destroy(i);
        }

        meshManager.enabled = !meshManager.enabled;

        foreach (MeshFilter i in meshManager.meshes)
        {
            GameObject instObj = Instantiate(obj, i.mesh.bounds.center, transform.rotation);
            centers.Add(instObj);
            if (!debug) instObj.GetComponent<MeshRenderer>().enabled = false;
        }

        Bounds bounds = new Bounds(centers[0].transform.position, Vector3.zero);
        for (int i = 1; i < centers.Count; i++)
        {
            bounds.Encapsulate(centers[i].transform.position);
        }
        centers.Add(Instantiate(level[Random.Range(0, level.Count-1)], new Vector3(bounds.center.x, bounds.min.y, bounds.center.z), new Quaternion(0, arCam.transform.rotation.y, 0, arCam.transform.rotation.w)));

        ResetPlayer();
        playUI.SetActive(true);
        scanUI.SetActive(false);
    }
}
