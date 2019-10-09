using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public enum ShapePreset {
    RECTANGLE,
    CIRCULAR,
    CUSTOM
}
public class ShapeBuilder : MonoBehaviour {

    [HideInInspector]
    public ShapePreset ShapePreset;

    [HideInInspector]
    public int RectangleWidth = 4;
    [HideInInspector]
    public int RectangleDepth = 4;

    [HideInInspector]
    public float CircleRadius = 4f;
    [HideInInspector]
    public int CirclePoints = 4;
    [HideInInspector]
    public bool RoundCoordinates = true;

    public UnityEvent OnPolygonUpdate;

    [Header("Editor settings")]

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

    public void GenerateReactanglePreset() {
        Clear();
        Points.Add(new Vector3(-RectangleWidth/2, 0, RectangleDepth/2));
        Points.Add(new Vector3(RectangleWidth/2, 0, RectangleDepth/2));
        Points.Add(new Vector3(RectangleWidth/2, 0, -RectangleDepth/2));
        Points.Add(new Vector3(-RectangleWidth/2, 0, -RectangleDepth/2));
    }

    public void GenerateCirclePreset() {
        Clear();

        for (int i = 0; i < CirclePoints; i++) {
            float x = Mathf.Cos(2 * Mathf.PI * i / CirclePoints) * CircleRadius;
            float y = Mathf.Sin(2 * Mathf.PI * i / CirclePoints) * CircleRadius;
            if (RoundCoordinates) {
                x = Mathf.RoundToInt(x);
                y = Mathf.RoundToInt(y);
            }
            Points.Add(new Vector3(x, 0, y));
        }
    }

    public void Clear() {
        Points.Clear();
    }
}
