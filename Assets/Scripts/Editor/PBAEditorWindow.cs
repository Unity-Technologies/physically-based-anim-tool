using UnityEditor;
using UnityEngine;

class PBAEditorWindow : EditorWindow
{
    private CentredSkinnedMesh m_obj = null;
    private Curve m_physicallyAccurateCurve = new Curve();
    private Curve m_adjustedCurve = new Curve();
    private Curve m_comCurve = new Curve();
    private Curve m_oldComCurve = new Curve();
    private int m_NumSamples = 10;
    private AnimationClip m_clip;
    private TransformCurves m_physicallyAccurateTransCurves = null;
    private TransformCurves m_adjustedTransCurves = null;
    private TransformCurves m_oldComCurves = null; 
    private bool m_showGizmos = false;
    private float m_takeOffTime = 1.375f;
    private float m_landTime = 3.03f;
    private float m_gravity = -9.8f;

    // Used to determine if we want to recompute:
    private CentredSkinnedMesh m_prevObj = null;
    private int m_prevNumSamples = 0;
    private float m_prevTakeOffTime = 0.0f;
    private float m_prevLandTime = 0.0f;

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
        m_NumSamples = Mathf.Max(2, EditorGUILayout.IntField("Num samples", m_NumSamples));
        EditorGUILayout.Space();
        m_takeOffTime = EditorGUILayout.FloatField("Take Off time", m_takeOffTime);
        m_takeOffTime = Mathf.Clamp(m_takeOffTime, 0f, m_landTime);
        m_landTime = EditorGUILayout.FloatField("Land Time", m_landTime);
        m_landTime = Mathf.Clamp(m_landTime, m_takeOffTime, m_clip.length);
        EditorGUILayout.MinMaxSlider(GUIContent.none, ref m_takeOffTime, ref m_landTime, 0f, m_clip.length);

        EditorGUILayout.Space();
        m_gravity = EditorGUILayout.FloatField("Gravity", m_gravity);
        EditorGUILayout.Space();

        m_showGizmos = EditorGUILayout.Toggle("Show gizmos", m_showGizmos);

        EditorGUILayout.Space();

        bool userChangedOptions = m_obj != m_prevObj || m_NumSamples != m_prevNumSamples || m_takeOffTime != m_prevTakeOffTime || m_landTime != m_prevLandTime;
        userChangedOptions |= m_physicallyAccurateTransCurves == null;

        if (userChangedOptions)
        {
            m_prevObj = m_obj;
            m_prevNumSamples = m_NumSamples;
            m_prevLandTime = m_landTime;
            m_prevTakeOffTime = m_takeOffTime;

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

            m_oldComCurves = TransformCurves.ConvertRootCurvesToCOMCurves(rootToCOMs, times, hierarchyCurves[0]);    // DONE

            m_physicallyAccurateTransCurves = TransformCurves.GetTrajectoryCurves(m_oldComCurves, m_takeOffTime, m_landTime, m_gravity);

            m_adjustedTransCurves = TransformCurves.ConvertCOMCurvesToRootCurves(rootToCOMs, times, m_physicallyAccurateTransCurves);
        }

        EditorGUILayout.Space();

        var style = new GUIStyle(GUI.skin.button);
        /*style.normal.textColor = Color.cyan;
        if (GUILayout.Button("Draw old COM curves", style)
            || (userChangedOptions && m_adjustedCurve.HasPoints))
        {
            m_oldComCurve.Clear();
            if (m_oldComCurves != null)
                m_oldComCurve = GetCurveTransformCurve(m_oldComCurves, m_NumSamples, m_clip.length);
            else
                Debug.Log("Compute curves first");
            SceneView.RepaintAll();
        }*/
        EditorGUILayout.Space();

        style.normal.textColor = Color.red;

        if (GUILayout.Button("Draw new COM curves", style)
            || (userChangedOptions && m_physicallyAccurateCurve.HasPoints))
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

        //Draw center of mass
        if (GUILayout.Button("Draw old COM curves", style)
            || (userChangedOptions && m_comCurve.HasPoints))
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
            m_BezierDrawer = null;
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
       // m_oldComCurve.DrawCurve(m_showGizmos, Color.cyan);
        if (m_BezierDrawer != null)
            m_BezierDrawer.DrawBezier(Color.red, 5f);

        m_comCurve.DrawCurve(m_showGizmos, Color.cyan);
        if (m_comCurve.HasPoints)
        {
            Handles.SphereHandleCap(0, m_comCurve.EvaluatePoint(m_takeOffTime / m_clip.length), Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.SphereHandleCap(0, m_comCurve.EvaluatePoint(m_landTime / m_clip.length), Quaternion.identity, 0.1f, EventType.Repaint);
        }
    }
}

