using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PolygonStock))]
public class PolygonStockEditor : Editor {

    private PolygonStock m_PolygonStock;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (m_PolygonStock == null) {
            m_PolygonStock = target as PolygonStock;
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Generate")) {
            Undo.RecordObject(m_PolygonStock, "Generate building");
            m_PolygonStock.Generate();
        }

        if (GUILayout.Button("Normalize base")) {
            Undo.RecordObject(m_PolygonStock, "Normalize building");
            m_PolygonStock.Normalize();
        }

        if (GUILayout.Button("Destroy")) {
            Undo.RecordObject(m_PolygonStock, "Destroy building");
            m_PolygonStock.Clear();
        }
    }
}
