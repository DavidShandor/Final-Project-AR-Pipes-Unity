using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class ScanListController : MonoBehaviour
{
    [SerializeField] private GameObject ScanBtnPref, AlertMessage;
    [SerializeField] private Transform ScanBtnParent;
    [SerializeField] private Text title, _scanName, Message;
    //[SerializeField] private Button btnCancel, btnConfirm;
    [HideInInspector] public static ARLineMenifest lineMenifest;

    private FileInfo _file;
    private GameObject toDestroy;
    private string _action;
    
  
    private void OnEnable()
    {
       _action = SwitchScene._action;
        Debug.Log(_action);
    }
    void Start()
    {
        title.gameObject.SetActive(false);
        AlertMessage.gameObject.SetActive(false);

        if (!Directory.Exists(Application.persistentDataPath + "/Scans"))
        {
            NoList();
            return;
        }
        else
        {
            LoadScanButtons();
        }
    }

    private void LoadScanButtons()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath + "/Scans");
        FileInfo[] info = dir.GetFiles();

        if (info.Length == 0)
        {
            NoList();
            return;
        }

        foreach (FileInfo f in info)
        {
            GameObject ScanButtonObj = Instantiate(ScanBtnPref, ScanBtnParent) as GameObject;
            ScanButtonObj.GetComponent<ScanButtonItem>().file = f;
            ScanButtonObj.GetComponent<ScanButtonItem>().scanListController = this;
        }
    }

    public void OnScanButtonClicked(FileInfo file, ScanButtonItem obj)
    {
        _scanName.text = file.Name;
        _file = file;
        Message.text = $"Are you sure want to {_action} this scan?";
        toDestroy = obj.gameObject;
        AlertMessage.SetActive(true);
    }

    private void LoadScan()
    {
        string data = File.ReadAllText(_file.FullName);
        lineMenifest = JsonUtility.FromJson<ARLineMenifest>(data);
        SceneManager.LoadScene("Load");
    }

    private void deleteFileRoutine(FileInfo file, ScanButtonItem _obj)
    {
        Debug.Log("Try to Delete File");
        _scanName.text = file.Name;
        _file = file;
        toDestroy = _obj.gameObject;
        AlertMessage.SetActive(true);
    }

    
    public void OnConfirmPress()
    {
        switch (_action)
        {
            case "Delete":
                deleteFile();
                break;
            case "Load":
                LoadScan();
                break;

            default: break;
        }

    }

    private void deleteFile()
    {
        try
        {
            _file.Delete();
            Destroy(toDestroy);
            AlertMessage.SetActive(false);

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

    public void OnCancelPress()
    {
        AlertMessage.SetActive(false);
    }

    private void NoList()
    {
        ScrollRect ScrollView = GetComponent<ScrollRect>();
        ScrollView.gameObject.SetActive(false);
        title.gameObject.SetActive(true);
    }
}
