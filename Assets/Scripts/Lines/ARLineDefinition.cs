using System;
using UnityEngine;

namespace ARApp.Lines
{
    [Serializable]
    public class ARLineDefinition
    {
        public int uid;
        public string title;
        public string description;
        public string tag;
        public bool visible = true;
        public Color color;
        public Vector3 startPosition;
        public Vector3 endPosition;
        public Vector3 mid;

        public ARLineDefinition(int uid, string title, string description, string tag, Color color, Vector3 start, Vector3 end, Vector3 mid)
        {
            this.uid = uid;
            this.title = title;
            this.description = description;
            this.tag = tag;
            this.color = color;
            this.startPosition = start;
            this.endPosition = end;
            this.mid = mid;
        }
    }
}


