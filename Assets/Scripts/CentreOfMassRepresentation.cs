using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CentreOfMassRepresentation : MonoBehaviour
{
    public CentredSkinnedMesh centredSkinnedMesh;

    Transform m_Transform;
    
    void Awake ()
    {
        m_Transform = transform;
    }

    void Update ()
    {
        m_Transform.position = centredSkinnedMesh.CalculateCentreOfMass ();
    }
}
