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
    [SerializeField] LevelGenerator generator;
    [SerializeField] LevelGeneratorTest generatorTest;

    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    private List<float> cameraYPositions = new List<float>();
    private float timer = 0;


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
        generator.Init(arCam, meshManager, player.GetComponent<PlayerControlls>().JumpHeight);
        player.SetActive(false);
    }

    private void Update() {
        timer += Time.deltaTime;
        if(timer > 2) {
            cameraYPositions.Add(arCam.transform.position.y);
            timer = 0;
        }
    }

    public void ResetPlayer()
    {
        player.GetComponent<PlayerControlls>().Reset();
        player.GetComponent<CharacterController>().enabled = false;
        if(generator != null)
            player.transform.position = generator.StartPosition;
        if(generatorTest != null)
            player.transform.position = generatorTest.startPosition.transform.position;
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
        //AROcclusionManager occlusionManager = arCam.GetComponent<AROcclusionManager>();
        //if (occlusionManager != null) occlusionManager.enabled = true;
        meshManager.enabled = !meshManager.enabled;
        cameraYPositions.Sort();
        generator.MaxHeight = cameraYPositions[cameraYPositions.Count - 1];
        generator.GenerateLevel();
        player.SetActive(true);
        ResetPlayer();
        scanning = false;
    }
}
