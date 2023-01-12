using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject obj;
    [SerializeField] GameObject arCam;
    private int counter = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InstantiateObject() {
        Instantiate(obj, arCam.transform.position + arCam.transform.forward * 2, transform.rotation);
        Debug.Log(counter);
        counter++;
    }
}
