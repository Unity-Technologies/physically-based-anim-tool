using UnityEditor;
using UnityEngine;

class PBAEditorWindow : EditorWindow
{
    private CentredSkinnedMesh m_obj = null;
    private Curve m_physicallyAccurateCurve = new Curve();
    private Curve m_adjustedCurve = new Curve();
    private Curve m_comCurve = new Curve();
    private int m_NumSamples = 10;
    private AnimationClip m_clip;
    private TransformCurves m_physicallyAccurateCOMCurves = null;
    private TransformCurves m_adjustedTransCurves = null;

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
        EditorGUILayout.Space();
        m_NumSamples = EditorGUILayout.IntField("Num samples", m_NumSamples);
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        if (GUILayout.Button("Compute Curves"))
        {
            //Draw authored curves
            TransformCurves[] hierarchyCurves = m_obj.GetTransformCurves(m_clip);

            float timePerFrame = m_clip.length / m_NumSamples;
            Vector3[] rootToCOMs = new Vector3[m_NumSamples];
            float[] times = new float[m_NumSamples];
            for (int i = 0; i < m_NumSamples; i++)
            {
                times[i] = i * timePerFrame;
                Vector3 comAtTime = m_obj.CalculateCentreOfMass(hierarchyCurves, times[i]);
                Vector3 rootAtTime = hierarchyCurves[0].GetPosition(times[i]);
                rootToCOMs[i] = comAtTime - rootAtTime;
            }

            TransformCurves comCurves = TransformCurves.ConvertRootCurvesToCOMCurves(rootToCOMs, times, hierarchyCurves[0]);    // DONE

            float takeOffTime = 1.375f;
            float landTime = 3.03f;
            float gravity = -9.8f;
            m_physicallyAccurateCOMCurves = TransformCurves.GetTrajectoryCurves(comCurves, takeOffTime, landTime, gravity);

            m_adjustedTransCurves = TransformCurves.ConvertCOMCurvesToRootCurves(rootToCOMs, times, m_physicallyAccurateCOMCurves);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Draw physically accurate curve"))
        {
            m_physicallyAccurateCurve.Clear();
            if (m_physicallyAccurateCOMCurves != null)
                m_physicallyAccurateCurve = GetCurveTransformCurve(m_physicallyAccurateCOMCurves, m_NumSamples);
            else
                Debug.Log("Compute curves first");
        }
        EditorGUILayout.Space();

        if (GUILayout.Button("Draw adjusted curve"))
        {
            m_adjustedCurve.Clear();
            if(m_adjustedTransCurves != null)
                m_adjustedCurve = GetCurveTransformCurve(m_adjustedTransCurves, m_NumSamples);
            else
                Debug.Log("Compute curves first");
        }
        EditorGUILayout.Space();

        //Draw center of mass
        if (GUILayout.Button("Draw COMs"))
        {
            m_comCurve.Clear();
            m_comCurve = GetCOMCurves(m_clip, m_NumSamples);
        }

        EditorGUILayout.Space();
        //Overwrite animation clip
        if (GUILayout.Button("Save"))
        {
            Undo.RecordObject(m_clip, "Changed Root AnimationCurves");
            if (m_adjustedTransCurves != null)
                AnimationInfo.WriteTransformCurves(m_clip, m_adjustedTransCurves);
            else
                Debug.Log("Compute curves first");
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

    public Curve GetCurveTransformCurve(TransformCurves authoredCurves, int count)
    {
        Curve res = new Curve();

        for (int i = 0; i < count; i++)
        {
            res.Positions.Add(authoredCurves.GetPosition(i));
            res.Orientations.Add(Quaternion.identity);
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

