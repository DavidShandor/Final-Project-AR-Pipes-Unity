using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    [HideInInspector]
    public static string _action;
    public void SwitchScenes(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void LoadDelete()
    {
        _action = "Delete";
        SwitchScenes("LoadList");
    }
    public void LoadList()
    {
        _action = "Load";
        SwitchScenes("LoadList");
    }
}
