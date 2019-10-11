using System.Collections;
using System.Collections.Generic;
using Demo;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ShapeBuilder))]
public class PolygonStock : Shape {
    public ShapeParameters ShapeParameters;
    public int HeightRemaining=0;

    private ShapeBuilder m_ShapeBuilder;

    public void Initialize(ShapeBuilder shape, int HeightRemaining) {
        this.HeightRemaining = HeightRemaining;
        m_ShapeBuilder = shape;
    }

    public void Normalize() {
        for (int i = 0; i < m_ShapeBuilder.Points.Count; i++) {
            Vector3 p1 = m_ShapeBuilder.Points[i];
            Vector3 p2 = m_ShapeBuilder.Points[(i + 1) % m_ShapeBuilder.Points.Count];
            Vector3 direction = p2 - p1;

            float distance = direction.magnitude;
            float overflow = distance - Mathf.Floor(distance);

            if (overflow > 0.05f) {
                m_ShapeBuilder.Points[(i + 1) % m_ShapeBuilder.Points.Count] = p2 - direction.normalized * overflow;
            }
        }
        Generate();
    }

    protected override void Execute() {
        if (parameters == null) {
            parameters = GetComponent<BuildingParameters>();
        }

        if (m_ShapeBuilder == null) {
            m_ShapeBuilder = GetComponent<ShapeBuilder>();

        }

//        m_ShapeBuilder.ShapePreset = ShapeParameters.ShapePreset;
//        m_ShapeBuilder.RectangleWidth = ShapeParameters.RectangleWidth;
//        m_ShapeBuilder.RectangleDepth = ShapeParameters.RectangleDepth;
//        m_ShapeBuilder.CircleRadius = ShapeParameters.CircleRadius;
//        m_ShapeBuilder.CirclePoints = ShapeParameters.CirclePoints;
//        m_ShapeBuilder.RoundCoordinates = ShapeParameters.RoundCoordinates;
//        m_ShapeBuilder.HandleRadius = ShapeParameters.HandleRadius;
//        m_ShapeBuilder.HandleColor = ShapeParameters.HandleColor;
//        m_ShapeBuilder.HoveredHandleColor = ShapeParameters.HoveredHandleColor;
//        m_ShapeBuilder.SelectedHandleColor = ShapeParameters.SelectedHandleColor;
//        m_ShapeBuilder.LineDensity = ShapeParameters.LineDensity;
//        m_ShapeBuilder.LineColor = ShapeParameters.LineColor;
//        m_ShapeBuilder.HoveredLineColor = ShapeParameters.HoveredLineColor;
//        m_ShapeBuilder.Points = ShapeParameters.Points;

        if (m_ShapeBuilder.Points.Count < 3) {
            Debug.LogWarning("Stock polygon should have at least 3 points.");
            return;
        }

        BuildingParameters param = (BuildingParameters)parameters;

        int wallsCount = m_ShapeBuilder.Points.Count;
        for (int i = 0; i<wallsCount; i++) {
            Vector3 startPoint = m_ShapeBuilder.Points[i];
            Vector3 endPoint = m_ShapeBuilder.Points[(i + 1) % wallsCount];

            Vector3 localPosition = new Vector3((startPoint.x + endPoint.x)/2, 0f, (startPoint.z + endPoint.z)/2);
            Quaternion localRotation = Quaternion.LookRotation(endPoint - startPoint, Vector3.up);
            Row newRow = CreateSymbol<Row>("wall", localPosition, localRotation, transform);
            newRow.Initialize(
                Mathf.RoundToInt(Vector3.Distance(startPoint, endPoint)),
                param.wallStyle,
                param.wallPattern
            );
            newRow.Generate();
        }

        double randomValue = param.Rand.NextDouble();

        if (HeightRemaining > 0) {
            PolygonStock nextStock = CreateSymbol<PolygonStock>("polygonBuilding", new Vector3(0, 1, 0), Quaternion.identity, transform);
            nextStock.Initialize(m_ShapeBuilder, HeightRemaining-1);
            nextStock.Generate(param.buildDelay);
        } else {
//            Roof nextRoof = CreateSymbol<Roof>("roof", new Vector3(0, 1, 0), Quaternion.identity, transform);
//            nextRoof.Initialize(4, 4, HeightRemaining-1);
//            nextRoof.Generate(param.buildDelay);
        }
    }

    public void Clear() {
        DeleteGenerated();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
