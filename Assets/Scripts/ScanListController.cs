using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

/// <summary>
/// Scan list Controller. Show and recieve press events from the buttons.
/// </summary>
public class ScanListController : MonoBehaviour
{
    // Editor fields
    [SerializeField] private GameObject ScanBtnPref, AlertMessage;
    [SerializeField] private Transform ScanBtnParent;
    [SerializeField] private Text title, _scanName, Message;

    // Variable to store data from file.
    [HideInInspector] public static ARLineMenifest lineMenifest;
    
    // Scene Util object reference.
    [HideInInspector] public SceneUtilities sceneUtile;

    // Variables and object reference.
    private FileInfo _file;
    private GameObject toDestroy;
    private string _action;
    
  
    // Called when the script is enable.
    private void OnEnable()
    {
       _action = SceneUtilities._action;
        Debug.Log(_action);
    }

    // Called in the first frame.
    void Start()
    {
        // Hide alert and message "no scan".
        title.gameObject.SetActive(false);
        AlertMessage.SetActive(false);

        if (!Directory.Exists(Application.persistentDataPath + "/Scans"))
        {
            NoList(); // No saved scans - show message.
            return;
        }
        else
        {
            // Load the buttons list.
            LoadScanButtons();
        }

        sceneUtile = GameObject.FindGameObjectWithTag("Crossfade").GetComponent<SceneUtilities>();
    }

    /// <summary>
    /// Show button corresponding to the saved scans.
    /// </summary>
    private void LoadScanButtons()
    {
        // Get all the saved file in the device memory
        DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath + "/Scans");
        FileInfo[] info = dir.GetFiles();

        // No files - show message.
        if (info.Length == 0)
        {
            NoList();
            return;
        }

        // populate each file as a button and attach file data and name to the file.
        foreach (FileInfo f in info)
        {
            GameObject ScanButtonObj = Instantiate(ScanBtnPref, ScanBtnParent) as GameObject;
            ScanButtonObj.GetComponent<ScanButtonItem>().file = f;
            ScanButtonObj.GetComponent<ScanButtonItem>().scanListController = this;
        }
    }

    /// <summary>
    /// Called when press the scan button and show alert depent the _action value
    /// _action = Delete or Load.
    /// </summary>
    /// <param name="file">Saved file</param>
    /// <param name="obj">Button gameobject (for delete)</param>
    public void OnScanButtonClicked(FileInfo file, ScanButtonItem obj)
    {
        _scanName.text = file.Name;
        _file = file;
        Message.text = $"Are you sure want to {_action} this scan?";
        toDestroy = obj.gameObject;
        AlertMessage.SetActive(true);
    }

    /// <summary>
    /// Read file data and store it in variable to transfer it to the next scene, then go to Load scene.
    /// </summary>
    private void LoadScan()
    {
        string data = File.ReadAllText(_file.FullName);
        lineMenifest = JsonUtility.FromJson<ARLineMenifest>(data);
        sceneUtile.SwitchScenes("Load");
    }

    /// <summary>
    /// Called when user press Confirm on the alert.
    /// </summary>
    public void OnConfirmPress()
    {
        switch (_action)
        {
            case "Delete":
                DeleteFile();
                break;
            case "Load":
                LoadScan();
                break;

            default: break;
        }

    }

    /// <summary>
    /// Delete file from the device memory and remove its object from the list. 
    /// </summary>
    private void DeleteFile()
    {
        try
        {
            _file.Delete();
            Destroy(toDestroy);
            AlertMessage.SetActive(false);

            // Check if there are more files to show, if not - show "No Scan" message.
            if (new DirectoryInfo(Application.persistentDataPath + "/Scans").GetFiles().Length == 0)
            {
                NoList();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    /// <summary>
    /// Called when user press Cancel on the alert popup.
    /// </summary>
    public void OnCancelPress()
    {
        AlertMessage.SetActive(false);
    }

    /// <summary>
    /// Hide ScrollView object and show "No Scan" message.
    /// </summary>
    private void NoList()
    {
        ScrollRect ScrollView = GetComponent<ScrollRect>();
        ScrollView.gameObject.SetActive(false);
        title.gameObject.SetActive(true);
    }
}
