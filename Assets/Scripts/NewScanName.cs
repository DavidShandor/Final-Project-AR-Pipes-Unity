using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewScanName : MonoBehaviour 
{
    [SerializeField] private InputField input;

    [SerializeField] private Button start;
    
    public static string SceneName;

    private void Awake()
    {
        start.gameObject.SetActive(false);
    }
    public void onValueChange()
    {
        if (input.text == "")
        {
            start.gameObject.SetActive(false);
        }
        else
        {
            start.gameObject.SetActive(true);
        }
    }

    public void OnPressStart()
    {
        //TODO: Validate the input?
        SceneName = input.text;
        input.text = "";
        SceneManager.LoadScene("NewScan");
    }
  
}
