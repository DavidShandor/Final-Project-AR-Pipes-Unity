using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Scans list button class
/// </summary>
public class ScanButtonItem : MonoBehaviour
{
    [HideInInspector] public FileInfo file;
    [HideInInspector] public ScanListController scanListController;
    [SerializeField] Text fileName;


    private void Start()
    {
        fileName.text = file.Name;
    }
    public void OnScanButtonClick() { 
        scanListController.OnScanButtonClicked(file, this);
    }
}
