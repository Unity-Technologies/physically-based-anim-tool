using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CenterOfMass
{
    public Vector3 pos;
    public Vector3 velocity;

    public Vector3 KeyframeToVelocity (Vector3ClipTimeInfo clip)
    {
        Vector3 vel = Vector3.zero;

        vel.x = (clip.currentX.value - clip.previousX.value) / (clip.currentX.time - clip.previousX.time);
        vel.y = (clip.currentY.value - clip.previousY.value) / (clip.currentY.time - clip.previousY.time);
        vel.z = (clip.currentZ.value - clip.previousZ.value) / (clip.currentZ.time - clip.previousZ.time);

        return vel;
    }

}

