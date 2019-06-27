using UnityEditor;
using UnityEngine;

public class TransformCurves
{
    public readonly Transform transform;

    readonly AnimationCurve m_PosX;
    readonly AnimationCurve m_PosY;
    readonly AnimationCurve m_PosZ;
    readonly AnimationCurve m_RotX;
    readonly AnimationCurve m_RotY;
    readonly AnimationCurve m_RotZ;
    readonly AnimationCurve m_RotW;
    readonly AnimationCurve m_SclX;
    readonly AnimationCurve m_SclY;
    readonly AnimationCurve m_SclZ;

    readonly string m_TransformPath;

    readonly TransformCurves m_Parent;

    readonly Vector3 m_DefaultPos;
    readonly Quaternion m_DefaultRot;
    readonly Vector3 m_DefaultScl;

    public TransformCurves(TransformCurves parent, Transform transform, AnimationClip clip)
    {
        this.transform = transform;
        
        m_Parent = parent;

        m_DefaultPos = transform.localPosition;
        m_DefaultRot = transform.localRotation;
        m_DefaultScl = transform.localScale;

        if (m_Parent != null)
            m_TransformPath = m_Parent.m_TransformPath + "/";
        m_TransformPath += transform.name;

        m_PosX = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalPosition.x"));
        m_PosY = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalPosition.y"));
        m_PosZ = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalPosition.z"));
        m_RotX = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalRotation.x"));
        m_RotY = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalRotation.y"));
        m_RotZ = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalRotation.z"));
        m_RotW = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalRotation.w"));
        m_SclX = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalScale.x"));
        m_SclY = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalScale.y"));
        m_SclZ = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalScale.z"));
    }

    public TransformCurves(TransformCurves copyFrom, AnimationCurve x, AnimationCurve y, AnimationCurve z)
    {
        m_TransformPath = copyFrom.m_TransformPath;
        m_Parent = copyFrom.m_Parent;
        m_DefaultPos = copyFrom.m_DefaultPos;
        m_DefaultRot = copyFrom.m_DefaultRot;
        m_DefaultScl = copyFrom.m_DefaultScl;
        
        m_PosX = x;
        m_PosY = y;
        m_PosZ = z;
        m_RotX = copyFrom.m_RotX;
        m_RotY = copyFrom.m_RotY;
        m_RotZ = copyFrom.m_RotZ;
        m_RotW = copyFrom.m_RotW;
        m_SclX = copyFrom.m_SclX;
        m_SclY = copyFrom.m_SclY;
        m_SclZ = copyFrom.m_SclZ;
    }
    
    Vector3 GetLocalPosition (float time)
    {
        float x = m_PosX?.Evaluate(time) ?? m_DefaultPos.x;
        float y = m_PosY?.Evaluate(time) ?? m_DefaultPos.x;
        float z = m_PosZ?.Evaluate(time) ?? m_DefaultPos.x;

        return new Vector3(x, y, z);
    }

    Quaternion GetLocalRotation (float time)
    {
        float x = m_RotX?.Evaluate(time) ?? m_DefaultRot.x;
        float y = m_RotY?.Evaluate(time) ?? m_DefaultRot.x;
        float z = m_RotZ?.Evaluate(time) ?? m_DefaultRot.x;
        float w = m_RotW?.Evaluate(time) ?? m_DefaultRot.x;

        return new Quaternion(x, y, z, w);
    }

    Vector3 GetLocalScale (float time)
    {
        float x = m_SclX?.Evaluate(time) ?? m_DefaultScl.x;
        float y = m_SclY?.Evaluate(time) ?? m_DefaultScl.x;
        float z = m_SclZ?.Evaluate(time) ?? m_DefaultScl.x;

        return new Vector3(x, y, z);
    }
    
    public Vector3 GetPosition (float time)
    {
        TransformCurves current = this;
        Matrix4x4 globalTRS = Matrix4x4.TRS (current.GetLocalPosition (time), current.GetLocalRotation (time), current.GetLocalScale (time));

        while (current.m_Parent != null)
        {
            current = current.m_Parent;
            globalTRS = Matrix4x4.TRS (current.GetLocalPosition (time), current.GetLocalRotation (time), current.GetLocalScale (time)) * globalTRS;
        }

        return globalTRS.GetColumn (3);
    }

    public static TransformCurves[] GetTransformCurvesHierarchy(Animator animator, AnimationClip clip)
    {
        Transform currentTransform = animator.transform;
        TransformCurves[] allCurves = new TransformCurves[currentTransform.hierarchyCount];
        TransformCurves currentCurves = null;

        for (int i = 0; i < allCurves.Length; i++)
        {
            currentCurves = new TransformCurves(currentCurves, currentTransform, clip);
            currentTransform.GetNext();
            allCurves[i] = currentCurves;
        }

        return allCurves;
    }

    public TransformCurves GetTrajectoryCurves(float takeOffTime, float landTime, float gravity = -9.81f)
    {
        return new TransformCurves(null, null, null);
    }

    public static TransformCurves ConvertRootCurvesToCOMCurves(Vector3[] rootToCOMs, float[] times,
        TransformCurves rootCurves)
    {
        AnimationCurve x = new AnimationCurve();
        AnimationCurve y = new AnimationCurve();
        AnimationCurve z = new AnimationCurve();
        
        for (int i = 0; i < rootToCOMs.Length; i++)
        {
            Vector3 delta = rootToCOMs[i];
            float time = times[i];

            Vector3 com = delta + rootCurves.GetPosition(time);

            x.AddKey(time, com.x);
            y.AddKey(time, com.y);
            z.AddKey(time, com.z);
        }
        
        TransformCurves comCurves = new TransformCurves(rootCurves, x, y, z);

        return comCurves;
    }

    public static TransformCurves ConvertCOMCurvesToRootCurves(Vector3[] rootToCOMs, float[] times,
        TransformCurves comCurves)
    {
        AnimationCurve x = new AnimationCurve();
        AnimationCurve y = new AnimationCurve();
        AnimationCurve z = new AnimationCurve();
        
        for (int i = 0; i < rootToCOMs.Length; i++)
        {
            Vector3 delta = rootToCOMs[i];
            float time = times[i];

            Vector3 root = comCurves.GetPosition(time) - delta;

            x.AddKey(time, root.x);
            y.AddKey(time, root.y);
            z.AddKey(time, root.z);
        }

        TransformCurves rootCurves = new TransformCurves(comCurves, x, y, z);
        
        return rootCurves;
    }
}
