using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;
using System.Linq;

public class NewScanName : MonoBehaviour 
{
    // Editor fields
    [SerializeField] private InputField input;
    [SerializeField] private TextMeshProUGUI validateText;
    [SerializeField] private Button start;

    [HideInInspector]
    public SceneUtilities switchScene;

    // Global variable to transfer data between scenes
    public static string SceneName;

    // Awake is called once in the very beginning of the scene.
    private void Awake()
    {
        //Hide start button and alert text.
        start.gameObject.SetActive(false);
        validateText.gameObject.SetActive(false);

        switchScene = GameObject.FindGameObjectWithTag("Crossfade").GetComponent<SceneUtilities>();
    }

    /// <summary>
    /// Called every time the input change.
    /// </summary>
    public void OnValueChange()
    {
        if (input.text == "") // Empty input - hide start and alert.
        {
            start.gameObject.SetActive(false);
            validateText.gameObject.SetActive(false);
        }
        else
        {
            // Validate the input to contain letter or digit only
            if (input.text.Any(c => !char.IsLetterOrDigit(c)))
            {
                validateText.text = "Only Letters and Digits allowed.";
                validateText.gameObject.SetActive(true);
                start.gameObject.SetActive(false);
            }
            // Validate the input.
            else if(ValidationByName(input.text.Trim(' ')))
            {
                validateText.gameObject.SetActive(false);
                start.gameObject.SetActive(true);
            }
            else // File name already exist.
            {   
                validateText.text = "File name already exist";
                validateText.gameObject.SetActive(true);
                start.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Check if file name already exist or not.
    /// </summary>
    /// <param name="fileName">File name to check</param>
    /// <returns></returns>
    private bool ValidationByName(string fileName)
    {   
        // Check if Scans directory exist.
        if (Directory.Exists(Application.persistentDataPath + "/Scans"))
        {   
            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath + "/Scans");
            FileInfo[] info = dir.GetFiles();

            // Check each file name
            foreach (FileInfo f in info)
            {
                if(f.Name == fileName) return false;
            }

        }
        return true;
    }

    /// <summary>
    /// Called when the user press Start and start New Scan scene.
    /// </summary>
    public void OnPressStart()
    {
        SceneName = input.text;
        input.text = "";
        switchScene.SwitchScenes("NewScan");
        //SceneManager.LoadScene("NewScan");
    }
  
}
