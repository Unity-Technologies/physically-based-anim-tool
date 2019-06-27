using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example
{
    public CentredSkinnedMesh centredSkinnedMesh;

    void OtherExample()
    {
       /* AnimationClip clip = AnimationWindowInfo.GetClip();    // DONE

        TransformCurves[] hierarchyCurves = centredSkinnedMesh.GetTransformCurves(clip);    // DONE

        RootMotionCurves authoredCurves = AnimationWindowInfo.GetRootMotionCurves();    // DONE

        int frameCount = 10;
        float timePerFrame = clip.length / frameCount;
        Vector3[] deltas = new Vector3[frameCount];
        float[] times = new float[0];
        for (int i = 0; i < frameCount; i++)
        {
            times[i] = i * timePerFrame;
            Vector3 comAtTime = centredSkinnedMesh.CalculateCentreOfMass(hierarchyCurves, times[i]);
            Vector3 rootAtTime = authoredCurves.GetRootPosition(times[i]);
            deltas[i] = comAtTime - rootAtTime;
        }

        RootMotionCurves centreOfMassCurves = RootMotionCurves.GetCOMCurvesFromRootCurves(deltas, times, authoredCurves);    // DONE

        float takeOffTime = 0.1f;
        float landTime = 0.9f;
        RootMotionCurves physicallyAccurateCurves = centreOfMassCurves.GetTrajectoryCurves(takeOffTime, landTime);    // DONE

        RootMotionCurves adjustedCurves =
            RootMotionCurves.GetRootCurvesFromCOMCurves(deltas, times, physicallyAccurateCurves);    // DONE
        
        AnimationInfo.WriteRootMotionCurves(clip, adjustedCurves);    // DONE*/
    }

    void AnotherExample()
    {
        AnimationClip clip = AnimationWindowInfo.GetClip();    // DONE
        
        TransformCurves[] hierarchyCurves = centredSkinnedMesh.GetTransformCurves(clip);    // DONE
        
        int frameCount = 10;
        float timePerFrame = clip.length / frameCount;
        Vector3[] rootToCOMs = new Vector3[frameCount];
        float[] times = new float[0];
        for (int i = 0; i < frameCount; i++)
        {
            times[i] = i * timePerFrame;
            Vector3 comAtTime = centredSkinnedMesh.CalculateCentreOfMass(hierarchyCurves, times[i]);
            Vector3 rootAtTime = hierarchyCurves[0].GetPosition(times[i]);
            rootToCOMs[i] = comAtTime - rootAtTime;
        }
        
        TransformCurves comCurves = TransformCurves.ConvertRootCurvesToCOMCurves(rootToCOMs, times, hierarchyCurves[0]);
        
        float takeOffTime = 0.1f;
        float landTime = 0.9f;
        TransformCurves physicallyAccurateCOMCurves = TransformCurves.GetTrajectoryCurves(comCurves, takeOffTime, landTime);
        
        TransformCurves adjustedRootCurves = TransformCurves.ConvertCOMCurvesToRootCurves(rootToCOMs, times, physicallyAccurateCOMCurves);
        
        AnimationInfo.WriteTransformCurves(clip, adjustedRootCurves);
    }
}
