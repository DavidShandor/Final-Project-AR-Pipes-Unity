using System.IO;
using Unity.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;

public class NewScanARManager : MonoBehaviour
{
    /// <summary>
    /// State to maintance the scene flow
    /// </summary>
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
    public TextMeshProUGUI textMeshPro, alertText;

    // Variables and Objects Reference.
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
    private readonly ARLineMenifest ScanMenifest = new ARLineMenifest();
    private NativeArray<XRRaycastHit> raycastHits = new NativeArray<XRRaycastHit>();
    [HideInInspector] public SceneUtilities sceneUtil;

    // Awake is called once in the very beginning of the scene.
    void Awake()
    {
        Debug.Log($"State is: {state}");

        // If State == findDoor - need to reset scene, otherwise, place door.
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

    // Start is called once in the first frame.
    private void Start()
    {
        //Hide UI
        HUD.SetActive(false);
        MenuIcon.SetActive(false);
        MenuPanel.SetActive(false);
        Alert.SetActive(false);

        //Instantiate objects and hide them
        startPoint = Instantiate(pointsPrefab, Vector3.zero, Quaternion.identity);
        endPoint = Instantiate(pointsPrefab, Vector3.zero, Quaternion.identity);
        door = Instantiate(doorPrefab, Vector3.zero, Quaternion.identity);

        startPoint.SetActive(false);
        endPoint.SetActive(false);
        door.SetActive(false);

        // This boolean used to switch pointer asset (between start and end points).
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

    /// <summary>
    /// Called when user press OK to reset the scene.
    /// </summary>
    public void OnOkPressed()
    {
        SceneStage.ResetScene = (int)State.placeDoor;
        sceneUtil.ResetScene("NewScan", true);
    }

    /// <summary>
    /// Position door and show UI and tools to the user.
    /// </summary>
    private void PlaceDoor()
    {
        if (Input.GetMouseButtonDown(0)) // Detect touch
        {
            inputRay = sessionOrigin.camera.ScreenPointToRay(Input.mousePosition);
            raycastHits = planeManager.Raycast(inputRay, TrackableType.All, Allocator.Temp);

            if (raycastHits.Length > 0) // Touch on a plane.
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

    /// <summary>
    /// Detect touch on the screen, and place point or pipe (depend isFirstPoint flag)
    /// </summary>
    private void DetectTouch()
    {
        // Detect Touch and check if not on another pipe or button/UI.
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {   
            // User must choose type of pipe before place it.
            if (color == Color.white || tag == "line")
            {
                AndroidMessage._ShowAndroidToastMessage("Please choose pipe type first");
                return;
            }
            inputRay = sessionOrigin.camera.ScreenPointToRay(Input.mousePosition);
            raycastHits = planeManager.Raycast(inputRay, TrackableType.All, Allocator.Temp);

            if (raycastHits.Length > 0) // Touch on a plane.
            {
                asset = isFirstPoint ? startPoint : endPoint;
                asset.SetActive(true);
                Pose hitPose = raycastHits[0].pose;
                asset.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);

                if (!isFirstPoint) // End point - Draw line and hide points.
                {
                    DrawLine();
                    startPoint.SetActive(false);
                    endPoint.SetActive(false);
                }

                // Toggle the bool so next we will place the other point.
                isFirstPoint = !isFirstPoint;
            }
            else
            {
                AndroidMessage._ShowAndroidToastMessage("Tap on plane only");
            }

        }
    }

    /// <summary>
    /// Draw pipe and save it data temporarly. 
    /// </summary>
    private void DrawLine()
    {
        // Calculate the middle, start and end of the pipe relative to the door position.
        var DistanceVectors = CalcVectors(startPoint.transform.position,
                                          endPoint.transform.position,
                                          door.transform.position, out Vector3 mid);

        // Save pipe data (positions, tag and color) to Menifest 
        AddNewLine(DistanceVectors);
        
        // Instantiate the new pipe.
        GameObject newLine = Instantiate(linePrefab, mid, Quaternion.identity);

        // Set new pipe attributes.
        SetLine(ref newLine);

        //
        //lines.Add(newLine);

        // Show new pipe.
        newLine.SetActive(true);  
    }

    /// <summary>
    /// Set new pipe attributes: positions and colors. 
    /// Use LineRenderer component to set attributes.
    /// </summary>
    /// <param name="_lineobj">Pipe object reference</param>
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
    /// Create new pipe Descriptor and add it to the line container 
    /// </summary>
    /// <param name="distanceVectors">Tuple of position vectors (mid, start, end)</param>
    private void AddNewLine(Tuple<Vector3, Vector3, Vector3> distanceVectors)
    {
        ARLineDefinition definition = new ARLineDefinition(tag, color, distanceVectors);

        ScanMenifest.LineDefinitions.Add(definition);
    }

    /// <summary>
    /// Return the Vectors of all the positions relative to door in the scene.
    /// </summary>
    /// <param name="startPoint">Position of the start point</param>
    /// <param name="endPoint">Position of the end point</param>
    /// <param name="doorPoint">Position of the door</param>
    /// <param name="_mid">Position of the middle of the pipe (out)</param>
    /// <returns>1. Middle position between start point and end point<br/>
    ///          2. Middle point relative to door position.<br/>
    ///          3. Start point relative to door position.<br/>
    ///          4. End point relative to door position.</returns>
    private Tuple<Vector3,Vector3,Vector3> CalcVectors(Vector3 startPoint, Vector3 endPoint, Vector3 doorPoint, out Vector3 _mid)
    {
        _mid = Vector3.Lerp(startPoint, endPoint, 0.5f);
        return new Tuple<Vector3, Vector3, Vector3>(doorPoint - _mid, // middle
                                                    doorPoint - startPoint, // start
                                                    doorPoint - endPoint); // end
    }

    /// <summary>
    /// Save the pipes data to the device memory.
    /// </summary>
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

    
    /// <summary>
    /// Set pipe attribute. Called when user press symbles (lighting, water drop or fire)
    /// </summary>
    /// <param name="x"></param>
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

    /// <summary>
    /// Called when user press Save on the menu panel. Show alert.
    /// </summary>
    public void OnSavePress()
    {
        alertText.text = "Save and Exit?";
        save = true;
        Alert.SetActive(true);
        MenuPanel.SetActive(false);
    }

    /// <summary>
    /// Called when user press Exit on the menu panel. Show alert.
    /// </summary>
    public void OnExitPress()
    {
        alertText.text = "Are You sure you want to exit to\nmain menu without saving?\n\nAll data will be lost.";
        save = false;
        Alert.SetActive(true);
        MenuPanel.SetActive(false);
    }


    /// <summary>
    /// Called when user press Menu. Show menu panel.
    /// </summary>
    public void OnMenuPress()
    {
        MenuPanel.SetActive(true);
    }

    /// <summary>
    /// Called when user press exit menu. Hide menu panel.
    /// </summary>
    public void OnExitMenuPress()
    {
        MenuPanel.SetActive(false);
    }

    /// <summary>
    /// Exit the scene with or without save the data.
    /// </summary>
    public void OnConfirmPress()
    {
        if (save) // User press save on the menu panel
        {
             Save();
        }
        sceneUtil.ResetScene("MainMenu", false);
    }

    /// <summary>
    /// Called when user press Cancel. Show menu panel and hide the alert.
    /// </summary>
    public void OnCancelPress()
    {
        Alert.SetActive(false);
        MenuPanel.SetActive(true);
    }


}
