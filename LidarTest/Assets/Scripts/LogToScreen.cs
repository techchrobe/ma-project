using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogToScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    Queue myLogQueue = new Queue();

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string newString = "\n [" + type + "] : " + logString;
        myLogQueue.Enqueue(newString);
        if (type == LogType.Exception)
        {
            newString = "\n" + stackTrace;
            myLogQueue.Enqueue(newString);
        }
        text.text = "";
        foreach (string log in myLogQueue)
        {
            text.text += log;
        }
        if(myLogQueue.Count > 10)
        {
            myLogQueue.Dequeue();
        }
    }

}
