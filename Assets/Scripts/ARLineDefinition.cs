using System;
using UnityEngine;


[Serializable]
public class ARLineDefinition
{
    public string tag;
    public bool visible = true;
    public Color color;
    public Vector3 mid;
    public Vector3 start;
    public Vector3 end;

    public ARLineDefinition(string tag, Color color, Tuple<Vector3, Vector3, Vector3> position)
    {
        this.tag = tag;
        this.color = color;
        mid = position.Item1;
        start = position.Item2;
        end = position.Item3;
    }
}



