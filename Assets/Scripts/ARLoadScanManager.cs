using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SceneManagement;
using System;

public class ARLoadScanManager : MonoBehaviour
{
    /// <summary>
    /// State to maintance the scene flow
    /// </summary>
    private enum State : int
    {
        findDoor = 0,
        placeDoor = 1,
        pickmMesh = 2
    }

    // Editor Fields
    public Button okBTM;
    public TMPro.TextMeshProUGUI textMeshPro;

    [HideInInspector]
    public SceneUtilities switchScene;

    [SerializeField]
    private GameObject doorPrefab, linePrefab,MenuIcon, MenuPanel, Alert;

    // Objects and References 
    private Ray inputRay;
    private Camera arCamera;
    private GameObject doorObj;
    private Vector3 DoorRefPosition;
    private ARPlaneManager planeManager;
    private ARSessionOrigin sessionOrigin;
    private ARLineMenifest arLineMenifest;
    private State state = (State)SceneStage.ResetScene; 
    private List<GameObject> lines = new List<GameObject>();
    private NativeArray<XRRaycastHit> raycastHits = new NativeArray<XRRaycastHit>();

    // Awake is called once in the very beginning of the scene.
    void Awake()
    {   
        // If State == findDoor - need to reset scene, otherwise, place door.
        if(state == State.placeDoor)
        {
            textMeshPro.text = "Touch the Upper-Left\nDoor Corner to show pipes";
            okBTM.gameObject.SetActive(false);
        }
        
        // Initializing objects reference
        sessionOrigin = GetComponent<ARSessionOrigin>();
        planeManager = GetComponent<ARPlaneManager>();
        arCamera = sessionOrigin.camera;

        //Get scan data from the Controller.
        arLineMenifest = ScanListController.lineMenifest;

        //Hide UI 
        MenuPanel.SetActive(false);
        MenuIcon.SetActive(false);
        Alert.SetActive(false);

        switchScene = GameObject.FindGameObjectWithTag("Crossfade").GetComponent<SceneUtilities>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.placeDoor)
        {
            PlaceDoor();
        }
    }

    /// <summary>
    /// Called when user press OK to reset the scene.
    /// </summary>
    public void OnOkPressed()
    {
        SceneStage.ResetScene = (int)State.placeDoor;
        switchScene.ResetScene("Load", true);
    }

    /// <summary>
    /// Position door and draw pipes relative to its position
    /// </summary>
    private void PlaceDoor()
    {
        if (Input.GetMouseButtonDown(0)) // Detect touch
        {
            inputRay = arCamera.ScreenPointToRay(Input.mousePosition);
            raycastHits = planeManager.Raycast(inputRay, TrackableType.All, Allocator.Temp);

            if (raycastHits.Length > 0) // Touch on a plane.
            {
                Pose hitPose = raycastHits[0].pose;
                doorObj = Instantiate(doorPrefab, hitPose.position, Quaternion.identity);
                DoorRefPosition = hitPose.position;
                state = State.pickmMesh;
                MenuIcon.SetActive(true);
                textMeshPro.text = "";
                AndroidMessage._ShowAndroidToastMessage("Drawing Lines...");
                DrawLines();
            }
            else
            {
                AndroidMessage._ShowAndroidToastMessage("Tap on plane only");
            }
        }
    }

    /// <summary>
    /// Draw all the lines.
    /// </summary>
    private void DrawLines()
    {
        foreach(var line in arLineMenifest.LineDefinitions)
        {
            Draw(line);
        }
    }


    /// <summary>
    /// Instantiate Line in the space.
    /// </summary>
    /// <param name="arLine">Line definition to draw</param>
    private void Draw(ARLineDefinition arLine)
    {
        var ReferenceDistance = CalcRelativePosition(arLine.position, out Vector3 _mid);
        GameObject newLine = Instantiate(linePrefab, _mid, Quaternion.identity);
        lines.Add(newLine);
        SetLine(ref newLine, arLine, ReferenceDistance);
        
        newLine.SetActive(true);
    }

    /// <summary>
    /// Set Line color and positions
    /// </summary>
    /// <param name="_newLine">Pipe Object reference</param>
    /// <param name="arLine">Pipe Definition</param>
    /// <param name="refDis">Position of start and end points of the pipe</param>
    private void SetLine(ref GameObject _newLine, ARLineDefinition arLine, Tuple<Vector3, Vector3> refDis)
    {
        _newLine.tag = arLine.tag;
        LineRenderer lineRenderer = _newLine.GetComponent<LineRenderer>();
        lineRenderer.startColor = arLine.color;
        lineRenderer.endColor = arLine.color;
        lineRenderer.SetPosition(0, refDis.Item1);
        lineRenderer.SetPosition(1, refDis.Item2);
        _newLine.transform.parent = doorObj.transform; // Attach the line to the door, so the Anchor manager will track it.
    }

    /// <summary>
    /// Calulate the center, start and end positions of the pipe relative to the door position based on the original scanned position. 
    /// </summary>
    /// <param name="mid">Saved mid position</param>
    /// <param name="start">Saved start position</param>
    /// <param name="end">Saved end position</param>
    /// <param name="_mid">Current pipe mid position</param>
    /// <returns>Positions of the start and the end of the pipe</returns>
    private Tuple<Vector3, Vector3> CalcRelativePosition(Tuple<Vector3, Vector3, Vector3>position, out Vector3 _mid)
    {
        _mid = DoorRefPosition - position.Item1; // mid
        return new Tuple<Vector3, Vector3>(DoorRefPosition - position.Item2, // start
                                           DoorRefPosition - position.Item3);// end
    }

    /// <summary>
    /// Used to toggle visibility of the pipes.
    /// </summary>
    /// <param name="_tag">Pipes tag to be toggled</param>
    /// <param name="flag">True - Show, False - Hide</param>
    public void OnTogglePipes(string _tag, bool flag)
    {
        foreach (var line in lines)
        {
            if (line.tag == _tag)
            {
                line.SetActive(flag);
            }
        }
    }

    /// <summary>
    /// Toggle electricty pipes visibility
    /// </summary>
    /// <param name="flag">Show (true) or Hide (false)?</param>
    public void OnToggleElec(bool flag)
    {
        OnTogglePipes("Electricity", flag);
    }

    /// <summary>
    /// Toggle cold water pipes visibility
    /// </summary>
    /// <param name="flag">Show (true) or Hide (false)?</param>
    public void OnToggleCold(bool flag)
    {
        OnTogglePipes("Cold", flag);
    }

    /// <summary>
    /// Toggle hot water pipes visibility
    /// </summary>
    /// <param name="flag">Show (true) or Hide (false)?</param>
    public void OnToggleHot(bool flag)
    {
        OnTogglePipes("Hot", flag);
    }

    /// <summary>
    /// Toggle planes visibility
    /// </summary>
    /// <param name="flag">Show (true) or Hide (false)?</param>
    public void OnTogglePlane(bool flag)
    {
        Debug.Log($"Try to Toggle Planes, flag is: {flag}");
        planeManager.enabled = flag;

        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(flag);
        }

    }

    /// <summary>
    /// Called when user press exit, show alert.
    /// </summary>
    public void OnExitPress()
    {
        Alert.SetActive(true);
        MenuPanel.SetActive(false);
    }

    /// <summary>
    /// Called when user press Menu, show Menu panel.
    /// </summary>
    public void OnMenuPress()
    {
        MenuPanel.SetActive(true);
    }

    /// <summary>
    /// Called when user close the menu panel, hide the panel.
    /// </summary>
    public void OnExitMenuPress()
    {
        MenuPanel.SetActive(false);
    }

    /// <summary>
    /// Called when user press Confirm.
    /// Set state and reset the scene.
    /// </summary>
    public void OnConfirmPress()
    {
        SceneStage.ResetScene = (int)State.findDoor; // Set state to find door so next scene will start over.
        switchScene.ResetScene("MainMenu", false); // Clear Scene and go to Main Menu.
    }

    //Called when user press Cancel, dismiss alert and show menu panel again.
    public void OnCancelPress()
    {
        Alert.SetActive(false);
        MenuPanel.SetActive(true);
    }
}
