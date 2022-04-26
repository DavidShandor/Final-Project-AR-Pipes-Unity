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
        sessionOrigin = GetComponent<ARSessionOrigin>();
        planeManager = GetComponent<ARPlaneManager>();
        arCamera = sessionOrigin.camera;

        arLineMenifest = ScanListController.lineMenifest;
        Debug.Log(arLineMenifest);

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

    private void PlaceDoor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            inputRay = arCamera.ScreenPointToRay(Input.mousePosition);
            raycastHits = planeManager.Raycast(inputRay, TrackableType.All, Allocator.Temp);

            if (raycastHits.Length > 0)
            {
                Pose hitPose = raycastHits[0].pose;
                GameObject doorObj = Instantiate(doorPrefab, hitPose.position, hitPose.rotation);
 
                state = State.pickmMesh;
                //HUD.gameObject.SetActive(true);
                DrawLines();
                textMeshPro.text = "Pipes are presenting";
            }
        }
    }

    private void DrawLines()
    {
        foreach(var line in arLineMenifest.LineDefinitions)
        {
            Debug.Log(line);
            Draw(line);
        }
    }

    private void Draw(ARApp.Lines.ARLineDefinition arLine)
    {
        GameObject newLine = Instantiate(linePrefab, arLine.mid, Quaternion.identity);
        newLine.tag = arLine.tag;
        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();
        lineRenderer.startColor = arLine.color;
        lineRenderer.endColor = arLine.color;
        //line settings
        lineRenderer.SetPosition(0, arLine.startPosition);
        lineRenderer.SetPosition(1, arLine.endPosition);

        newLine.SetActive(true);
        //TODO: set the new line to be relative to the door.
        

        Debug.Log($"DrawLine Done. Object is: {newLine}");
    }

}
