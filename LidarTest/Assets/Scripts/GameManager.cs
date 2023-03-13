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
    [SerializeField] LevelGenerator generator;

    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }


    [SerializeField] static bool debug = false;
    private bool scanning = true;
    public bool Scanning { get => scanning; }
    public bool GetDebug()
    {
        return debug;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        playUI.SetActive(false);
        generator.Init(arCam, meshManager, player.GetComponent<PlayerControlls>().JumpHeight);
        player.SetActive(false);
    }

    public void ResetPlayer()
    {
        player.GetComponent<PlayerControlls>().Reset();
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = generator.StartPosition;
        Debug.Log("Player reset");
        player.GetComponent<CharacterController>().enabled = true;
    }

    public void ToggleMesh()
    {
        meshManager.DestroyAllMeshes();
        meshManager.enabled = !meshManager.enabled;
    }

    public void PlaceObject()
    {
        Instantiate(obj, arCam.transform.position + arCam.transform.forward * 2, transform.rotation);
    }

    public void SaveMesh()
    {
        AROcclusionManager occlusionManager = arCam.GetComponent<AROcclusionManager>();
        if (occlusionManager != null) occlusionManager.enabled = true;
        meshManager.enabled = !meshManager.enabled;
        generator.GenerateLevel();
        player.SetActive(true);
        ResetPlayer();
        playUI.SetActive(true);
        scanUI.SetActive(false);
        scanning = false;
    }
}
