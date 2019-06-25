using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vector3ClipTimeInfo
{
    public Keyframe previousX;
    public Keyframe previousY;
    public Keyframe previousZ;
    public Keyframe currentX;
    public Keyframe currentY;
    public Keyframe currentZ;
    public Keyframe nextX;
    public Keyframe nextY;
    public Keyframe nextZ;
}
