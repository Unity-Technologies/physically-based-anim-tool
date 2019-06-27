using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

[CustomEditor(typeof(Curve3))]
public class Curve3Editor : Editor
{
    int m_NumComSamples = 10;
    bool showGizmos = true;

    void Start() { }

    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("New point"))
        {
            Curve3 c3 = target as Curve3;
            c3.curve.Positions.Add(new Vector3());
            c3.curve.Orientations.Add(Quaternion.identity);
        }

        m_NumComSamples = EditorGUILayout.IntField("Num COM samples", m_NumComSamples);
        showGizmos = EditorGUILayout.Toggle("Show gizmos", showGizmos);

        if (GUILayout.Button("Calculate COMs"))
        {
            CalculateCOMs(m_NumComSamples);
        }

        if (GUILayout.Button("Wipe Points"))
        {
            Curve3 c3 = target as Curve3;
            c3.curve.Positions.Clear();
            c3.curve.Orientations.Clear();
        }
    }

    void CalculateCOMs(int numAnimSamples)
    {
        Curve3 c3 = target as Curve3;

        c3.curve.Positions.Clear();
        c3.curve.Orientations.Clear();

        AnimationWindowInfo.GetTypeInfo();
        AnimationClip c = AnimationWindowInfo.GetClip();
        if (c == null)
        {
            Debug.Log("Couldn't get clip. Not recalculating.");
            return;
        }

        GameObject selfCopy = Instantiate(c3.transform.gameObject, c3.transform.parent);
        for (int i = 0; i < numAnimSamples; i++)
        {
            float animTime = i * c.length / numAnimSamples;
            c.SampleAnimation(selfCopy, animTime);

            var com = selfCopy.GetComponent<CentredSkinnedMesh>().CalculateCentreOfMass();

            c3.curve.Positions.Add(com);
            c3.curve.Orientations.Add(Quaternion.identity);
        }
        DestroyImmediate(selfCopy);
    }

    void OnSceneGUI()
    {
        EditorGUI.BeginChangeCheck();
        Curve3 c3 = target as Curve3;

        for (int i = 0; i < c3.curve.Positions.Count; i++)
        {
            if (showGizmos)
            {
                if (Tools.current == Tool.Rotate)
                {
                    Quaternion newRot = Handles.RotationHandle(c3.curve.Orientations[i], c3.curve.Positions[i]);
                    c3.curve.Orientations[i] = newRot;
                }
                else
                {
                    Vector3 newPos = Handles.PositionHandle(c3.curve.Positions[i], c3.curve.Orientations[i]);
                    c3.curve.Positions[i] = newPos;
                }
            }
        }

        {
            float tIncrement = 0.05f;
            for (float t = 0.0f; t + tIncrement <= 1.0f; t += tIncrement)
            {
                var curPos = c3.curve.EvaluatePoint(t);
                var nextPos = c3.curve.EvaluatePoint(t + tIncrement);
                Handles.DrawLine(curPos, nextPos);
                
            }

        }

        EditorGUI.EndChangeCheck();
    }
}
