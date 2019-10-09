using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Building/Shape Parameters")]
public class ShapeParameters : ScriptableObject {
    public ShapePreset ShapePreset;
    public int RectangleWidth = 4;
    public int RectangleDepth = 4;
    public float CircleRadius = 4f;
    public int CirclePoints = 4;
    public bool RoundCoordinates = true;

    [Space]
    [Range(0.01f, 1f)]
    public float HandleRadius = .3f;
    public Color HandleColor = Color.red;
    public Color HoveredHandleColor = Color.cyan;
    public Color SelectedHandleColor = Color.yellow;

    [Space]
    [Range(0.1f, 10f)]
    public float LineDensity = 3f;
    public Color LineColor = Color.green;
    public Color HoveredLineColor = Color.magenta;

    [HideInInspector]
    public List<Vector3> Points = new List<Vector3>();
}
