using ARApp.Lines;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;

public class ARLoadScanManager : MonoBehaviour
{
    private enum State : int
    {
        Idle = 0,
        placeDoor = 1,
        pickmMesh = 2
    }

    // Editor Fields
    [SerializeField]
    private GameObject doorPrefab, linePrefab, HUD;
    public TMPro.TextMeshProUGUI textMeshPro;

    private Vector3 DoorRefPosition;
    private GameObject doorObj;
    private Ray inputRay;
    private ARPlaneManager planeManager;
    private ARSessionOrigin sessionOrigin;
    private Camera arCamera;
    private State state = State.Idle;
    private ARLineMenifest arLineMenifest;
    private List<GameObject> lines = new List<GameObject>();
    private NativeArray<XRRaycastHit> raycastHits = new NativeArray<XRRaycastHit>();

    void Awake()
    {
        //ResetScene(SceneManager.GetActiveScene().buildIndex);
        
        sessionOrigin = GetComponent<ARSessionOrigin>();
        planeManager = GetComponent<ARPlaneManager>();
        arCamera = sessionOrigin.camera;

        arLineMenifest = ScanListController.lineMenifest;

        //HUD.gameObject.SetActive(false);
        state = State.placeDoor;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.placeDoor)
        {
            PlaceDoor();
        }
        else if (state == State.pickmMesh)
        {
            //TODO: write function toggle pipes visibility by tags
        }
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
                doorObj = Instantiate(doorPrefab, hitPose.position, hitPose.rotation);
                DoorRefPosition = hitPose.position;
                //doorObj.AddComponent<ARAnchor>();
                state = State.pickmMesh;
                //HUD.gameObject.SetActive(true);
                textMeshPro.text = "Pipes are presenting";
                DrawLines();
            }
        }
    }

    private void DrawLines()
    {
        Debug.Log("Start Draw lines.");
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
}
