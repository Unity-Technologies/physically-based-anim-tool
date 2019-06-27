using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BezierDrawer
{
    struct Bezier1DPoints
    {
        public float p0;
        public float p1;
        public float p2;
        public float p3;

        public float startTime;
        public float endTime;
    }

    struct Bezier3DPoints
    {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p3;
    }

    Bezier3DPoints[] m_Curve;
    

    public BezierDrawer(AnimationCurve xCurve, AnimationCurve yCurve, AnimationCurve zCurve)
    {
        List<Bezier1DPoints> xPoints = new List<Bezier1DPoints>(xCurve.length - 1);
        for (int i = 0; i < xCurve.length - 1; i++)
        {
            xPoints.Add(GetBezierPointsFromKeyframes(xCurve[i], xCurve[i + 1]));
        }
        
        List<Bezier1DPoints> yPoints = new List<Bezier1DPoints>(yCurve.length - 1);
        for (int i = 0; i < yCurve.length - 1; i++)
        {
            yPoints.Add(GetBezierPointsFromKeyframes(yCurve[i], yCurve[i + 1]));
        }
        
        List<Bezier1DPoints> zPoints = new List<Bezier1DPoints>(zCurve.length - 1);
        for (int i = 0; i < zCurve.length - 1; i++)
        {
            zPoints.Add(GetBezierPointsFromKeyframes(zCurve[i], zCurve[i + 1]));
        }
        
        AddCutsToPoints(xPoints, ref yPoints, ref zPoints);
        AddCutsToPoints(yPoints, ref xPoints, ref zPoints);
        AddCutsToPoints(zPoints, ref xPoints, ref yPoints);

        m_Curve = ConvertBezier1DsToBezier3Ds(xPoints, yPoints, zPoints);
    }

    Bezier1DPoints GetBezierPointsFromKeyframes(Keyframe first, Keyframe second)
    {
        Bezier1DPoints bezier2DPoints = new Bezier1DPoints();
        
        bezier2DPoints.p0 = first.value;
        bezier2DPoints.p1 = first.value + first.outTangent * first.outWeight;
        bezier2DPoints.p2 = second.value - second.inTangent * second.inWeight;
        bezier2DPoints.p3 = second.value;
        bezier2DPoints.startTime = first.time;
        bezier2DPoints.endTime = second.time;

        return bezier2DPoints;
    }

    void AddCutsToPoints(List<Bezier1DPoints> toCheck, ref List<Bezier1DPoints> toAugment1,
        ref List<Bezier1DPoints> toAugment2)
    {
        for (int i = 0; i < toCheck.Count; i++)
        {
            Bezier1DPoints checkPoints = toCheck[i];
            float cutTime = checkPoints.startTime;
            bool doCut = true;
            int cutIndex = -1;

            for (int j = 0; j < toAugment1.Count; j++)
            {
                if (Mathf.Approximately(cutTime, toAugment1[j].startTime))
                {
                    doCut = false;
                }

                if (toAugment1[j].startTime < cutTime && toAugment1[j].endTime > cutTime)
                {
                    cutIndex = j;
                }
            }

            if (doCut)
            {
                Bezier1DPoints toCut = toAugment1[cutIndex];
                float normalisedCutTime = Mathf.InverseLerp(toAugment1[cutIndex].startTime, toAugment1[cutIndex].endTime, cutTime);
                CutBezier2D(toCut, normalisedCutTime, out Bezier1DPoints firstHalf, out Bezier1DPoints secondHalf);
                
                toAugment1.RemoveAt(cutIndex);

                if (cutIndex == toAugment1.Count)
                {
                    toAugment1.Add(firstHalf);
                    toAugment1.Add(secondHalf);
                }
                else
                {
                    toAugment1.Insert(cutIndex, secondHalf);
                    toAugment1.Insert(cutIndex, firstHalf);                    
                }
            }
            
            doCut = true;
            cutIndex = -1;

            for (int j = 0; j < toAugment2.Count; j++)
            {
                if (Mathf.Approximately(cutTime, toAugment2[j].startTime))
                {
                    doCut = false;
                }

                if (toAugment2[j].startTime < cutTime && toAugment2[j].endTime > cutTime)
                {
                    cutIndex = j;
                }
            }

            if (doCut)
            {
                Bezier1DPoints toCut = toAugment2[cutIndex];
                float normalisedCutTime = Mathf.InverseLerp(toAugment2[cutIndex].startTime, toAugment2[cutIndex].endTime, cutTime);
                CutBezier2D(toCut, normalisedCutTime, out Bezier1DPoints firstHalf, out Bezier1DPoints secondHalf);
                
                toAugment2.RemoveAt(cutIndex);

                if (cutIndex == toAugment2.Count)
                {
                    toAugment2.Add(firstHalf);
                    toAugment2.Add(secondHalf);
                }
                else
                {
                    toAugment2.Insert(cutIndex, secondHalf);
                    toAugment2.Insert(cutIndex, firstHalf);                    
                }
            }
        }
    }

    void CutBezier2D(Bezier1DPoints input, float t, out Bezier1DPoints firstHalf, out Bezier1DPoints secondHalf)
    {
        float q0 = Mathf.Lerp(input.p0, input.p1, t);
        float q1 = Mathf.Lerp(input.p1, input.p2, t);
        float q2 = Mathf.Lerp(input.p2, input.p3, t);

        float r0 = Mathf.Lerp(q0, q1, t);
        float r1 = Mathf.Lerp(q1, q2, t);

        float s0 = Mathf.Lerp(r0, r1, t);
        
        firstHalf = new Bezier1DPoints();
        firstHalf.p0 = input.p0;
        firstHalf.p1 = q0;
        firstHalf.p2 = r0;
        firstHalf.p3 = s0;

        secondHalf = new Bezier1DPoints();
        secondHalf.p0 = s0;
        secondHalf.p1 = r1;
        secondHalf.p2 = q2;
        secondHalf.p3 = input.p3;
    }

    Bezier3DPoints[] ConvertBezier1DsToBezier3Ds(List<Bezier1DPoints> x, List<Bezier1DPoints> y, List<Bezier1DPoints> z)
    {
        Bezier3DPoints[] bezier3DPoints = new Bezier3DPoints[x.Count];
        
        for (int i = 0; i < bezier3DPoints.Length; i++)
        {
            Bezier1DPoints xPoint = x[i];
            Bezier1DPoints yPoint = y[i];
            Bezier1DPoints zPoint = z[i];

            bezier3DPoints[i] = new Bezier3DPoints();
            bezier3DPoints[i].p0 = new Vector3(xPoint.p0, yPoint.p0, zPoint.p0);
            bezier3DPoints[i].p1 = new Vector3(xPoint.p1, yPoint.p1, zPoint.p1);
            bezier3DPoints[i].p2 = new Vector3(xPoint.p2, yPoint.p2, zPoint.p2);
            bezier3DPoints[i].p3 = new Vector3(xPoint.p3, yPoint.p3, zPoint.p3);
        }

        return bezier3DPoints;
    }
    
    public void DrawBezier(Color color, float thickness)
    {
        for (int i = 0; i < m_Curve.Length; i++)
        {
            Handles.DrawBezier(m_Curve[i].p0, m_Curve[i].p3, m_Curve[i].p1, m_Curve[i].p2, color, null, thickness);
        }
    }
}
