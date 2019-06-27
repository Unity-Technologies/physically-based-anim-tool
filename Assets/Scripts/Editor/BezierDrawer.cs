using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BezierDrawer
{
    struct Bezier2DPoints
    {
        public Vector2 p0;
        public Vector2 p1;
        public Vector2 p2;
        public Vector2 p3;
    }

    struct Bezier3DPoints
    {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p3;
    }

    Bezier3DPoints[] m_Curve;

    // for each keyframe go through this to get a bunch of bezier points (length - 1)
    Bezier2DPoints GetBezierPointsFromKeyframes(Keyframe first, Keyframe second)
    {
        Bezier2DPoints bezier2DPoints = new Bezier2DPoints();
        
        float deltaTime = second.time - first.time;
        
        bezier2DPoints.p0 = new Vector2(first.time, first.value);
        bezier2DPoints.p1 = new Vector2(first.time + deltaTime * first.outWeight, first.value + first.outTangent * deltaTime * first.outWeight);
        bezier2DPoints.p2 = new Vector2(second.time + deltaTime * second.inWeight, second.value + second.inTangent * deltaTime * second.inWeight);
        bezier2DPoints.p3 = new Vector2(second.time, second.value);

        return bezier2DPoints;
    }

    Vector2 Evaluate(Bezier2DPoints input, float t)
    {
        
        Vector2 q0 = Vector2.Lerp(input.p0, input.p1, t);
        Vector2 q1 = Vector2.Lerp(input.p1, input.p2, t);
        Vector2 q2 = Vector2.Lerp(input.p2, input.p3, t);

        Vector2 r0 = Vector2.Lerp(q0, q1, t);
        Vector2 r1 = Vector2.Lerp(q1, q2, t);

        Vector2 s0 = Vector2.Lerp(r0, r1, t);

        return s0;
    }

    // go through all keyframes in time order and cut where required
    void CutBezier2D(Bezier2DPoints input, float t, out Bezier2DPoints firstHalf, out Bezier2DPoints secondHalf)
    {
        Vector2 q0 = Vector2.Lerp(input.p0, input.p1, t);
        Vector2 q1 = Vector2.Lerp(input.p1, input.p2, t);
        Vector2 q2 = Vector2.Lerp(input.p2, input.p3, t);

        Vector2 r0 = Vector2.Lerp(q0, q1, t);
        Vector2 r1 = Vector2.Lerp(q1, q2, t);

        Vector2 s0 = Vector2.Lerp(r0, r1, t);
        
        firstHalf = new Bezier2DPoints();
        firstHalf.p0 = input.p0;
        firstHalf.p1 = q0;
        firstHalf.p2 = r0;
        firstHalf.p3 = s0;

        secondHalf = new Bezier2DPoints();
        secondHalf.p0 = s0;
        secondHalf.p1 = r1;
        secondHalf.p2 = q2;
        secondHalf.p3 = input.p3;
    }

    public BezierDrawer(AnimationCurve xCurve, AnimationCurve yCurve, AnimationCurve zCurve)
    {
        List<Bezier2DPoints> xPoints = new List<Bezier2DPoints>(xCurve.length - 1);
        for (int i = 0; i < xCurve.length - 1; i++)
        {
            xPoints.Add(GetBezierPointsFromKeyframes(xCurve[i], xCurve[i + 1]));
        }
        
        List<Bezier2DPoints> yPoints = new List<Bezier2DPoints>(yCurve.length - 1);
        for (int i = 0; i < yCurve.length - 1; i++)
        {
            yPoints.Add(GetBezierPointsFromKeyframes(yCurve[i], yCurve[i + 1]));
        }
        
        List<Bezier2DPoints> zPoints = new List<Bezier2DPoints>(zCurve.length - 1);
        for (int i = 0; i < zCurve.length - 1; i++)
        {
            zPoints.Add(GetBezierPointsFromKeyframes(zCurve[i], zCurve[i + 1]));
        }
        
        AddCutsToPoints(xPoints, ref yPoints, ref zPoints);
        AddCutsToPoints(yPoints, ref xPoints, ref zPoints);
        AddCutsToPoints(zPoints, ref xPoints, ref yPoints);

        m_Curve = ConvertBezier2DsToBezier3Ds(xPoints, yPoints, zPoints);
    }

    Bezier3DPoints[] ConvertBezier2DsToBezier3Ds(List<Bezier2DPoints> x, List<Bezier2DPoints> y, List<Bezier2DPoints> z)
    {
        Bezier3DPoints[] bezier3DPoints = new Bezier3DPoints[x.Count];
        
        for (int i = 0; i < bezier3DPoints.Length; i++)
        {
            Bezier2DPoints xPoint = x[i];
            Bezier2DPoints yPoint = y[i];
            Bezier2DPoints zPoint = z[i];

            bezier3DPoints[i] = new Bezier3DPoints();
            bezier3DPoints[i].p0 = new Vector3(xPoint.p0.x, yPoint.p0.x, zPoint.p0.x);
            bezier3DPoints[i].p3 = new Vector3(xPoint.p3.x, yPoint.p3.x, zPoint.p3.x);
            bezier3DPoints[i].p1 = new Vector3(xPoint.p1.x + xPoint.p1.y,
                yPoint.p1.x + yPoint.p1.y, zPoint.p1.x + zPoint.p1.y);
            bezier3DPoints[i].p2 = new Vector3(xPoint.p2.x + xPoint.p2.y,
                yPoint.p2.x + yPoint.p2.y, zPoint.p2.x + zPoint.p2.y);
        }

        return bezier3DPoints;
    }

    void AddCutsToPoints(List<Bezier2DPoints> toCheck, ref List<Bezier2DPoints> toAugment1,
        ref List<Bezier2DPoints> toAugment2)
    {
        for (int i = 0; i < toCheck.Count; i++)
        {
            Bezier2DPoints checkPoints = toCheck[i];
            bool doCut = true;
            int cutIndex = -1;

            for (int j = 0; j < toAugment1.Count; j++)
            {
                if (Mathf.Approximately(checkPoints.p0.x, toAugment1[j].p0.x))
                {
                    doCut = false;
                }

                if (toAugment1[j].p0.x < checkPoints.p0.x && toAugment1[j].p3.x > checkPoints.p0.x)
                {
                    cutIndex = j;
                }
            }

            if (doCut)
            {
                Bezier2DPoints toCut = toAugment1[cutIndex];
                CutBezier2D(toCut, checkPoints.p0.x, out Bezier2DPoints firstHalf, out Bezier2DPoints secondHalf);
                
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
                if (Mathf.Approximately(checkPoints.p0.x, toAugment2[j].p0.x))
                {
                    doCut = false;
                }

                if (toAugment2[j].p0.x < checkPoints.p0.x && toAugment2[j].p3.x > checkPoints.p0.x)
                {
                    cutIndex = j;
                }
            }

            if (doCut)
            {
                Bezier2DPoints toCut = toAugment2[cutIndex];
                CutBezier2D(toCut, checkPoints.p0.x, out Bezier2DPoints firstHalf, out Bezier2DPoints secondHalf);
                
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
    
    public void DrawBezier(Color color, float thickness)
    {
        for (int i = 0; i < m_Curve.Length - 1; i++)
        {
            Handles.DrawBezier(m_Curve[i].p0, m_Curve[i].p3, m_Curve[i].p1, m_Curve[i].p2, color, null, thickness);
        }
    }
}
