﻿using System;
using UnityEngine;

public class CentredSkinnedMesh : MonoBehaviour
{
    [Serializable]
    public struct BoneMass
    {
        public Transform bone;
        [Range(minRelativeMass, maxRelativeMass)]
        public float relativeDensity;
        public float mass;

        public const float minRelativeMass = 0.2f;
        public const float maxRelativeMass = 2f;

        public BoneMass (Transform bone, float mass)
        {
            this.bone = bone;
            relativeDensity = 1f;
            this.mass = mass;
        }

        public Vector3 GetBoneCentreOfMass ()
        {
            return bone.position * relativeDensity * mass;
        }
    }


    [SerializeField]
    BoneMass[] m_BoneMasses;

    public void SetBoneMasses (Transform[] bones, float[] weightedMasses)
    {
        if(bones.Length != weightedMasses.Length)
            throw new UnityException("Bones and weighted masses are not the same length.  Make sure there is one weighted mass per bone.");
        
        m_BoneMasses = new BoneMass[bones.Length];

        for (int i = 0; i < m_BoneMasses.Length; i++)
        {
            m_BoneMasses[i] = new BoneMass (bones[i], weightedMasses[i]);
        }
    }

    public Vector3 CalculateCentreOfMass ()
    {
        float summedWeight = 0f;
        Vector3 com = Vector3.zero;
        for (int i = 0; i < m_BoneMasses.Length; i++)
        {
            com += m_BoneMasses[i].GetBoneCentreOfMass ();
            summedWeight += m_BoneMasses[i].relativeDensity * m_BoneMasses[i].mass;
        }

        com /= summedWeight;
        
        return com;
    }
}
