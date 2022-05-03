using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;


public class ScanListController : MonoBehaviour
{
    [SerializeField] GameObject ScanBtnPref;
    [SerializeField] Transform ScanBtnParent;
    [SerializeField] Text title;

    [HideInInspector] public static ARLineMenifest lineMenifest;
    void Start()
    {
        title.gameObject.SetActive(false);
        LoadScanButtons();
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
        //Debug.Log(file);
        string data = File.ReadAllText(file.FullName);
        Debug.Log(data);
        lineMenifest = JsonUtility.FromJson<ARLineMenifest>(data);

        SceneManager.LoadScene("Load");
    }


    private void NoList()
    {
        ScrollRect ScrollView = GetComponent<ScrollRect>();
        ScrollView.gameObject.SetActive(false);
        title.gameObject.SetActive(true);
    }
}
