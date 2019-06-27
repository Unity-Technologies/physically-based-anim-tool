using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Curve
{
    private List<Vector3> m_positions = new List<Vector3>();
    public List<Vector3> Positions => m_positions;

    private List<Quaternion> m_orientations = new List<Quaternion>();
    public List<Quaternion> Orientations => m_orientations;

    public void Clear()
    {
        m_positions.Clear();
        m_orientations.Clear();
    }

    public Vector3 EvaluatePoint(float t)
    {
        t = Mathf.Clamp01(t);
        if (m_positions.Count == 0) return new Vector3(0, 0, 0);
        if (m_positions.Count == 1) return m_positions[0];
        if (m_positions.Count == 2) return Vector3.Lerp(m_positions[0], m_positions[1], t);

        int numGroups = m_positions.Count / 2;
        int groupIdx = System.Math.Min(numGroups - 1, (int)(t * numGroups));

        int idx0 = 2 * groupIdx;
        int idx1 = System.Math.Min(m_positions.Count - 1, idx0 + 1);
        int idx2 = System.Math.Min(m_positions.Count - 1, idx0 + 2);

        float groupStartTime = (float)groupIdx / (float)numGroups;
        float groupEndTime = (float)(groupIdx + 1) / (float)numGroups;
        float tLocal = (t - groupStartTime) / (groupEndTime - groupStartTime);

        float omt = 1.0f - tLocal;
        return omt * omt * m_positions[idx0]
            + 2.0f * omt * tLocal * m_positions[idx1]
            + tLocal * tLocal * m_positions[idx2];
    }

    public void DrawCurve(bool showGizmos, Color color, bool straightLines = false)
    {
        EditorGUI.BeginChangeCheck();
        for (int i = 0; i < Positions.Count; i++)
        {
            if (showGizmos)
            {
                if (Tools.current == Tool.Rotate)
                {
                    Quaternion newRot = Handles.RotationHandle(Orientations[i], Positions[i]);
                    Orientations[i] = newRot;
                }
                else
                {
                    Vector3 newPos = Handles.PositionHandle(Positions[i], Orientations[i]);
                    Positions[i] = newPos;
                }
            }
        }
        {
            Handles.color = color;
            if (straightLines)
            {
                for (int i = 0; i < Positions.Count - 1; i++)
                {
                    Handles.DrawLine(Positions[i], Positions[i + 1]);
                }
            }
            else
            {
                float tIncrement = 0.01f;
                for (float t = 0.0f; t + tIncrement <= 1.0f; t += tIncrement)
                {
                    var curPos = EvaluatePoint(t);
                    var nextPos = EvaluatePoint(t + tIncrement);
                    Handles.DrawLine(curPos, nextPos);
                }
            }
            Handles.color = Color.white;
        }

        EditorGUI.EndChangeCheck();
    }
}

public class Curve3 : MonoBehaviour
{
    public Curve curve = new Curve();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
