using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using UnityEngine.UI;
using System.Linq;




public class LineController : MonoBehaviour
{

    private enum State: int
    {
        Idle = 0,
        placeDoor = 1,
        placePoints = 2
    }
    //editor fields
    [SerializeField]
    private GameObject pointsPrefab, linePrefab, doorPrefab;

    // Variables
    private Ray inputRay;
    private bool isFirstPoint = true;
    private ARPlaneManager planeManager;
    private ARSessionOrigin sessionOrigin;
    private State state = State.Idle;
    private GameObject asset, startPoint, endPoint, door;
    private List<GameObject> lines = new List<GameObject>();
    private NativeArray<XRRaycastHit> raycastHits = new NativeArray<XRRaycastHit>();


    void Awake()
    {
        sessionOrigin = GetComponent<ARSessionOrigin>();
        planeManager = GetComponent<ARPlaneManager>();

        startPoint = Instantiate(pointsPrefab, Vector3.zero, Quaternion.identity);
        endPoint = Instantiate(pointsPrefab, Vector3.zero, Quaternion.identity);    
        door = Instantiate(doorPrefab, Vector3.zero, Quaternion.identity);    
        startPoint.SetActive(false);
        endPoint.SetActive(false);
        door.SetActive(false);

        state = State.placeDoor;
        isFirstPoint = true;
    }


    void Update()
    {
        if (state == State.placeDoor)
        {
            PlaceDoor();
        }
        else if (state == State.placePoints)
        {
            PlacePoint();
        }
    }

    private void PlaceDoor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            inputRay = sessionOrigin.camera.ScreenPointToRay(Input.mousePosition);
            raycastHits = planeManager.Raycast(inputRay, TrackableType.All, Allocator.Temp);

            if (raycastHits.Length > 0)
            {
                door.SetActive(true);
                Pose hitPose = raycastHits[0].pose;
                door.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);

                state = State.placePoints;
                showPointsButtoms();
            }

        }
    }

    private void showPointsButtoms()
    {
       
    }

    void PlacePoint()
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
                    PlaceNewLine();
                    startPoint.SetActive(false);
                    endPoint.SetActive(false);
                }

                // Toggle the bool so next we will place the next point.
                isFirstPoint = !isFirstPoint;
            }
            
        }
    }


    void PlaceNewLine()
    {
        // Calc the mid of the points to create the new line between
        Vector3 mid = CalcMidVector();
           

        GameObject newLine = Instantiate(linePrefab, mid, Quaternion.identity);
      
        //newLine.layer = 0; // set layer for each pipe type (3 types)

        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();
        
        //line settings
        lineRenderer.SetPosition(0, startPoint.transform.position);
        lineRenderer.SetPosition(1, endPoint.transform.position);

        //set the new line to be relative to the door.
        newLine.transform.parent = door.transform;

        lines.Add(newLine);
    }

    private Vector3 CalcMidVector()
    {
        return Vector3.Lerp(startPoint.transform.position, endPoint.transform.position, 0.5f);
    }
}