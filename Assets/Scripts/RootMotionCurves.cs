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
    
    public RootMotionCurves GetTrajectoryCurves(float takeOffTime, float landTime, float gravity = -9.81f)
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

        float duration = landTime - takeOffTime;
        Vector3 takeOffPosition = GetVector3(takeOffTime);
        Vector3 landPosition = GetVector3(landTime);
        Vector3 delta = landPosition - takeOffPosition;
        
        Keyframe[] xKeys = new Keyframe[keyTimes.Count];
        Keyframe[] yKeys = new Keyframe[keyTimes.Count];
        Keyframe[] zKeys = new Keyframe[keyTimes.Count];

        for (int i = 0; i < xKeys.Length; i++)
        {
            float time = keyTimes[i];
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
        
        return new RootMotionCurves
        {
            rootTXCurve = xCurve,
            rootTYCurve = yCurve,
            rootTZCurve = zCurve,
        };
    }

    public Vector3 GetVector3(float time)
    {
        return new Vector3(rootTXCurve.Evaluate(time), rootTYCurve.Evaluate(time), rootTZCurve.Evaluate(time));
    }

    public Quaternion GetQuaternion(float time)
    {
        return new Quaternion(rootQXCurve.Evaluate(time), rootQYCurve.Evaluate(time), rootQZCurve.Evaluate(time), rootQWCurve.Evaluate(time));
    }

    static float GetLateralTrajectory(float time, float duration, float distance)
    {
        return distance * time / duration;
    }

    static float GetVerticalTrajectory(float time, float duration, float gravity)
    {
        return -0.5f * gravity * time * time + 0.5f * gravity * duration * time;
    }
}