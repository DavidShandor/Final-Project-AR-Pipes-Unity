using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class ScanListController : MonoBehaviour
{
    [SerializeField] private GameObject ScanBtnPref, DeleteAlert;
    [SerializeField] private Transform ScanBtnParent;
    [SerializeField] private Text title, _scanName;
    [SerializeField] private Button btnCancel, btnConfirm;
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
        DeleteAlert.gameObject.SetActive(false);

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
        switch (_action) { 
            case "Delete": deleteFileRoutine(file, obj);
                break;
            case "Load": LoadScan(file);
                break;

            default: break;
            }
    }

    private void LoadScan(FileInfo file)
    {
        string data = File.ReadAllText(file.FullName);
        lineMenifest = JsonUtility.FromJson<ARLineMenifest>(data);
        SceneManager.LoadScene("Load");
    }

    private void deleteFileRoutine(FileInfo file, ScanButtonItem _obj)
    {
        Debug.Log("Try to Delete File");
        _scanName.text = file.Name;
        _file = file;
        toDestroy = _obj.gameObject;
        DeleteAlert.SetActive(true);
    }

    
    public void OnConfirmPress()
    {
        try
        {
            _file.Delete();
            Destroy(toDestroy);
            DeleteAlert.SetActive(false);      
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    
    public void OnCanclePress()
    {
        DeleteAlert.SetActive(false);
    }

    private void NoList()
    {
        ScrollRect ScrollView = GetComponent<ScrollRect>();
        ScrollView.gameObject.SetActive(false);
        title.gameObject.SetActive(true);
    }
}
