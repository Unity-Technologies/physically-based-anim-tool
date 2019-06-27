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
        if (copyFrom != null)
        {
            m_TransformPath = copyFrom.m_TransformPath;
            m_Parent = copyFrom.m_Parent;
            m_DefaultPos = copyFrom.m_DefaultPos;
            m_DefaultRot = copyFrom.m_DefaultRot;
            m_DefaultScl = copyFrom.m_DefaultScl;
            m_RotX = copyFrom.m_RotX;
            m_RotY = copyFrom.m_RotY;
            m_RotZ = copyFrom.m_RotZ;
            m_RotW = copyFrom.m_RotW;
            m_SclX = copyFrom.m_SclX;
            m_SclY = copyFrom.m_SclY;
            m_SclZ = copyFrom.m_SclZ;            
        }
        
        m_PosX = x;
        m_PosY = y;
        m_PosZ = z;
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

    public static TransformCurves GetTrajectoryCurves(TransformCurves comCurves, float takeOffTime, float landTime, float gravity = -9.81f)
    {
        float duration = landTime - takeOffTime;
        Vector3 takeOffPosition = comCurves.GetPosition(takeOffTime);
        Vector3 landPosition = comCurves.GetPosition(landTime);
        Vector3 delta = landPosition - takeOffPosition;
        
        int xIndex = 0;
        int yIndex = 0;
        int zIndex = 0;

        float time = 0f;
        
        AnimationCurve x = new AnimationCurve();
        AnimationCurve y = new AnimationCurve();
        AnimationCurve z = new AnimationCurve();

        while (xIndex < comCurves.m_PosX.length || yIndex < comCurves.m_PosY.length || zIndex < comCurves.m_PosZ.length)
        {
            x.AddKey(new Keyframe(time, GetLateralTrajectory(time, duration, delta.x) + takeOffPosition.x));
            y.AddKey(new Keyframe(time, GetVerticalTrajectory(time, duration, gravity) + takeOffPosition.y));
            z.AddKey(new Keyframe(time, GetLateralTrajectory(time, duration, delta.z) + takeOffPosition.z));
            
            if (Mathf.Approximately(comCurves.m_PosX[xIndex].time, time))
                xIndex++;
            if (Mathf.Approximately(comCurves.m_PosY[yIndex].time, time))
                yIndex++;
            if (Mathf.Approximately(comCurves.m_PosZ[zIndex].time, time))
                zIndex++;

            if (xIndex < comCurves.m_PosX.length)
                time = comCurves.m_PosX[xIndex].time;

            if (yIndex < comCurves.m_PosY.length && comCurves.m_PosY[yIndex].time < time)
                time = comCurves.m_PosY[yIndex].time;
            
            if (zIndex < comCurves.m_PosZ.length && comCurves.m_PosZ[zIndex].time < time)
                time = comCurves.m_PosZ[zIndex].time;
        }
        
        for (int i = 0; i < x.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode (x, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode (x, i, AnimationUtility.TangentMode.ClampedAuto);
            
            AnimationUtility.SetKeyLeftTangentMode (y, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode (y, i, AnimationUtility.TangentMode.ClampedAuto);
            
            AnimationUtility.SetKeyLeftTangentMode (z, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode (z, i, AnimationUtility.TangentMode.ClampedAuto);
        }
        
        return new TransformCurves(comCurves, x, y, z);
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

    static float GetLateralTrajectory(float time, float duration, float distance)
    {
        return distance * time / duration;
    }

    static float GetVerticalTrajectory(float time, float duration, float gravity)
    {
        return 0.5f * gravity * time * time - 0.5f * gravity * duration * time;
    }
}
