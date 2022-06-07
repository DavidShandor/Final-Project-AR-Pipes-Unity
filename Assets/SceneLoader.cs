using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    public Animator transition;
    [Tooltip("Transition Time")]
    public float transitionTime = 1f;
    // Update is called once per frame
    void Update()
    {
        //StartCoroutine(Loadlevel(0));
    }

    IEnumerator Loadlevel(string scene)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(scene);

    }
}
