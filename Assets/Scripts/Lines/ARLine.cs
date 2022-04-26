using UnityEngine;

namespace ARApp.Lines
{
    public class ARLine : MonoBehaviour
    {
        public ARLineDefinition definition;

        public ARLine(ARLineDefinition lineDefinition)
        {
            this.definition = lineDefinition;
            ARLineManager.Add(this);
        }

    }

}


