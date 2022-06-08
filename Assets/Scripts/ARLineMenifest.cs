using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Warpper class for the lines list. Required to save the data.
/// </summary>
[Serializable]
public class ARLineMenifest
{
    public List<ARLineDefinition> LineDefinitions = new List<ARLineDefinition>();
}
