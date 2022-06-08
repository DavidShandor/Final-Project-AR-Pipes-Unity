using System.IO;
using Unity.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;

public class NewScanARManager : MonoBehaviour
{
    private enum State : int
    {
        findDoor = 0,
        placeDoor = 1,
        placeLines = 2
    }

    // Editor Fields
    [SerializeField]
    private GameObject pointsPrefab, doorPrefab, linePrefab, HUD, MenuIcon, MenuPanel, Alert;
    [SerializeField] private Button okBTM;
    public TMPro.TextMeshProUGUI textMeshPro, alertText;

    private bool save;
    private Ray inputRay;
    private string SceneName;
    private new string tag = "line";
    private bool isFirstPoint = true;
    private Color color = Color.white;
    private ARPlaneManager planeManager;
    private ARSessionOrigin sessionOrigin;
    private State state = (State)SceneStage.ResetScene;
    private GameObject asset, startPoint, endPoint, door;
    private readonly List<GameObject> lines = new List<GameObject>();
    private readonly ARLineMenifest ScanMenifest = new ARLineMenifest();
    private NativeArray<XRRaycastHit> raycastHits = new NativeArray<XRRaycastHit>();
    [HideInInspector] public SceneUtilities sceneUtil;

    void Awake()
    {
        Debug.Log($"State is: {state}");
       
        if (state == State.placeDoor)
        {
            textMeshPro.text = "Tap the Upper-Left\nDoor edge to Place Cube Marker";
            okBTM.gameObject.SetActive(false);
        }
        sessionOrigin = GetComponent<ARSessionOrigin>();
        planeManager = GetComponent<ARPlaneManager>();

        SceneName = NewScanName.SceneName;

        sceneUtil = GameObject.FindGameObjectWithTag("Crossfade").GetComponent<SceneUtilities>();
    }

    private void Start()
    {
        HUD.SetActive(false);
        MenuIcon.SetActive(false);
        MenuPanel.SetActive(false);
        Alert.SetActive(false);

        startPoint = Instantiate(pointsPrefab, Vector3.zero, Quaternion.identity);
        endPoint = Instantiate(pointsPrefab, Vector3.zero, Quaternion.identity);
        door = Instantiate(doorPrefab, Vector3.zero, Quaternion.identity);

        startPoint.SetActive(false);
        endPoint.SetActive(false);
        door.SetActive(false);

        isFirstPoint = true;

    }


    // Update is called once per frame
    void Update()
    {
        if (state == State.placeDoor)
        {
            PlaceDoor();
        }
        else if (state == State.placeLines)
        {
            DetectTouch();
        }
    }

    public void OnOkPressed()
    {
        SceneStage.ResetScene = (int)State.placeDoor;
        sceneUtil.ResetScene("NewScan", true);
    }

    private void PlaceDoor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            inputRay = sessionOrigin.camera.ScreenPointToRay(Input.mousePosition);
            raycastHits = planeManager.Raycast(inputRay, TrackableType.All, Allocator.Temp);

