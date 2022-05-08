using System.IO;
using Unity.Collections;
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
    private GameObject pointsPrefab, doorPrefab, linePrefab, HUD;
    [SerializeField] private Button saveBTM, okBTM;
    public TMPro.TextMeshProUGUI textMeshPro;

    private Color color = Color.white;
    private new string tag = "line";
    private Ray inputRay;
    private bool isFirstPoint = true;
    private ARPlaneManager planeManager;
    private ARSessionOrigin sessionOrigin;
    private Camera arCamera;
    private State state = (State)SceneStage.ResetScene;
    private GameObject asset, startPoint, endPoint, door;
    private ARLineMenifest ScanMenifest = new ARLineMenifest();
    private string SceneName;
    
    private NativeArray<XRRaycastHit> raycastHits = new NativeArray<XRRaycastHit>();

    void Awake()
    {
        Debug.Log($"State is: {state}");
       
        if (state == State.placeDoor)
        {
            textMeshPro.text = "Touch the Left-Rigth\nDoor Corner to Place Door";
            okBTM.gameObject.SetActive(false);
        }
        sessionOrigin = GetComponent<ARSessionOrigin>();
        planeManager = GetComponent<ARPlaneManager>();
        arCamera = sessionOrigin.camera;

        HUD.gameObject.SetActive(false);
        SceneName = NewScanName.SceneName;
        Debug.Log(SceneName);
        saveBTM.gameObject.SetActive(false);

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
        ResetScene(SceneManager.GetActiveScene().buildIndex);
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
                door.transform.SetPositionAndRotation(hitPose.position, Quaternion.identity);
                door.SetActive(true);
                state = State.placeLines;
                HUD.gameObject.SetActive(true);               
                textMeshPro.text = "Draw Pipes";
            }
        }
    }

    // This metod reset the scene and load by index
    private void ResetScene(int _scene)
    {
        var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
        xrManagerSettings.DeinitializeLoader();
        SceneManager.LoadScene(_scene); // reload current scene
        xrManagerSettings.InitializeLoaderSync();
    }

    private void DetectTouch()
    {
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
        Vector3 mid;
        var DistanceVectors = CalcVectors(startPoint.transform.position, 
                                          endPoint.transform.position, 
                                          door.transform.position, out mid);

        addNewLine(DistanceVectors);
        
        GameObject newLine = Instantiate(linePrefab, mid, Quaternion.identity);

        SetLine(ref newLine);
        
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
    private void addNewLine(Tuple<Vector3, Vector3, Vector3> distanceVectors)
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
