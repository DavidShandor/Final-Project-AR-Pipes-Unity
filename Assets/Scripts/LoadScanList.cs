using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadScanList :MonoBehaviour 
{

    
    public GameObject template;
    public TextMeshProUGUI title;
    public GameObject scrollView;


    private void Awake()
    {
        title.gameObject.SetActive(false);
        GetScanList();
    }

    public void GetScanList()
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
            GameObject go = Instantiate(template);
            go.GetComponentInChildren<Text>().text = f.Name;
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => GoToScene(f));
            go.gameObject.SetActive(true);
            go.transform.SetParent(scrollView.transform.parent, false);   
            
        }
    }

    // send data to the next scene.
    private void GoToScene(FileInfo file)
    {
        Debug.Log(file.Name);
        
    }
   
    private void NoList()
    {
        scrollView.gameObject.SetActive(false);
        title.gameObject.SetActive(true);
    }
}
