using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public struct RootMotionCurves
{
    public AnimationCurve rootTXCurve;
    public AnimationCurve rootTYCurve;
    public AnimationCurve rootTZCurve;
    public AnimationCurve rootQXCurve;
    public AnimationCurve rootQYCurve;
    public AnimationCurve rootQZCurve;
    public AnimationCurve rootQWCurve;

    List<float> m_KeyTimes;
    
    public RootMotionCurves GetTrajectoryCurves(float takeOffTime, float landTime, float gravity = -9.81f)
    {
        if(m_KeyTimes == null || m_KeyTimes.Count == 0)
            SetKeyTimes();

        float duration = landTime - takeOffTime;
        Vector3 takeOffPosition = GetVector3(takeOffTime);
        Vector3 landPosition = GetVector3(landTime);
        Vector3 delta = landPosition - takeOffPosition;
        
        Keyframe[] xKeys = new Keyframe[m_KeyTimes.Count];
        Keyframe[] yKeys = new Keyframe[m_KeyTimes.Count];
        Keyframe[] zKeys = new Keyframe[m_KeyTimes.Count];

        for (int i = 0; i < xKeys.Length; i++)
        {
            float time = m_KeyTimes[i];
            xKeys[i] = new Keyframe(time, GetLateralTrajectory(time, duration, delta.x) + takeOffPosition.x);
            yKeys[i] = new Keyframe(time, GetVerticalTrajectory(time, duration, gravity) + takeOffPosition.y);
            zKeys[i] = new Keyframe(time, GetLateralTrajectory(time, duration, delta.z) + takeOffPosition.z);
        }
        
        AnimationCurve xCurve = new AnimationCurve(xKeys);
        AnimationCurve yCurve = new AnimationCurve(yKeys);
        AnimationCurve zCurve = new AnimationCurve(zKeys);

        for (int i = 0; i < xCurve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode (xCurve, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode (xCurve, i, AnimationUtility.TangentMode.ClampedAuto);
        }

        for (int i = 0; i < yCurve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode (yCurve, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode (yCurve, i, AnimationUtility.TangentMode.ClampedAuto);
        }

        for (int i = 0; i < zCurve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode (zCurve, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode (zCurve, i, AnimationUtility.TangentMode.ClampedAuto);
        }

        var curves = new RootMotionCurves();
        curves.rootTXCurve = xCurve;
        curves.rootTYCurve = yCurve;
        curves.rootTZCurve = zCurve;
        curves.m_KeyTimes = m_KeyTimes;
        return curves;
    }

    public RootMotionCurves ConvertRootToCOM()
    {
        return this;    // TODO: use COM calc to implement this.
    }

    public RootMotionCurves ConvertCOMToRoot()
    {
        return this;    // TODO: use inverse COM calc to implement this.
    }

    public void DrawBezier()
    {
        if(m_KeyTimes == null || m_KeyTimes.Count == 0)
            SetKeyTimes();
        
        for (int i = 0; i < m_KeyTimes.Count - 1; i++)
        {
            float startTime = m_KeyTimes[i];
            float endTime = m_KeyTimes[i + 1];
            Vector3 startPos = GetVector3(startTime);
            Vector3 endPos = GetVector3(endTime);
            Vector3 startTangent = Vector3.zero;    // TODO: calculate tangent for these.
            Vector3 endTangent = Vector3.zero;
            
            Handles.DrawBezier(startPos, endPos, startTangent, endTangent, Color.white, null, 2f);
        }
    }

    void SetKeyTimes()
    {
        float maxTime = Mathf.Max(rootTXCurve[rootTXCurve.length - 1].time, rootTYCurve[rootTYCurve.length - 1].time, rootTZCurve[rootTZCurve.length - 1].time);
        
        SetKeyTimes(0f, maxTime);
    }

    void SetKeyTimes(float takeOffTime, float landTime)
    {
        List<float> keyTimes = new List<float>();

        for (int i = 0; i < rootTXCurve.length; i++)
        {
            float curveTime = rootTXCurve[i].time;
            
            if(curveTime >= takeOffTime && curveTime <= landTime)
                keyTimes.Add(rootTXCurve[i].time);
        }

        for (int i = 0; i < rootTYCurve.length; i++)
        {
            float curveTime = rootTYCurve[i].time;
            
            if(curveTime < takeOffTime && curveTime > landTime)
                continue;
            
            bool doAdd = true;
            for (int j = 0; j < keyTimes.Count; j++)
            {
                if (Mathf.Approximately(curveTime, keyTimes[j]))
                    doAdd = false;
            }
            
            if(doAdd)
                keyTimes.Add(curveTime);
        }
        
        for (int i = 0; i < rootTZCurve.length; i++)
        {
            float curveTime = rootTZCurve[i].time;
            
            if(curveTime < takeOffTime && curveTime > landTime)
                continue;
            
            bool doAdd = true;
            for (int j = 0; j < keyTimes.Count; j++)
            {
                if (Mathf.Approximately(curveTime, keyTimes[j]))
                    doAdd = false;
            }
            
            if(doAdd)
                keyTimes.Add(curveTime);
        }
        
        keyTimes.Sort();
    }

    public Vector3 GetRootPosition (float time)
    {
        Vector3 startTranslation = GetVector3 (0f);
        Quaternion startRotation = GetQuaternion (0f);
            
        Vector3 currentTranslation = GetVector3 (time);
        Quaternion currentRotation = GetQuaternion (time);
            
        Vector3 translationDelta = currentTranslation - startTranslation;
        Quaternion rotationDelta = currentRotation * Quaternion.Inverse (startRotation);

        return rotationDelta * translationDelta;
    }

    Vector3 GetVector3(float time)
    {
        return new Vector3(rootTXCurve.Evaluate(time), rootTYCurve.Evaluate(time), rootTZCurve.Evaluate(time));
    }

    Quaternion GetQuaternion(float time)
    {
        return new Quaternion(rootQXCurve.Evaluate(time), rootQYCurve.Evaluate(time), rootQZCurve.Evaluate(time), rootQWCurve.Evaluate(time));
    }

    static float GetLateralTrajectory(float time, float duration, float distance)
    {
        return distance * time / duration;
    }

    static float GetVerticalTrajectory(float time, float duration, float gravity)
    {
        return 0.5f * gravity * time * time - 0.5f * gravity * duration * time;
    }

    public static RootMotionCurves GetCOMCurvesFromRootCurves(Vector3[] deltas, float[] times, RootMotionCurves rootMotionCurves)
    {
        RootMotionCurves comCurves = rootMotionCurves;
        
        AnimationCurve x = new AnimationCurve();
        AnimationCurve y = new AnimationCurve();
        AnimationCurve z = new AnimationCurve();
        
        for (int i = 0; i < deltas.Length; i++)
        {
            Vector3 delta = deltas[i];
            float time = times[i];

            x.AddKey(time, (delta + rootMotionCurves.GetRootPosition(time)).x);
            y.AddKey(time, (delta + rootMotionCurves.GetRootPosition(time)).y);
            z.AddKey(time, (delta + rootMotionCurves.GetRootPosition(time)).z);
        }

        comCurves.rootTXCurve = x;
        comCurves.rootTYCurve = y;
        comCurves.rootTZCurve = z;

        return comCurves;
    }

    public static RootMotionCurves GetRootCurvesFromCOMCurves(Vector3[] deltas, float[] times, RootMotionCurves comCurves)
    {
        // TODO: complete me
        return comCurves;
    }
}