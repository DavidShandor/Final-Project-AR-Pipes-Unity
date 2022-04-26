using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ARApp.Lines;

public class template : MonoBehaviour
{
    private enum State : int
    {
        Idle = 0,
        placeDoor = 1,
        placeLines = 2
    }

    // Editor Fields
    [SerializeField]
    private GameObject pointsPrefab, doorPrefab;
    [SerializeField] private static GameObject linePrefab;

    private Ray inputRay;
    private bool isFirstPoint = true;
    private ARPlaneManager planeManager;
    private ARSessionOrigin sessionOrigin;
    private Camera arCamera;
    private State state = State.Idle;
    private GameObject asset, startPoint, endPoint, door;
    private List<GameObject> lines = new List<GameObject>();
    private NativeArray<XRRaycastHit> raycastHits = new NativeArray<XRRaycastHit>();

    void Awake()
    {
        sessionOrigin = GetComponent<ARSessionOrigin>();
        planeManager = GetComponent<ARPlaneManager>();
        arCamera = sessionOrigin.camera;

        startPoint = Instantiate(pointsPrefab, Vector3.zero, Quaternion.identity);
        endPoint = Instantiate(pointsPrefab, Vector3.zero, Quaternion.identity);
        door = Instantiate(doorPrefab, Vector3.zero, Quaternion.identity);
        
        startPoint.SetActive(false);
        endPoint.SetActive(false);
        door.SetActive(false);

        state = State.placeDoor;
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

    private void PlaceDoor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            inputRay = arCamera.ScreenPointToRay(Input.mousePosition);
            raycastHits = planeManager.Raycast(inputRay, TrackableType.All, Allocator.Temp);

            if (raycastHits.Length > 0)
            {
                door.SetActive(true);
                Pose hitPose = raycastHits[0].pose;
                door.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);

                state = State.placeLines;
                // ShowHUDPanel();
            }
            

        }
    }

    private void DetectTouch()
    {
        if (Input.GetMouseButtonDown(0))
        {

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

        }
    }


    public static void DrawLine(ARLineData aData = null, GameObject obj = null)
    {
        Vector3 mid = CalcMidVector(aData.startPosition, aData.endPosition);

        GameObject newLine = Instantiate(linePrefab, mid, Quaternion.identity);

        newLine.transform.tag = aData.tag; 
        //newLine.layer = 0; // set layer for each pipe type (3 types)

        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();

        //line settings
        lineRenderer.SetPosition(0, aData.startPosition);
        lineRenderer.SetPosition(1, aData.endPosition);

        //set the new line to be relative to the door.
        //newLine.transform.parent = door.transform;

    }

    private static Vector3 CalcMidVector(Vector3 startPoint, Vector3 endPoint)
    {
        return Vector3.Lerp(startPoint, endPoint, 0.5f);
    }


}
