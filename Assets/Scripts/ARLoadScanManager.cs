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
    [SerializeField]
    private GameObject doorPrefab, linePrefab, HUD;
    public TMPro.TextMeshProUGUI textMeshPro;
    public Button okBTM;

    private Vector3 DoorRefPosition;
    private GameObject doorObj;
    private Ray inputRay;
    private ARPlaneManager planeManager;
    private ARSessionOrigin sessionOrigin;
    private Camera arCamera;
    private State state = (State)SceneStage.ResetScene;
    private ARLineMenifest arLineMenifest;
    private List<GameObject> lines = new List<GameObject>();
    private NativeArray<XRRaycastHit> raycastHits = new NativeArray<XRRaycastHit>();

    void Awake()
    {   
        if(state == State.placeDoor)
        {
            textMeshPro.text = "Touch the Left-Rigth\nDoor Corner to show pipes";
            okBTM.gameObject.SetActive(false);
        }
        sessionOrigin = GetComponent<ARSessionOrigin>();
        planeManager = GetComponent<ARPlaneManager>();
        arCamera = sessionOrigin.camera;

        arLineMenifest = ScanListController.lineMenifest;

        HUD.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.placeDoor)
        {
            PlaceDoor();
        }
        //else if (state == State.pickmMesh)
        //{
        //    //TODO: write function toggle pipes visibility by tags
        //}
    }
    public void OnOkPressed()
    {
        SceneStage.ResetScene = (int)State.placeDoor;
        ResetScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ResetScene(int _scene)
    {
        Debug.Log("Reset Scene");
        var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
        xrManagerSettings.DeinitializeLoader();
        SceneManager.LoadScene(_scene); // reload current scene
        xrManagerSettings.InitializeLoaderSync();
    }

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
                HUD.gameObject.SetActive(true);
                textMeshPro.text = "Pipes are presenting";
                DrawLines();
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
        Vector3 _mid;
        var ReferenceDistance = CalcRelativePosition(arLine.mid, arLine.start, arLine.end, out _mid);
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

    public void toggelPipes(string _tag)
    {
        foreach (var line in lines)
        {
            if (line.tag == _tag)
            {
                line.SetActive(!line.activeSelf);
            }
        }
    }
}
