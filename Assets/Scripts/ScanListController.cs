using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class ScanListController : MonoBehaviour
{
    [SerializeField] GameObject ScanBtnPref, Instruction;
    [SerializeField] Transform ScanBtnParent;
    [SerializeField] Text title;

    private string _action;

    [HideInInspector] public static ARLineMenifest lineMenifest;

    private void OnEnable()
    {
       _action = SwitchScene._action;
    }
    void Start()
    {
        title.gameObject.SetActive(false);
        LoadScanButtons();
        Instruction.gameObject.SetActive(false);
    }

    private void LoadScanButtons()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Scans"))
        {
            NoList();
            return;
        }

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

    public void OnScanButtonClicked(FileInfo file)
    {
        switch (_action) { 
            case "Delete": deleteFileRoutine(file);
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

    private void deleteFileRoutine(FileInfo file)
    {
        Instruction.gameObject.SetActive(true);
    }

    public void OnConfirmPress()
    {
     //TODO: DELETE THE FILE  
    }

    public void OnCanclePress()
    {
        Instruction.gameObject.SetActive(false);
    }

    private void NoList()
    {
        ScrollRect ScrollView = GetComponent<ScrollRect>();
        ScrollView.gameObject.SetActive(false);
        title.gameObject.SetActive(true);
    }
}
