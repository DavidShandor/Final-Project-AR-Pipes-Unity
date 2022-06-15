using System;
using UnityEngine;

/// <summary>
/// Schema of the Line object.
/// </summary>
[Serializable]
public class ARLineDefinition
{
    // Pipe Attributes 
    public string tag;
    public Color color;
    public Vector3 start, end;
    
   
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="tag">Tags: Electric / Hot / Cold </param>
    /// <param name="color">Color of the Line: Electric - Yellow, Hot - Red, Cold - Blue </param>
    /// <param name="position">start and end position of the line (required for place the line in space)</param>
    public ARLineDefinition(string tag, Color color, Tuple<Vector3, Vector3> position)
    {
        this.tag = tag;
        this.color = color;
        this.start = position.Item1;
        this.end = position.Item2;
    }
}



