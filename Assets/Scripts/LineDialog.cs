using UnityEngine;
using TMPro;



public class LineDialog : MonoBehaviour
{
    //editor fields
    public TextMeshProUGUI text;

    //Var

    [HideInInspector]
    public bool isVisible = true;

    // Unity Messages

    private void Start()
    {
        SetVisibility(false);
    }

    //methodd
    public void SetVisibility(bool pIsVisible)
    {
        gameObject.SetActive(pIsVisible);
        isVisible = pIsVisible;
        
    }

    public void Set(string pMessage)
    {
        SetVisibility(true);
        text.text = pMessage;
    }

    public void onClose()
    {
        SetVisibility(false);
    }

}
