using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example
{
    public CentredSkinnedMesh centredSkinnedMesh;
    
    void Start()
    {
        // Get the root motion curves authored by the user.
        RootMotionCurves authoredCurves = AnimationWindowInfo.GetRootMotionCurves();
        
        // Convert these curves to use the centre of mass.
        RootMotionCurves centreOfMassCurves = authoredCurves.ConvertRootToCOM();
        
        // "Smooth" out these COM curves to have physically accurate trajectory.
        float takeOffTime = 0.1f;
        float landTime = 0.9f;
        RootMotionCurves physicallyAccurateCurves = centreOfMassCurves.GetTrajectoryCurves(takeOffTime, landTime);
        
        // Convert the smoothed out COM curves back to root curves.
        RootMotionCurves adjustedCurves = physicallyAccurateCurves.ConvertCOMToRoot();
        
        // Write back to the clip with the new curves.
        AnimationWindowInfo.WriteRootMotionCurves(adjustedCurves);
    }

    void OtherExample()
    {
        AnimationClip clip = AnimationWindowInfo.GetClip();    // DONE

        TransformCurves[] hierarchyCurves = centredSkinnedMesh.GetTransformCurves(clip);    // DONE

        RootMotionCurves authoredCurves = AnimationWindowInfo.GetRootMotionCurves();    // DONE

        // TODO: figure out what to do with the below.
        {
            float time = 0f;
            Vector3 comAtTime = centredSkinnedMesh.CalculateCentreOfMass(hierarchyCurves, time);
            Vector3 rootAtTime = authoredCurves.GetRootPosition(time);
            Vector3 deltaAtTime = comAtTime - rootAtTime;
        }
        
        Vector3[] deltas = new Vector3[0];
        float[] times = new float[0];

        RootMotionCurves centreOfMassCurves = RootMotionCurves.GetCOMCurves(deltas, times, authoredCurves);    // DONE

        float takeOffTime = 0.1f;
        float landTime = 0.9f;
        RootMotionCurves physicallyAccurateCurves = centreOfMassCurves.GetTrajectoryCurves(takeOffTime, landTime);    // DONE
        
        RootMotionCurves adjustedCurves = new RootMotionCurves();    // TODO: make this using the inverse of the delta info.
        
        AnimationWindowInfo.WriteRootMotionCurves(adjustedCurves);    // DONE
    }
}
