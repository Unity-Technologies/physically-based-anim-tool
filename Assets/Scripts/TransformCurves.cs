using UnityEditor;
using UnityEngine;

public class TransformCurves
{
    public readonly Transform transform;

    public AnimationCurve m_PosX { get; private set; }
    public AnimationCurve m_PosY { get; private set; }
    public AnimationCurve m_PosZ { get; private set; }
    public AnimationCurve m_RotX { get; private set; }
    public AnimationCurve m_RotY { get; private set; }
    public AnimationCurve m_RotZ { get; private set; }
    public AnimationCurve m_RotW { get; private set; }
    public AnimationCurve m_SclX { get; private set; }
    public AnimationCurve m_SclY { get; private set; }
    public AnimationCurve m_SclZ { get; private set; }

    readonly string m_TransformPath;

    readonly TransformCurves m_Parent;

    readonly Vector3 m_DefaultPos;
    readonly Quaternion m_DefaultRot;
    readonly Vector3 m_DefaultScl;
    EditorCurveBinding m_PosXBinding;
    EditorCurveBinding m_PosYBinding;
    EditorCurveBinding m_PosZBinding;
    EditorCurveBinding m_RotXBinding;
    EditorCurveBinding m_RotYBinding;
    EditorCurveBinding m_RotZBinding;
    EditorCurveBinding m_RotWBinding;
    EditorCurveBinding m_SclXBinding;
    EditorCurveBinding m_SclYBinding;
    EditorCurveBinding m_SclZBinding;

    public TransformCurves(TransformCurves parent, Animator animator, Transform transform, AnimationClip clip)
    {
        this.transform = transform;
        
        m_Parent = parent;

        m_DefaultPos = transform.localPosition;
        m_DefaultRot = transform.localRotation;
        m_DefaultScl = transform.localScale;

        m_TransformPath = AnimationUtility.CalculateTransformPath(transform, animator.transform);

        m_PosXBinding = EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalPosition.x");
        m_PosX = AnimationUtility.GetEditorCurve(clip, m_PosXBinding);
        m_PosYBinding = EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalPosition.y");
        m_PosY = AnimationUtility.GetEditorCurve(clip, m_PosYBinding);
        m_PosZBinding = EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalPosition.z");
        m_PosZ = AnimationUtility.GetEditorCurve(clip, m_PosZBinding);
        m_RotXBinding = EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalRotation.x");
        m_RotX = AnimationUtility.GetEditorCurve(clip, m_RotXBinding);
        m_RotYBinding = EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalRotation.y");
        m_RotY = AnimationUtility.GetEditorCurve(clip, m_RotYBinding);
        m_RotZBinding = EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalRotation.z");
        m_RotZ = AnimationUtility.GetEditorCurve(clip, m_RotZBinding);
        m_RotWBinding = EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalRotation.w");
        m_RotW = AnimationUtility.GetEditorCurve(clip, m_RotWBinding);
        m_SclXBinding = EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalScale.x");
        m_SclX = AnimationUtility.GetEditorCurve(clip, m_SclXBinding);
        m_SclYBinding = EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalScale.y");
        m_SclY = AnimationUtility.GetEditorCurve(clip, m_SclYBinding);
        m_SclZBinding = EditorCurveBinding.FloatCurve(m_TransformPath, typeof(Transform), "m_LocalScale.z");
        m_SclZ = AnimationUtility.GetEditorCurve(clip, m_SclZBinding);
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
        float y = m_PosY?.Evaluate(time) ?? m_DefaultPos.y;
        float z = m_PosZ?.Evaluate(time) ?? m_DefaultPos.z;

        return new Vector3(x, y, z);
    }

    Quaternion GetLocalRotation (float time)
    {
        float x = m_RotX?.Evaluate(time) ?? m_DefaultRot.x;
        float y = m_RotY?.Evaluate(time) ?? m_DefaultRot.y;
        float z = m_RotZ?.Evaluate(time) ?? m_DefaultRot.z;
        float w = m_RotW?.Evaluate(time) ?? m_DefaultRot.w;

        return new Quaternion(x, y, z, w);
    }

    Vector3 GetLocalScale (float time)
    {
        float x = m_SclX?.Evaluate(time) ?? m_DefaultScl.x;
        float y = m_SclY?.Evaluate(time) ?? m_DefaultScl.y;
        float z = m_SclZ?.Evaluate(time) ?? m_DefaultScl.z;

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
            currentCurves = new TransformCurves(currentCurves, animator, currentTransform, clip);
            currentTransform = currentTransform.GetNext();
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
        
        float time = takeOffTime;
        
        int xIndex = -1;
        int yIndex = -1;
        int zIndex = -1;
        
        AnimationCurve x = comCurves.m_PosX;
        AnimationCurve y = comCurves.m_PosY;
        AnimationCurve z = comCurves.m_PosZ;

        for (int i = 0; i < x.length; i++)
        {
            float keyTime = x[i].time;
            if (keyTime >= takeOffTime && keyTime <= landTime)
            {
                x.RemoveKey(i);

                if (xIndex == -1)
                    xIndex = i;
            }
        }
        
        for (int i = 0; i < y.length; i++)
        {
            float keyTime = y[i].time;
            if (keyTime >= takeOffTime && keyTime <= landTime)
            {
                y.RemoveKey(i);

                if (yIndex == -1)
                    yIndex = i;
            }
        }
        
        for (int i = 0; i < z.length; i++)
        {
            float keyTime = z[i].time;
            if (keyTime >= takeOffTime && keyTime <= landTime)
            {
                z.RemoveKey(i);

                if (zIndex == -1)
                    zIndex = i;
            }
        }

        while (time < landTime)
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
            AnimationUtility.SetKeyLeftTangentMode(x, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(x, i, AnimationUtility.TangentMode.ClampedAuto);
        }

        for (int i = 0; i < y.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(y, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(y, i, AnimationUtility.TangentMode.ClampedAuto);
        }
        
        for (int i = 0; i < z.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(z, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(z, i, AnimationUtility.TangentMode.ClampedAuto);
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

    public void WriteCurves(AnimationClip clip)
    {
        AnimationUtility.SetEditorCurve(clip, m_PosXBinding, m_PosX);
        AnimationUtility.SetEditorCurve(clip, m_PosYBinding, m_PosY);
        AnimationUtility.SetEditorCurve(clip, m_PosZBinding, m_PosZ);
        AnimationUtility.SetEditorCurve(clip, m_RotXBinding, m_RotX);
        AnimationUtility.SetEditorCurve(clip, m_RotYBinding, m_RotY);
        AnimationUtility.SetEditorCurve(clip, m_RotZBinding, m_RotZ);
        AnimationUtility.SetEditorCurve(clip, m_RotWBinding, m_RotW);
        AnimationUtility.SetEditorCurve(clip, m_SclXBinding, m_SclX);
        AnimationUtility.SetEditorCurve(clip, m_SclYBinding, m_SclY);
        AnimationUtility.SetEditorCurve(clip, m_SclZBinding, m_SclZ);
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
