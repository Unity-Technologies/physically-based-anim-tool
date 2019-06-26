using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curve3 : MonoBehaviour
{
    public List<Vector3> m_positions = new List<Vector3>();
    public List<Quaternion> m_orientations = new List<Quaternion>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 EvaluatePoint(float t)
    {
        t = Mathf.Clamp01(t);
        if (m_positions.Count == 0) return new Vector3(0,0,0);
        if (m_positions.Count == 1) return m_positions[0];
        if (m_positions.Count == 2) return Vector3.Lerp(m_positions[0], m_positions[1], t);
        
        int numGroups = m_positions.Count / 2;
        int groupIdx = System.Math.Min(numGroups - 1, (int)(t * numGroups));

        int idx0 = 2 * groupIdx;
        int idx1 = System.Math.Min(m_positions.Count - 1, idx0 + 1);
        int idx2 = System.Math.Min(m_positions.Count - 1, idx0 + 2);

        float groupStartTime = (float)groupIdx / (float)numGroups;
        float groupEndTime = (float)(groupIdx + 1) / (float)numGroups;
        float tLocal = (t - groupStartTime) / (groupEndTime - groupStartTime);

        float omt =  1.0f - tLocal;
        return omt * omt * m_positions[idx0]
            + 2.0f * omt * tLocal * m_positions[idx1]
            + tLocal * tLocal * m_positions[idx2];
    }
}
