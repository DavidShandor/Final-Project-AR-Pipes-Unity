using UnityEngine;
using System;

namespace ARApp.Lines
{   
    [Serializable]
    public class ARLineData
    {
        public int uid;
        public string tag;
        public bool visible;
        public Color color;
        public Vector3 startPosition;
        public Vector3 endPosition;

    }
   

}


