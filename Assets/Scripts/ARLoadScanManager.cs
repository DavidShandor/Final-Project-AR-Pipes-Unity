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
    private enum State : int
    {
        findDoor = 0,
        placeDoor = 1,
        pickmMesh = 2
    }

    // Editor Fields
    public Button okBTM;
    public TMPro.TextMeshProUGUI textMeshPro;
    
    [SerializeField]
    private GameObject doorPrefab, linePrefab,MenuIcon, MenuPanel, Alert;

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
    [HideInInspector] public SceneUtilities switchScene;

    void Awake()
    {   
        if(state == State.placeDoor)
        {
            textMeshPro.text = "Touch the Upper-Left\nDoor Corner to show pipes";
            okBTM.gameObject.SetActive(false);
        }
        sessionOrigin = GetComponent<ARSessionOrigin>();
        planeManager = GetComponent<ARPlaneManager>();
        arCamera = sessionOrigin.camera;

        arLineMenifest = ScanListController.lineMenifest;

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
    public void OnOkPressed()
    {
        SceneStage.ResetScene = (int)State.placeDoor;
        switchScene.ResetScene("Load", true);
    }

    //private void ResetScene(string _scene)
    //{
    //    Debug.Log("Reset Scene");
    //    var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
    //    //SceneManager.LoadScene(_scene);
    //    SwitchScene switchScene = GameObject.FindGameObjectWithTag("Crossfade").GetComponent<SwitchScene>();
    //    switchScene.SwitchScenes(_scene);
    //    xrManagerSettings.InitializeLoaderSync();
    //}

    private void PlaceDoor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            inputRay = arCamera.ScreenPointToRay(Input.mousePosition);
            raycastHits = planeManager.Raycast(inputRay, TrackableType.All, Allocator.Temp);

            if (raycastHits.Length > 0)
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

    private void DrawLines()
    {
        foreach(var line in arLineMenifest.LineDefinitions)
        {
            Draw(line);
        }
    }

    private void Draw(ARLineDefinition arLine)
    {
        var ReferenceDistance = CalcRelativePosition(arLine.mid, arLine.start, arLine.end, out Vector3 _mid);
        GameObject newLine = Instantiate(linePrefab, _mid, Quaternion.identity);
        lines.Add(newLine);
        SetLine(ref newLine, arLine, ReferenceDistance);
        
        newLine.SetActive(true);
    }

    private void SetLine(ref GameObject _newLine, ARLineDefinition arLine, Tuple<Vector3, Vector3> refDis)
    {
        _newLine.tag = arLine.tag;
        LineRenderer lineRenderer = _newLine.GetComponent<LineRenderer>();
        lineRenderer.startColor = arLine.color;
        lineRenderer.endColor = arLine.color;
        lineRenderer.SetPosition(0, refDis.Item1);
        lineRenderer.SetPosition(1, refDis.Item2);
        _newLine.transform.parent = doorObj.transform;
    }

    private Tuple<Vector3, Vector3> CalcRelativePosition(Vector3 mid, Vector3 start, Vector3 end, out Vector3 _mid)
    {
        _mid = DoorRefPosition - mid; // mid
        return new Tuple<Vector3, Vector3>(DoorRefPosition - start, // start
                                           DoorRefPosition - end);// end
    }

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

    public void OnToggleElec(bool flag)
    {
        OnTogglePipes("Electricity", flag);
    }
    public void OnToggleCold(bool flag)
    {
        OnTogglePipes("Cold", flag);
    } 
    public void OnToggleHot(bool flag)
    {
        OnTogglePipes("Hot", flag);
    }
    public void OnTogglePlane(bool flag)
    {
        Debug.Log($"Try to Toggle Planes, flag is: {flag}");
        planeManager.enabled = flag;

        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(flag);
        }

    }

    public void OnExitPress()
    {
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


    //private void DestroyAllElements()
    //{
    //    foreach (var l in lines)
    //    {
    //        Destroy(l);
    //    }

    //    foreach (ARPlane p in planeManager.trackables)
    //    {
    //        Destroy(p);
    //    }

    //    Destroy(doorObj);
    //}

    public void OnConfirmPress()
    {
        SceneStage.ResetScene = (int)State.findDoor;
        switchScene.ResetScene("MainMenu", false);
    }
    public void OnCancelPress()
    {
        Alert.SetActive(false);
        MenuPanel.SetActive(true);
    }
}
