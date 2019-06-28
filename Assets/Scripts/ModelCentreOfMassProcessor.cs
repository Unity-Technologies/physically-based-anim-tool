using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ModelCentreOfMassProcessor : AssetPostprocessor
{
    void OnPostprocessModel (GameObject g)
    {
        SkinnedMeshRenderer[] allSkinnedMeshRenderers = g.GetComponentsInChildren<SkinnedMeshRenderer> ();
        
        CentredSkinnedMesh centredSkinnedMesh = g.AddComponent<CentredSkinnedMesh> ();
        
        for (int i = 0; i < allSkinnedMeshRenderers.Length; i++)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = allSkinnedMeshRenderers[0];

            Mesh mesh = skinnedMeshRenderer.sharedMesh;
        
            Transform[] bones = skinnedMeshRenderer.bones;
        
            float[] boneMasses = TetrahedronVolumeToBoneMasses (mesh, bones);

            centredSkinnedMesh.AddBoneMasses (bones, boneMasses);
        }
    }

    static float[] TetrahedronVolumeToBoneMasses (Mesh mesh, Transform[] bones)
    {
        int[] triangles = mesh.triangles;
        BoneWeight[] boneWeights = mesh.boneWeights;
        Vector3[] verts = mesh.vertices;
        Vector3[] normals = mesh.normals;
        
        float[] boneMasses = new float[bones.Length];

        // Go through all the bones...
        for (int i = 0; i < bones.Length; i++)
        {
            Vector3 bonePosition = bones[i].position;
            float mass = 0f;

            // For each bone go through all the triangles...
            for (int j = 0; j < triangles.Length; j += 3)
            {
                // Find the verts of each triangles.
                int vertIndex0 = triangles[j];
                int vertIndex1 = triangles[j + 1];
                int vertIndex2 = triangles[j + 2];
                
                // Find the bone weights for each of the verts.
                BoneWeight boneWeight0 = boneWeights[vertIndex0];
                BoneWeight boneWeight1 = boneWeights[vertIndex1];
                BoneWeight boneWeight2 = boneWeights[vertIndex2];

                bool isBoneWeight0Index0CurrentBone = boneWeight0.boneIndex0 == i;
                bool isBoneWeight0Index1CurrentBone = boneWeight0.boneIndex1 == i;
                bool isBoneWeight0Index2CurrentBone = boneWeight0.boneIndex2 == i;
                bool isBoneWeight0Index3CurrentBone = boneWeight0.boneIndex3 == i;
                
                bool isBoneWeight1Index0CurrentBone = boneWeight1.boneIndex0 == i;
                bool isBoneWeight1Index1CurrentBone = boneWeight1.boneIndex1 == i;
                bool isBoneWeight1Index2CurrentBone = boneWeight1.boneIndex2 == i;
                bool isBoneWeight1Index3CurrentBone = boneWeight1.boneIndex3 == i;
                
                bool isBoneWeight2Index0CurrentBone = boneWeight2.boneIndex0 == i;
                bool isBoneWeight2Index1CurrentBone = boneWeight2.boneIndex1 == i;
                bool isBoneWeight2Index2CurrentBone = boneWeight2.boneIndex2 == i;
                bool isBoneWeight2Index3CurrentBone = boneWeight2.boneIndex3 == i;
                
                // Is vert 0 weighted to the current bone?
                if(isBoneWeight0Index0CurrentBone || isBoneWeight0Index1CurrentBone || isBoneWeight0Index2CurrentBone || isBoneWeight0Index3CurrentBone)
                {
                    // Get the weight of vert 0 to the current bone.
                    float weight = isBoneWeight0Index0CurrentBone ? boneWeight0.weight0 : isBoneWeight0Index1CurrentBone ? boneWeight0.weight1 : isBoneWeight0Index2CurrentBone ? boneWeight0.weight2 : boneWeight0.weight3;
                    
                    // Is vert 1 weighted to the current bone?
                    if(isBoneWeight1Index0CurrentBone || isBoneWeight1Index1CurrentBone || isBoneWeight1Index2CurrentBone || isBoneWeight1Index3CurrentBone)
                    {
                        // Add the weight of vert 1 to the total.
                        weight += isBoneWeight1Index0CurrentBone ? boneWeight1.weight0 : isBoneWeight1Index1CurrentBone ? boneWeight1.weight1 : isBoneWeight1Index2CurrentBone ? boneWeight1.weight2 : boneWeight1.weight3;
                        
                        // Is vert 2 weighted tot he current bone?
                        if (isBoneWeight2Index0CurrentBone || isBoneWeight2Index1CurrentBone || isBoneWeight2Index2CurrentBone || isBoneWeight2Index3CurrentBone)
                        {
                            // Add the weight of vert 2 to the total.
                            weight += isBoneWeight2Index0CurrentBone ? boneWeight2.weight0 : isBoneWeight2Index1CurrentBone ? boneWeight2.weight1 : isBoneWeight2Index2CurrentBone ? boneWeight2.weight2 : boneWeight2.weight3;
                            
                            // Average out the weight total across the 3 verts.
                            weight /= 3f;
                            
                            // Find the volume of the triangle to bone tetrahedron.
                            float volume = VolumeOfTetrahedron (bonePosition, verts[vertIndex0], verts[vertIndex1], verts[vertIndex2]);
                            
                            // Find the centre of the triangle.
                            Vector3 triCentre = (verts[vertIndex0] + verts[vertIndex1] + verts[vertIndex2]) / 3f;
                            
                            // Find the vector from the bone to the centre of the triangle.
                            Vector3 boneToTriCentre = triCentre - bonePosition;
                            
                            // Find whether the triangle is facing towards or away from the bone.
                            Vector3 averageNormalForTri = (normals[vertIndex0] + normals[vertIndex1] + normals[vertIndex2]) / 3f;
                            float dot = Vector3.Dot (boneToTriCentre, averageNormalForTri);
                            float dotSign = Mathf.Sign (dot);

                            // Add the weighted volume to the mass of the bone.
                            mass += dotSign * weight * volume;
                        }
                    }
                }
            }

            boneMasses[i] = mass;
        }

        return boneMasses;
    }

    static float VolumeOfTetrahedron (Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        return Vector3.Dot (a - d, Vector3.Cross (b - d, c - d)) / 6f;
    }

    static float[] WeightedVertDistanceToBoneMasses (Mesh mesh, Transform[] bones)
    {
        BoneWeight[] boneWeights = mesh.boneWeights;
        Vector3[] verts = mesh.vertices;

        float[] boneMasses = new float[bones.Length];
        float longestDistanceFromBoneToVert = 0f;

        for (int i = 0; i < bones.Length; i++)
        {
            Vector3 bonePosition = bones[i].position;
            float averageBoneWeight = 0f;
            int affectedVertCount = 0;
            
            for (int j = 0; j < boneWeights.Length; j++)
            {
                BoneWeight boneWeight = boneWeights[j];

                if (boneWeight.boneIndex0 == i)
                {
                    float boneToVertDistance = Vector3.Distance (bonePosition, verts[j]);
                    averageBoneWeight += boneToVertDistance * boneWeight.weight0;
                    affectedVertCount++;

                    if (boneToVertDistance > longestDistanceFromBoneToVert)
                        longestDistanceFromBoneToVert = boneToVertDistance;
                }
                else if (boneWeight.boneIndex1 == i)
                {
                    float boneToVertDistance = Vector3.Distance (bonePosition, verts[j]);
                    averageBoneWeight += boneToVertDistance * boneWeight.weight1;
                    affectedVertCount++;

                    if (boneToVertDistance > longestDistanceFromBoneToVert)
                        longestDistanceFromBoneToVert = boneToVertDistance;
                }
                else if (boneWeight.boneIndex2 == i)
                {
                    float boneToVertDistance = Vector3.Distance (bonePosition, verts[j]);
                    averageBoneWeight += boneToVertDistance * boneWeight.weight2;
                    affectedVertCount++;

                    if (boneToVertDistance > longestDistanceFromBoneToVert)
                        longestDistanceFromBoneToVert = boneToVertDistance;
                }
                else if (boneWeight.boneIndex3 == i)
                {
                    float boneToVertDistance = Vector3.Distance (bonePosition, verts[j]);
                    averageBoneWeight += boneToVertDistance * boneWeight.weight3;
                    affectedVertCount++;

                    if (boneToVertDistance > longestDistanceFromBoneToVert)
                        longestDistanceFromBoneToVert = boneToVertDistance;
                }
            }

            if (longestDistanceFromBoneToVert > 0f)
                averageBoneWeight /= longestDistanceFromBoneToVert;
            
            if(affectedVertCount > 0)
                averageBoneWeight /= affectedVertCount;

            boneMasses[i] = averageBoneWeight;
        }

        return boneMasses;
    }
}
