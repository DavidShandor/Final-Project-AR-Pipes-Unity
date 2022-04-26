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

public class NewScanARManager : MonoBehaviour
{
    private enum State : int
    {
        Idle = 0,
        placeDoor = 1,
        placeLines = 2
    }

    // Editor Fields
    [SerializeField]
    private GameObject pointsPrefab, doorPrefab, linePrefab, HUD;
    [SerializeField] private Button saveBTM;
    public TMPro.TextMeshProUGUI textMeshPro;

    private Color color = Color.white;
    private new string tag = "line";
    private Ray inputRay;
    private bool isFirstPoint = true;
    private ARPlaneManager planeManager;
    private ARSessionOrigin sessionOrigin;
    private ARAnchorManager anchorManager;
    private Camera arCamera;
    private State state = State.Idle;
    private GameObject asset, startPoint, endPoint, door;
    private ARLineMenifest linesList = new ARLineMenifest();
    private string SceneName;
    
    private NativeArray<XRRaycastHit> raycastHits = new NativeArray<XRRaycastHit>();

    void Awake()
    {
        sessionOrigin = GetComponent<ARSessionOrigin>();
        planeManager = GetComponent<ARPlaneManager>();
        anchorManager = GetComponent<ARAnchorManager>();
        arCamera = sessionOrigin.camera;

        HUD.gameObject.SetActive(false);
        SceneName = NewScanName.SceneName;
        saveBTM.gameObject.SetActive(false);

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
                Pose hitPose = raycastHits[0].pose;
                door.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                door.SetActive(true);
                door.AddComponent<ARAnchor>();
                state = State.placeLines;
                HUD.gameObject.SetActive(true);               
                textMeshPro.text = "Draw Pipes";
            }

        }
    }

    private void DetectTouch()
    {
        //TODO: Change input to Touch.Began and Touch.End to draw line. 
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            
            inputRay = arCamera.ScreenPointToRay(Input.mousePosition);
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
                    saveBTM.gameObject.SetActive(true);
                }

                // Toggle the bool so next we will place the next point.
                isFirstPoint = !isFirstPoint;
            }

        }
    }

    private void DrawLine()
    {
        //TODO: Draw continouse line 
        Vector3 mid = CalcMidVector(startPoint.transform.position, endPoint.transform.position);
        ARLineDefinition definition = new ARLineDefinition(
                                    1,"Line", "Line", tag, color,
                                    startPoint.transform.position,
                                    endPoint.transform.position, mid);
        
        linesList.LineDefinitions.Add(definition);

        GameObject newLine = Instantiate(linePrefab, mid, Quaternion.identity);
        
        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.SetPosition(0, startPoint.transform.position);
        lineRenderer.SetPosition(1, endPoint.transform.position);

        newLine.SetActive(true);
        //set the new line to be relative to the door.
        newLine.transform.parent = door.transform;
    }

    //TODO: calculate line position relative to the door
    private static Vector3 CalcMidVector(Vector3 startPoint, Vector3 endPoint)
    {
        return Vector3.Lerp(startPoint, endPoint, 0.5f);
    }


    public void Save()
    {
        Debug.Log("Save Start");
        
        if (!Directory.Exists(Application.persistentDataPath + "/Scans"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Scans");
        }
        string data = JsonUtility.ToJson(linesList);

        File.WriteAllText(Application.persistentDataPath + $"/Scans/{SceneName}", data);
        Debug.Log("Save Has Done!");
        Debug.Log(data);
        SceneManager.LoadScene("MainMenu");
    }

    private static void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
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


}
