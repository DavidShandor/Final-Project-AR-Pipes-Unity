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

    public void LoadList(string _act)
    {
        _action = _act;
        SwitchScenes("LoadList");
    }
}
