using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

[CustomEditor(typeof(ShapeBuilder))]
public class PolygonBuilderEditor : Editor {

    private ShapeBuilder m_ShapeBuidler;
    private SelectionInfo m_SelectionInfo;
    private bool m_NeedsRepaint;

    private SerializedProperty m_ShapePreset;
    private SerializedProperty m_RectangleWidth;
    private SerializedProperty m_RectangleDepth;
    private SerializedProperty m_CircleRadius;
    private SerializedProperty m_CirclePoints;
    private SerializedProperty m_RoundCoordinates;

    private void OnEnable() {
        m_ShapeBuidler = target as ShapeBuilder;
        m_SelectionInfo = new SelectionInfo();

        m_ShapePreset = serializedObject.FindProperty("ShapePreset");
        m_RectangleWidth = serializedObject.FindProperty("RectangleWidth");
        m_RectangleDepth = serializedObject.FindProperty("RectangleDepth");
        m_CircleRadius = serializedObject.FindProperty("CircleRadius");
        m_CirclePoints = serializedObject.FindProperty("CirclePoints");
        m_RoundCoordinates = serializedObject.FindProperty("RoundCoordinates");
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontStyle = FontStyle.Bold;
        GUILayout.Label("Shape settings", labelStyle);

        EditorGUILayout.PropertyField(m_ShapePreset);
        serializedObject.ApplyModifiedProperties();

        switch (m_ShapeBuidler.ShapePreset) {
            case ShapePreset.RECTANGLE:
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(m_RectangleWidth);
                EditorGUILayout.PropertyField(m_RectangleDepth);

                if (EditorGUI.EndChangeCheck()) {
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(m_ShapeBuidler, "Rectangle preset");
                    m_ShapeBuidler.GenerateReactanglePreset();
                    m_NeedsRepaint = true;
                    EditorUtility.SetDirty(m_ShapeBuidler);
                    m_ShapeBuidler.OnPolygonUpdate?.Invoke();
                }
                break;
            case ShapePreset.CIRCULAR:
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(m_CircleRadius);
                EditorGUILayout.PropertyField(m_CirclePoints);
                EditorGUILayout.PropertyField(m_RoundCoordinates);

                if (EditorGUI.EndChangeCheck()) {
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(m_ShapeBuidler, "Circle preset");
                    m_ShapeBuidler.GenerateCirclePreset();
                    m_NeedsRepaint = true;
                    EditorUtility.SetDirty(m_ShapeBuidler);
                    m_ShapeBuidler.OnPolygonUpdate?.Invoke();
                }
                break;
            default:
                break;
        }


        GUILayout.Space(20);
        if (GUILayout.Button("Clear")) {
            Clear();
        }

    }

    private void OnSceneGUI() {
        Event guiEvent = Event.current;

        if (guiEvent.type == EventType.Repaint) {
            DrawPolygon();
        } else if (guiEvent.type == EventType.Layout) {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        } else {
            if (m_ShapeBuidler.ShapePreset == ShapePreset.CUSTOM) {
                HandleInput(guiEvent);
            }
            if (m_NeedsRepaint) {
                HandleUtility.Repaint();
            }
        }
    }

    private void Clear() {
        Undo.RecordObject(m_ShapeBuidler, "Clear");
        m_ShapeBuidler.Clear();
        EditorUtility.SetDirty(m_ShapeBuidler);
        m_ShapeBuidler.OnPolygonUpdate?.Invoke();
    }

    private void DrawPolygon() {
        Vector3 transformPosition = m_ShapeBuidler.transform.position;
        Color oldColor = Handles.color;
        for (int i = 0; i < m_ShapeBuidler.Points.Count; i++) {
            if (m_ShapeBuidler.ShapePreset != ShapePreset.CUSTOM) {
                Handles.color = Color.white;
            } else if (i == m_SelectionInfo.PointIndex) {
                Handles.color = m_SelectionInfo.IsPointSelected ? m_ShapeBuidler.SelectedHandleColor : m_ShapeBuidler.HoveredHandleColor;
            } else {
                Handles.color = m_ShapeBuidler.HandleColor;
            }
            Handles.DrawSolidDisc(m_ShapeBuidler.Points[i] + transformPosition, Vector3.up, m_ShapeBuidler.HandleRadius);

            if (i == m_SelectionInfo.LineIndex) {
                Handles.color = m_ShapeBuidler.ShapePreset != ShapePreset.CUSTOM ? Color.green : m_ShapeBuidler.HoveredLineColor;
                Vector3 nextPoint = m_ShapeBuidler.Points[(i + 1) % m_ShapeBuidler.Points.Count];
                Handles.DrawLine(m_ShapeBuidler.Points[i] + transformPosition, nextPoint + transformPosition);
            } else {
                Handles.color = m_ShapeBuidler.ShapePreset != ShapePreset.CUSTOM ? Color.green : m_ShapeBuidler.LineColor;
                Vector3 nextPoint = m_ShapeBuidler.Points[(i + 1) % m_ShapeBuidler.Points.Count];
                Handles.DrawDottedLine(m_ShapeBuidler.Points[i] + transformPosition, nextPoint + transformPosition, m_ShapeBuidler.LineDensity);
            }
        }
        Handles.color = oldColor;
        m_NeedsRepaint = false;
    }

