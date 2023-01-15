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

    private List<GameObject> centers = new List<GameObject>();

    private int counter = 0;

    public void InstantiateObject() {
        Instantiate(obj, arCam.transform.position + arCam.transform.forward * 2, transform.rotation);
        Debug.Log(counter);
        counter++;
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
        foreach(GameObject i in centers)
        {
            GameObject.Destroy(i);
        }
        meshManager.enabled = !meshManager.enabled;

        foreach(MeshFilter i in meshManager.meshes)
        {
            centers.Add(Instantiate(obj, i.mesh.bounds.center, transform.rotation));
        }
    }
}
