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
    [SerializeField] private InputField input;
    [SerializeField] private TextMeshProUGUI validateText;
    [SerializeField] private Button start;
    
    public static string SceneName;

    private void Awake()
    {
        start.gameObject.SetActive(false);
        validateText.gameObject.SetActive(false);
    }
    public void onValueChange()
    {
        if (input.text == "")
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
            else if(validationByName(input.text.Trim(' ')))
            {
                validateText.gameObject.SetActive(false);
                start.gameObject.SetActive(true);
            }
            else
            {
                validateText.text = "File name already exist";
                validateText.gameObject.SetActive(true);
            }
        }
    }

    //Validate the input.
    private bool validationByName(string fileName)
    {
        if (Directory.Exists(Application.persistentDataPath + "/Scans"))
        {   
            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath + "/Scans");
            FileInfo[] info = dir.GetFiles();

            foreach (FileInfo f in info)
            {
                if(f.Name == fileName) return false;
            }

           }
        return true;
    }

    public void OnPressStart()
    {
        //TODO: Validate the input?
        SceneName = input.text;
        input.text = "";
        SceneManager.LoadScene("NewScan");
    }
  
}
