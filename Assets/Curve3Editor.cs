using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

[CustomEditor(typeof(Curve3))]
public class Curve3Editor : Editor
{
    void Start() { }

    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("New point"))
        {
            Curve3 c3 = target as Curve3;
            c3.m_positions.Add(new Vector3());
            c3.m_orientations.Add(Quaternion.identity);
        }

        if (GUILayout.Button("Calculate COMs"))
        {
            CalculateCOMs();
        }
    }

    void CalculateCOMs()
    {
        Curve3 c3 = target as Curve3;

        c3.m_positions.Clear();
        c3.m_orientations.Clear();

        for(int i = 0; i < 10; i++)
        {
            c3.m_positions.Add(new Vector3(0, 1.5f + 0.2f * Mathf.Sin((float)i * 5.0f), 0.5f * (float)i));
            c3.m_orientations.Add(Quaternion.identity);
        }
    }

    void OnSceneGUI()
    {
        EditorGUI.BeginChangeCheck();
        Curve3 c3 = target as Curve3;

        for(int i = 0; i < c3.m_positions.Count; i++)
        {
            Vector3 newPos = Handles.PositionHandle(c3.m_positions[i], c3.m_orientations[i]);
            c3.m_positions[i] = newPos;
        }

        for(int i = 0; i < c3.m_positions.Count - 1; i++)
        {
            Handles.DrawLine(c3.m_positions[i], c3.m_positions[i + 1]);
        }

        EditorGUI.EndChangeCheck();
    }
}
