using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MiniLogger : MonoBehaviour
{
    public TMP_Text logText;

    void OnLogReceive(string condition, string stackTrace, LogType type)
    {
        // Add color tag by type
        switch (type)
        {
            case LogType.Error:
                condition = "<color=red>" + condition + "</color>";
                break;
            case LogType.Assert:
                condition = "<color=red>" + condition + "</color>";
                break;
            case LogType.Warning:
                condition = "<color=yellow>" + condition + "</color>";
                break;
            case LogType.Log:
                condition = "<color=white>" + condition + "</color>";
                break;
            case LogType.Exception:
                condition = "<color=red>" + condition + "</color>";
                break;
            default:
                break;
        }

        if (logText)
        {
            logText.text += condition + "\n";
        }
    }

    private void Awake()
    {
        if (logText)
        {
            logText.text = "";
        }
        Application.logMessageReceived += OnLogReceive;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLogReceive;
    }
}
