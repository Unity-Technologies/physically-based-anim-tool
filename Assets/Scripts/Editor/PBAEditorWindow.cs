using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

class PBAEditorWindow : EditorWindow
{
    private CentredSkinnedMesh m_obj = null;
    private Curve m_physicallyAccurateCurve = new Curve();
    private Curve m_adjustedCurve = new Curve();
    private Curve m_comCurve = new Curve();
    private int m_NumComSamples = 10;
    private AnimationClip m_clip;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Physically Based Animation Example")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        PBAEditorWindow window = (PBAEditorWindow)EditorWindow.GetWindow(typeof(PBAEditorWindow));
        window.Show();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += this.OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        m_obj = (CentredSkinnedMesh)EditorGUI.ObjectField(new Rect(3, 3, position.width - 6, 20), "Select object", m_obj, typeof(CentredSkinnedMesh), true);

        if (m_obj == null)
            return;
        EditorGUILayout.Space();

        Animator animator = m_obj.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("No Animator is associated to " + m_obj.name);
            return;
        }

        //Retrieve clip
        m_clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        EditorGUILayout.Space();
        m_NumComSamples = EditorGUILayout.IntField("Num samples", m_NumComSamples);
        EditorGUILayout.Space();

        //Draw center of mass
        if (GUILayout.Button("Draw COMs"))
        {
            m_comCurve.Clear();
            m_comCurve = GetCOMCurves(m_clip, m_NumComSamples);
        }

        //Draw authored curves
        TransformCurves[] hierarchyCurves = m_obj.GetTransformCurves(m_clip);
        RootMotionCurves authoredCurves = AnimationInfo.GetRootMotionCurves(m_clip);

        int frameCount = 10;
        float timePerFrame = m_clip.length / frameCount;
        Vector3[] deltas = new Vector3[frameCount];
        float[] times = new float[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            times[i] = i * timePerFrame;
            Vector3 comAtTime = m_obj.CalculateCentreOfMass(hierarchyCurves, times[i]);
            Vector3 rootAtTime = authoredCurves.GetRootPosition(times[i]);
            deltas[i] = comAtTime - rootAtTime;
        }

        RootMotionCurves centreOfMassCurves = RootMotionCurves.GetCOMCurvesFromRootCurves(deltas, times, authoredCurves);    // DONE

        float takeOffTime = 0.1f;
        float landTime = 0.9f;
        RootMotionCurves physicallyAccurateCurves = centreOfMassCurves.GetTrajectoryCurves(takeOffTime, landTime);    // DONE

        EditorGUILayout.Space();
        if (GUILayout.Button("Draw physically accurate curve"))
        {
            m_physicallyAccurateCurve.Clear();
            m_physicallyAccurateCurve = GetCurveFromRootMotion(physicallyAccurateCurves);
        }
        EditorGUILayout.Space();

        RootMotionCurves adjustedCurves = RootMotionCurves.GetRootCurvesFromCOMCurves(deltas, times, physicallyAccurateCurves);    // DONE

        EditorGUILayout.Space();
        if (GUILayout.Button("Draw physically accurate curve"))
        {
            m_physicallyAccurateCurve.Clear();
            m_physicallyAccurateCurve = GetCurveFromRootMotion(physicallyAccurateCurves);
        }
        EditorGUILayout.Space();

        //Overwrite animation clip
        if(GUILayout.Button("Save"))
        {
            Undo.RecordObject(m_clip, "Changed Root AnimationCurves");
            AnimationInfo.WriteRootMotionCurves(m_clip, adjustedCurves);
        }

        EditorGUILayout.EndVertical();
    }

    public Curve GetCOMCurves(AnimationClip clip, int count)
    {
        Curve res = new Curve();

        // GameObject selfCopy = Instantiate(m_obj.transform.gameObject, m_obj.transform.parent);
        for (int i = 0; i < count; i++)
        {
            float animTime = i * clip.length / count;
            clip.SampleAnimation(m_obj.gameObject, animTime);

            var com = m_obj.CalculateCentreOfMass();

            res.Positions.Add(com);
            res.Orientations.Add(Quaternion.identity);
        }
        //DestroyImmediate(selfCopy);
        return res;
    }

    public Curve GetCurveFromRootMotion(RootMotionCurves authoredCurves)
    {
        Curve res = new Curve();
        if (authoredCurves.m_KeyTimes == null)
            return res;

        for (int i = 0; i < authoredCurves.m_KeyTimes.Count; i++)
        {
            res.Positions.Add(new Vector3(authoredCurves.rootTXCurve.Evaluate(i),
                authoredCurves.rootTYCurve.Evaluate(i),
                authoredCurves.rootTXCurve.Evaluate(i)));
            res.Orientations.Add(new Quaternion(authoredCurves.rootQXCurve.Evaluate(i),
                authoredCurves.rootQYCurve.Evaluate(i),
                authoredCurves.rootQZCurve.Evaluate(i),
                authoredCurves.rootQWCurve.Evaluate(i)));
        }
        return res;
    }

    private void DrawCurve(Curve curve)
    {
        EditorGUI.BeginChangeCheck();
        for (int i = 0; i < curve.Positions.Count; i++)
        {
            if (Tools.current == Tool.Rotate)
            {
                Quaternion newRot = Handles.RotationHandle(curve.Orientations[i], curve.Positions[i]);
                curve.Orientations[i] = newRot;
            }
            else
            {
                Vector3 newPos = Handles.PositionHandle(curve.Positions[i], curve.Orientations[i]);
                curve.Positions[i] = newPos;
            }
        }
        {
            float tIncrement = 0.05f;
            for (float t = 0.0f; t + tIncrement <= 1.0f; t += tIncrement)
            {
                var curPos = curve.EvaluatePoint(t);
                var nextPos = curve.EvaluatePoint(t + tIncrement);
                Handles.DrawLine(curPos, nextPos);
            }
        }
        EditorGUI.EndChangeCheck();
    }

    void OnSceneGUI(SceneView view)
    {
        DrawCurve(m_comCurve);
        DrawCurve(m_physicallyAccurateCurve);
        DrawCurve(m_adjustedCurve);
    }
}

