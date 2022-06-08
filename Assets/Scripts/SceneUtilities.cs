using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneUtilities : MonoBehaviour
{
    [HideInInspector]
    public static string _action;

    public Animator transition;

    [Tooltip("Transition Time")]
    public float transitionTime = 1f;

    IEnumerator LoadWithAnimation(string scene)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(scene);

    }
    public void SwitchScenes(string scene)
    {
        StartCoroutine(LoadWithAnimation(scene));
    }
    public void ExitApplication()
    {
        
        Application.Quit();
    }

    public void LoadList(string _act)
    {
        _action = _act;
        SwitchScenes("LoadList");
    }
    public void ResetScene(string _scene, bool _reset)
    {
        string s = _reset ? "Reset" : "Exit";
        Debug.Log($"Try to {s} scene");
        
        var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
        
        if (_reset)
        {
            AndroidMessage._ShowAndroidToastMessage("Resetting the scene, please wait");
            SceneManager.LoadScene(_scene);
        }
        else
        {
            SwitchScenes(_scene);
        }

        xrManagerSettings.InitializeLoaderSync();
    }
}
