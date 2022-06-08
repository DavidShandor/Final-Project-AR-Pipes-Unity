using System;
using UnityEngine;

/// <summary>
/// Schema of the Line object.
/// </summary>
[Serializable]
public class ARLineDefinition
{
    // Line Attributes 
    public string tag;
    public bool visible = true;
    public Color color;
    public Vector3 mid;
    public Vector3 start;
    public Vector3 end;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="tag">Tags: Electric / Hot / Cold </param>
    /// <param name="color">Color of the Line: Electric - Yellow, Hot - Red, Cold - Blue </param>
    /// <param name="position">mid, start and end position of the line (required for place the line in space)</param>
    public ARLineDefinition(string tag, Color color, Tuple<Vector3, Vector3, Vector3> position)
    {
        this.tag = tag;
        this.color = color;
        mid = position.Item1;
        start = position.Item2;
        end = position.Item3;
    }
}



