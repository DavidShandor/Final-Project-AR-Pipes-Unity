using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
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
        // Add here an alert "R U sure?" bla bla
        Application.Quit();
    }

    public void LoadList(string _act)
    {
        _action = _act;
        SwitchScenes("LoadList");
    }
}