            if (raycastHits.Length > 0)
            {
                Pose hitPose = raycastHits[0].pose;
                door.transform.SetPositionAndRotation(hitPose.position, Quaternion.identity);
                door.SetActive(true);
                state = State.placeLines;
                HUD.SetActive(true);
                MenuIcon.SetActive(true);
                textMeshPro.text = "Draw Pipes";
            }
            else
            {
                AndroidMessage._ShowAndroidToastMessage("Tap on plane only");
            }
        }
    }

    // This metod reset the scene and load by index
    //private void ResetScene(string _scene)
    //{
    //    var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
    //    xrManagerSettings.DeinitializeLoader();
    //    //SceneManager.LoadScene(_scene);
    //    SwitchScene switchScene = GameObject.FindGameObjectWithTag("Crossfade").GetComponent<SwitchScene>();
    //    switchScene.SwitchScenes(_scene);
    //    xrManagerSettings.InitializeLoaderSync();
    //}

    private void DetectTouch()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {   
            if (color == Color.white || tag == "line")
            {
                AndroidMessage._ShowAndroidToastMessage("Please choose pipe type first");
                return;
            }
            inputRay = sessionOrigin.camera.ScreenPointToRay(Input.mousePosition);
            raycastHits = planeManager.Raycast(inputRay, TrackableType.All, Allocator.Temp);

            if (raycastHits.Length > 0)
            {
                asset = isFirstPoint ? startPoint : endPoint;
                asset.SetActive(true);
                Pose hitPose = raycastHits[0].pose;
                asset.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);

                if (!isFirstPoint)
                {
                    DrawLine();
                    startPoint.SetActive(false);
                    endPoint.SetActive(false);
                }

                // Toggle the bool so next we will place the next point.
                isFirstPoint = !isFirstPoint;
            }
            else
            {
                AndroidMessage._ShowAndroidToastMessage("Tap on plane only");
            }

        }
    }

    private void DrawLine()
    {
        var DistanceVectors = CalcVectors(startPoint.transform.position,
                                          endPoint.transform.position,
                                          door.transform.position, out Vector3 mid);

        AddNewLine(DistanceVectors);
        
        GameObject newLine = Instantiate(linePrefab, mid, Quaternion.identity);

        SetLine(ref newLine);

        lines.Add(newLine);

        newLine.SetActive(true);  
    }

    private void SetLine(ref GameObject _lineobj)
    {
        LineRenderer lineRenderer = _lineobj.GetComponent<LineRenderer>();
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.SetPosition(0, startPoint.transform.position);
        lineRenderer.SetPosition(1, endPoint.transform.position);
        _lineobj.transform.parent = door.transform;
    }

    /// <summary>
    /// Create new line Descriptor and add it to the line container 
    /// </summary>
    /// <param name="distanceVectors">Tuple of position vectors (mid, start, end)</param>
    private void AddNewLine(Tuple<Vector3, Vector3, Vector3> distanceVectors)
    {
        ARLineDefinition definition = new ARLineDefinition(tag, color, distanceVectors);

        ScanMenifest.LineDefinitions.Add(definition);
    }

    /// <summary>
    /// Return the Vectors of all the position relative to door and scene, so next time 
    /// </summary>
    /// <param name="startPoint">Position of the start point</param>
    /// <param name="endPoint">Position of the end point</param>
    /// <param name="doorPoint">Position of the door</param>
    /// <param name="_mid"></param>
    /// <returns>1. Middle position between start point and end point<br/>
    ///          2. Middle point relative to door position.<br/>
    ///          3. Start point relative to door position.<br/>
    ///          4. End point relative to door position.</returns>
    private static Tuple<Vector3,Vector3,Vector3> CalcVectors(Vector3 startPoint, Vector3 endPoint, Vector3 doorPoint, out Vector3 _mid)
    {
        _mid = Vector3.Lerp(startPoint, endPoint, 0.5f);
        return new Tuple<Vector3, Vector3, Vector3>(doorPoint - _mid,
                                                    doorPoint - startPoint,
                                                    doorPoint - endPoint);
    }


    public void Save()
    {
        SceneStage.ResetScene = 0;

        if (!Directory.Exists(Application.persistentDataPath + "/Scans"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Scans");
        }
        string data = JsonUtility.ToJson(ScanMenifest);
        Debug.Log(data);

        File.WriteAllText(Application.persistentDataPath + $"/Scans/{SceneName}", data);
        SceneStage.ResetScene = (int)State.findDoor;
        AndroidMessage._ShowAndroidToastMessage("Scan Saved!");
    }

    

    public void SetLineAtt(int x)
    {
        switch (x)
        {
            case 0: // Electricity
                color = Color.yellow;
                tag = "Electricity";
                break;
            case 1: // Hot Water
                color = Color.red;
                tag = "Hot";
                break ;
            case 2: // Cold Water
                color = Color.blue;
                tag = "Cold";
                break;
            default:
                color = Color.white;
                tag = "Normal";
                break;
        }
    }

    public void OnSavePress()
    {
        alertText.text = "Save and Exit?";
        save = true;
        Alert.SetActive(true);
        MenuPanel.SetActive(false);
    }

    public void OnExitPress()
    {
        alertText.text = "Are You sure you want to exit to\nmain menu without saving?\n\nAll data will be lost.";
        save = false;
        Alert.SetActive(true);
        MenuPanel.SetActive(false);
    }

    public void OnMenuPress()
    {
        MenuPanel.SetActive(true);
    }
    public void OnExitMenuPress()
    {
        MenuPanel.SetActive(false);
    }

    public void OnConfirmPress()
    {
        if (save)
        {
             Save();
        }
        sceneUtil.ResetScene("MainMenu", false);
    }
    public void OnCancelPress()
    {
        Alert.SetActive(false);
        MenuPanel.SetActive(true);
    }


}
