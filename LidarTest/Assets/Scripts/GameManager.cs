using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject obj;
    [SerializeField] GameObject player;
    [SerializeField] GameObject arCam;
    [SerializeField] ARMeshManager meshManager;
    [SerializeField] LevelGenerator generator;
    [SerializeField] LevelGeneratorTest generatorTest;
    [SerializeField] GameObject tutorialScreen;
    [SerializeField] GameObject continueButton;
    [SerializeField] TextMeshProUGUI tutorialText;

    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    private List<float> cameraYPositions = new List<float>();
    private float timer = 0;
    private bool showTutorial = false;
    private bool readTutorial = false;


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
        generator.Init(arCam, meshManager);
        player.SetActive(false);
        continueButton.SetActive(false);
    }

    private void Update() {
        if(!showTutorial && meshManager.meshes.Count > 0)
        {
            tutorialText.text = "Move and rotate the device around to scan.";
            continueButton.SetActive(true);
            showTutorial = true;
        }
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
        if (!scanning)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        //AROcclusionManager occlusionManager = arCam.GetComponent<AROcclusionManager>();
        //if (occlusionManager != null) occlusionManager.enabled = true;
        meshManager.enabled = !meshManager.enabled;
        cameraYPositions.Sort();
        generator.MaxHeight = cameraYPositions[cameraYPositions.Count - 1];
        generator.GenerateLevel();
        player.SetActive(true);
        ResetPlayer();
        scanning = false;
        tutorialScreen.SetActive(false);
    }

    public void TutorialNext()
    {
        if (readTutorial)
        {
            tutorialScreen.SetActive(false);
        }
        else
        {
            tutorialText.text = "Press the 'Start' button when you're done scanning.";
            readTutorial = true;
        }
    }
}