    private void HandleInput(Event guiEvent) {
        Vector3 transformPosition = m_ShapeBuidler.transform.position;

        Vector3 mousePosition = GetMousePosition(guiEvent) - transformPosition;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 &&
            guiEvent.modifiers == EventModifiers.Shift) {
            DeletePointUnderMouse();
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 &&
            guiEvent.modifiers == EventModifiers.None) {
            HandleLeftMouseDown(mousePosition);
        }

        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0 &&
            guiEvent.modifiers == EventModifiers.None) {
            HandleLeftMouseUp(mousePosition);
        }

        if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 &&
            guiEvent.modifiers == EventModifiers.None) {
            HandleLeftMouseDrag(mousePosition);
        }

        if (!m_SelectionInfo.IsPointSelected) {
            UpdateMouseHoverSelection(mousePosition);
        }
    }

    private void HandleLeftMouseDown (Vector3 mousePosition) {
        if (!m_SelectionInfo.IsMouseOverPoint) {
            int pointIndex = m_SelectionInfo.IsMouseOverLine
                ? m_SelectionInfo.LineIndex + 1
                : m_ShapeBuidler.Points.Count;
            Undo.RecordObject(m_ShapeBuidler, "Add point");
            m_ShapeBuidler.Points.Insert(pointIndex, new Vector3(Mathf.RoundToInt(mousePosition.x), 0, Mathf.RoundToInt(mousePosition.z)));
            m_SelectionInfo.PointIndex = pointIndex;
            m_ShapeBuidler.OnPolygonUpdate?.Invoke();
        }

        m_SelectionInfo.LineIndex = -1;
        m_SelectionInfo.IsPointSelected = true;
        m_SelectionInfo.StartDragPosition = mousePosition;
        m_NeedsRepaint = true;
    }

    private void HandleLeftMouseUp (Vector3 mousePosition) {
        if (m_SelectionInfo.IsPointSelected) {
            m_ShapeBuidler.Points[m_SelectionInfo.PointIndex] = m_SelectionInfo.StartDragPosition;
            Undo.RecordObject(m_ShapeBuidler, "Move point");
            m_ShapeBuidler.Points[m_SelectionInfo.PointIndex] = new Vector3(Mathf.RoundToInt(mousePosition.x), 0, Mathf.RoundToInt(mousePosition.z));

            m_SelectionInfo.IsPointSelected = false;
            m_SelectionInfo.PointIndex = -1;
            m_NeedsRepaint = true;
            m_ShapeBuidler.OnPolygonUpdate?.Invoke();
        }
    }

    private void HandleLeftMouseDrag (Vector3 mousePosition) {
        if (m_SelectionInfo.IsPointSelected) {
            Vector3 newPosition = new Vector3(Mathf.RoundToInt(mousePosition.x), 0,
                Mathf.RoundToInt(mousePosition.z));
            m_ShapeBuidler.Points[m_SelectionInfo.PointIndex] = newPosition;
            m_NeedsRepaint = true;
        }
    }

    private Vector3 GetMousePosition(Event guiEvent) {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float drawPlaneHeight = 0f;
        float distanceToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
        return  mouseRay.GetPoint(distanceToDrawPlane);
    }

    private void UpdateMouseHoverSelection(Vector3 mousePosition) {
        int mouseHoverPointIndex = -1;
        for (int i = 0; i < m_ShapeBuidler.Points.Count; i++) {
            if (Vector3.Distance(mousePosition, m_ShapeBuidler.Points[i]) < m_ShapeBuidler.HandleRadius) {
                mouseHoverPointIndex = i;
                break;
            }
        }

        if (mouseHoverPointIndex != m_SelectionInfo.PointIndex) {

            m_NeedsRepaint = true;
        }
        m_SelectionInfo.PointIndex = mouseHoverPointIndex;
        m_SelectionInfo.IsMouseOverPoint = mouseHoverPointIndex != -1;

        if (m_SelectionInfo.IsMouseOverPoint) {
            m_SelectionInfo.IsMouseOverLine = false;
            m_SelectionInfo.LineIndex = -1;
        } else {
            int mouseOverLineIndex = -1;
            float closestLineDistance = m_ShapeBuidler.HandleRadius;

            for (int i = 0; i < m_ShapeBuidler.Points.Count; i++) {
                Vector3 nextPoint =
                    m_ShapeBuidler.Points[(i + 1) % m_ShapeBuidler.Points.Count];
                float distanceBetweenMouse =
                    HandleUtility.DistancePointToLineSegment(mousePosition.ToXZ(),
                        m_ShapeBuidler.Points[i].ToXZ(), nextPoint.ToXZ());
                if (distanceBetweenMouse < closestLineDistance) {
                    closestLineDistance = distanceBetweenMouse;
                    mouseOverLineIndex = i;
                }
            }

            if (m_SelectionInfo.LineIndex != mouseOverLineIndex) {
                m_SelectionInfo.LineIndex = mouseOverLineIndex;
                m_SelectionInfo.IsMouseOverLine = mouseOverLineIndex != -1;
                m_NeedsRepaint = true;
            }
        }
    }

    private void DeletePointUnderMouse() {
        Undo.RecordObject(m_ShapeBuidler, "Delete point");
        m_ShapeBuidler.Points.RemoveAt(m_SelectionInfo.PointIndex);
        m_SelectionInfo.IsPointSelected = false;
        m_SelectionInfo.IsMouseOverPoint = false;
        m_NeedsRepaint = true;
        m_ShapeBuidler.OnPolygonUpdate?.Invoke();
    }

    public class SelectionInfo {
        public int PointIndex = -1;
        public bool IsMouseOverPoint;
        public bool IsPointSelected;
        public Vector3 StartDragPosition;

        public int LineIndex = -1;
        public bool IsMouseOverLine;
    }
}
