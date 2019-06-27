using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curve3Driver : MonoBehaviour
{
    public Curve3 m_sourceCurve;
    float m_localTime;

    void Start()
    {
        m_localTime = 0;
    }

    void Update()
    {
        if (m_sourceCurve == null) return;
    
        var csm = GetComponent<CentredSkinnedMesh>();
        if (csm == null) return;

        var animator = GetComponent<Animator>();
        if(animator == null) return;

        var clips = animator.runtimeAnimatorController.animationClips;
        float maxAnimLen = 0.0f;
        foreach (var c in clips)
        {
            maxAnimLen = Mathf.Max(c.length);
        }
        float t = Mathf.Clamp01(m_localTime / maxAnimLen);

        Vector3 desiredCom = m_sourceCurve.EvaluatePoint(t);
        Quaternion desiredOrientation = Quaternion.identity;
        Matrix4x4 worldFromDesired = Matrix4x4.TRS(desiredCom, desiredOrientation, new Vector3(1, 1, 1));

        Vector3 actualCom = csm.COM.pos;
        Matrix4x4 worldFromCurrent = Matrix4x4.TRS(csm.COM.pos, transform.rotation, new Vector3(1, 1, 1));

        updateTransform(transform, transform.localToWorldMatrix * worldFromCurrent.inverse * worldFromDesired);
        
        m_localTime += Time.deltaTime;
    }

    void updateTransform(Transform t, Matrix4x4 m)
    {
        t.position = m.GetColumn(3);
        t.rotation = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }
}
