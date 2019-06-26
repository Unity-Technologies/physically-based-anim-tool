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

        if (GUILayout.Button("Wipe Points"))
        {
            Curve3 c3 = target as Curve3;
            c3.m_positions.Clear();
            c3.m_orientations.Clear();
        }
    }

    void CalculateCOMs()
    {
        Curve3 c3 = target as Curve3;

        c3.m_positions.Clear();
        c3.m_orientations.Clear();

        AnimationWindowInfo.GetTypeInfo();
        AnimationClip c = AnimationWindowInfo.GetClip();
        GameObject selfCopy = Instantiate(c3.transform.gameObject, c3.transform.parent);
        int numAnimSamples = 100;
        for (int i = 0; i < numAnimSamples; i++)
        {
            float animTime = i * c.length / numAnimSamples;
            c.SampleAnimation(selfCopy, animTime);

            var com = selfCopy.GetComponent<CentredSkinnedMesh>().CalculateCentreOfMass();

            c3.m_positions.Add(com);
            c3.m_orientations.Add(Quaternion.identity);
        }
    }

    void OnSceneGUI()
    {
        EditorGUI.BeginChangeCheck();
        Curve3 c3 = target as Curve3;

        for (int i = 0; i < c3.m_positions.Count; i++)
        {
            if (Tools.current == Tool.Rotate)
            {
                Quaternion newRot = Handles.RotationHandle(c3.m_orientations[i], c3.m_positions[i]);
                c3.m_orientations[i] = newRot;
            }
            else
            {
                Vector3 newPos = Handles.PositionHandle(c3.m_positions[i], c3.m_orientations[i]);
                c3.m_positions[i] = newPos;
            }
        }

        {
            float tIncrement = 0.05f;
            for (float t = 0.0f; t + tIncrement <= 1.0f; t += tIncrement)
            {
                var curPos = c3.EvaluatePoint(t);
                var nextPos = c3.EvaluatePoint(t + tIncrement);
                Handles.DrawLine(curPos, nextPos);
                
            }

        }

        EditorGUI.EndChangeCheck();
    }
}
