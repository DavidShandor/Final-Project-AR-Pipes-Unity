using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Utilites to control transitions between scenes
/// </summary>
public class SceneUtilities : MonoBehaviour
{
    // Used to act on the Load List scene (delete of load scan)
    [HideInInspector]
    public static string _action;

    // Animation between scenes
    public Animator transition;
    [Tooltip("Transition Time")]
    public float transitionTime = 0.5f; // Animation duration

    /// <summary>
    /// Animation coroutine, used to control the animation duration and trigger.
    /// </summary>
    /// <param name="scene">Scene to load</param>
    /// <returns></returns>
    IEnumerator LoadWithAnimation(string scene)
    {
        transition.SetTrigger("Start"); // Trigger animation

        yield return new WaitForSeconds(transitionTime); // wait transitionTime seconds.

        SceneManager.LoadScene(scene); // Load Scene

    }

    /// <summary>
    /// Warpped method to call coroutine "LoadWithAnimation"
    /// </summary>
    /// <param name="scene">Scene name to load</param>
    public void SwitchScenes(string scene)
    {
        StartCoroutine(LoadWithAnimation(scene));
    }

    /// <summary>
    /// Exit application (called by the Yes button in the Exit Scene only)
    /// </summary>
    public void ExitApplication()
    {    
        Application.Quit();
    }

    /// <summary>
    /// Load "Load list" scene with information about the action to do with the list (Load or Delete scan)
    /// </summary>
    /// <param name="_act"></param>
    //public void LoadList(string _act)
    //{
    //    _action = _act;
    //    SwitchScenes("LoadList");
    //}

    /// <summary>
    /// Reload or exit scene, and clear all the XR data from the scene,
    /// include reset the space.
    /// </summary>
    /// <param name="_scene">Scene to be Load</param>
    /// <param name="_reset">Reset the scene?</param>
    public void ResetScene(string _scene, bool _reset)
    {
        //Debugging stuff
        string s = _reset ? "Reset" : "Exit";
        Debug.Log($"Try to {s} scene");
        
        // Scene instance manager,
        var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;

        // De-Initialize scene XR instance (reset the scene)
        xrManagerSettings.DeinitializeLoader(); 

        if (_reset)
        {
            AndroidMessage._ShowAndroidToastMessage("Resetting the scene, please wait");
            SceneManager.LoadScene(_scene); //Reload the current scene
        }
        else
        {
            SwitchScenes(_scene); // Swith to another scene (mainly Main Menu)
        }

        // Re-initialize scene XR instance.
        xrManagerSettings.InitializeLoaderSync();  
    }
}
