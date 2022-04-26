using UnityEngine;

namespace ARApp.UI
{

    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;
   
        void Awake()
        {
            instance = this;
        }
    }

}