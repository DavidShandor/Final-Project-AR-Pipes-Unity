using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARAnchorManager))]
public class ARExp : MonoBehaviour
{
    [SerializeField] private UnityEvent OnInitialized;

    [SerializeField] private UnityEvent OnRetarted;

    private ARPlaneManager arPlaneManager;

    private bool Initialized { get; set; }


    void Awake()
    {
        arPlaneManager = GetComponent<ARPlaneManager>();
        arPlaneManager.planesChanged += PlanesChanged;

        #if UNITY_EDITOR
            OnInitialized.Invoke();
            Initialized = true;
            arPlaneManager.enabled = false;
        #endif
    }

    void PlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (!Initialized)
        {
            Activate();
        }
    }

    private void Activate()
    {
        OnInitialized?.Invoke();
        Initialized = true;
        arPlaneManager.enabled = false; 
    }
   
    public void Restart()
    {
        OnRetarted?.Invoke();
        Initialized = false;
        arPlaneManager.enabled = true;
    }
}
