using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example
{
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
}
