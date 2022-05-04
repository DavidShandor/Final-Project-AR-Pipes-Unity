using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public void SwitchScenes(string scene)
    {
        //TODO: Check how to reset scene
        //SceneManager.LoadScene(SceneManager.GetSceneByName(scene).buildIndex);
        SceneManager.LoadScene(scene);
    }
}
