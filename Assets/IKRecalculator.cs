using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class IKRecalculator : MonoBehaviour
{
    public bool m_ConstantWorldSpace;
    public Transform m_Root;
    public Transform m_Target;
    public Transform m_Hint;

    Matrix4x4 m_PreviousRoot = Matrix4x4.identity;

    void Start() { }

    void Update()
    {
        if(m_Root == null)
        {
            return;
        }

        if (m_ConstantWorldSpace)
        {
            Matrix4x4 deltaRoot = m_Root.localToWorldMatrix.inverse * m_PreviousRoot;
            if(m_Target != null)
            {
                updateTransform(m_Target, deltaRoot * m_Target.localToWorldMatrix);
            }
            if(m_Hint != null)
            {
                updateTransform(m_Hint, deltaRoot * m_Hint.localToWorldMatrix);
            }
        }

        m_PreviousRoot = m_Root.localToWorldMatrix;
    }

    void updateTransform(Transform t, Matrix4x4 m)
    {
        t.position = m.GetColumn(3);
        t.rotation = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }
}
