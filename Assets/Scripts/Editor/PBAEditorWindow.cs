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
    private TransformCurves m_physicallyAccurateTransCurves = null;
    private TransformCurves m_adjustedTransCurves = null;
    private bool showGizmos = true;

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
        else if(animator.GetCurrentAnimatorClipInfo(0).Length == 0)
        {
            Debug.LogWarning("Current animator clip info empty ");
            return;
        }

        //Retrieve clip
        m_clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        m_NumSamples = EditorGUILayout.IntField("Num samples", m_NumSamples);
        EditorGUILayout.Space();

        showGizmos = EditorGUILayout.Toggle("Show gizmos", showGizmos);

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
            m_physicallyAccurateTransCurves = TransformCurves.GetTrajectoryCurves(comCurves, takeOffTime, landTime, gravity);

            m_adjustedTransCurves = TransformCurves.ConvertCOMCurvesToRootCurves(rootToCOMs, times, m_physicallyAccurateTransCurves);
        }

        EditorGUILayout.Space();
        var style = new GUIStyle(GUI.skin.button);
        style.normal.textColor = Color.green;
        if (GUILayout.Button("Draw physically accurate curve", style))
        {
            m_BezierDrawer = new BezierDrawer(m_physicallyAccurateTransCurves.m_PosX, m_physicallyAccurateTransCurves.m_PosY, m_physicallyAccurateTransCurves.m_PosZ);
            
            
            m_physicallyAccurateCurve.Clear();
            if (m_physicallyAccurateTransCurves != null)
                m_physicallyAccurateCurve = GetCurveTransformCurve(m_physicallyAccurateTransCurves, m_NumSamples, m_clip.length);
            else
                Debug.Log("Compute curves first");
            SceneView.RepaintAll();
        }
        EditorGUILayout.Space();

        style.normal.textColor = Color.cyan;
        if (GUILayout.Button("Draw adjusted curve", style))
        {
            m_adjustedCurve.Clear();
            if(m_adjustedTransCurves != null)
                m_adjustedCurve = GetCurveTransformCurve(m_adjustedTransCurves, m_NumSamples, m_clip.length);
            else
                Debug.Log("Compute curves first");
            SceneView.RepaintAll();
        }
        EditorGUILayout.Space();

        //Draw center of mass
        style.normal.textColor = Color.white;
        if (GUILayout.Button("Draw COMs", style))
        {
            m_comCurve.Clear();
            m_comCurve = GetCOMCurves(m_clip, m_NumSamples);
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();
        style.normal.textColor = Color.white;
        //Wipe curves
        if (GUILayout.Button("Wipe all curves"))
        {
            m_comCurve.Clear();
            m_adjustedCurve.Clear();
            m_physicallyAccurateCurve.Clear();
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();
        //Overwrite animation clip
        if (GUILayout.Button("Save"))
        {
            Undo.RecordObject(m_clip, "Changed Root AnimationCurves");
            if (m_adjustedTransCurves != null)
                m_adjustedTransCurves.WriteCurves(m_clip);
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

    public Curve GetCurveTransformCurve(TransformCurves authoredCurves, int count, float duration)
    {
        Curve res = new Curve();

        float timePerCount = duration / count;
        for (int i = 0; i < count; i++)
        {
            res.Positions.Add(authoredCurves.GetPosition(timePerCount * i));
            res.Orientations.Add(Quaternion.identity);
        }
        return res;
    }

    BezierDrawer m_BezierDrawer;
    void OnSceneGUI(SceneView view)
    {
        if (m_BezierDrawer != null)
            m_BezierDrawer.DrawBezier(Color.red, 5f);
        m_comCurve.DrawCurve(showGizmos, Color.white);
        m_physicallyAccurateCurve.DrawCurve(showGizmos, Color.green, straightLines: true);
        m_adjustedCurve.DrawCurve(showGizmos, Color.cyan);
    }
}

