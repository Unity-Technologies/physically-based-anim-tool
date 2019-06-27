using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curve3Driver : MonoBehaviour
{
    public Curve3 m_sourceCurve;
    float m_localTime;
    public float m_timeScale = 1.0f;

    void Start()
    {
        m_localTime = 0;    
    }

    void Update()
    {
        float t = m_localTime - Mathf.Floor(m_localTime);
        int idx = (int)m_localTime;

        if (m_sourceCurve == null) return;
    
        var csm = GetComponent<CentredSkinnedMesh>();
        if (csm == null) return;

        Vector3 desiredCom = m_sourceCurve.curve.EvaluatePoint(m_localTime);
        Quaternion desiredOrientation = Quaternion.identity;
        Matrix4x4 worldFromDesired = Matrix4x4.TRS(desiredCom, desiredOrientation, new Vector3(1, 1, 1));

        Vector3 actualCom = csm.COM.pos;
        Matrix4x4 worldFromCurrent = Matrix4x4.TRS(csm.COM.pos, transform.rotation, new Vector3(1, 1, 1));

        updateTransform(transform, transform.localToWorldMatrix * worldFromCurrent.inverse * worldFromDesired);
        
        m_localTime += Time.deltaTime * m_timeScale;
    }

    void updateTransform(Transform t, Matrix4x4 m)
    {
        t.position = m.GetColumn(3);
        t.rotation = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }
}
