using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CentredSkinnedMesh))]
public class CentredSkinnedMeshEditor : Editor
{
    SerializedProperty m_CentreOfMassProp;
    SerializedProperty m_BoneMassesProp;
    SerializedProperty m_SkeletonRootProp;

    static readonly GUILayoutOption k_BoneWidth = GUILayout.Width (150f);
    static readonly GUILayoutOption k_SliderWidth = GUILayout.Width (150f);
    static readonly GUILayoutOption k_RelativeMassWidth = GUILayout.Width (80f);
    static readonly GUILayoutOption k_CalculatedMassWidth = GUILayout.Width (80f);

    void OnEnable ()
    {
        m_CentreOfMassProp = serializedObject.FindProperty("m_CentreOfMass");
        m_BoneMassesProp = serializedObject.FindProperty ("m_BoneMasses");
        m_SkeletonRootProp = serializedObject.FindProperty ("m_SkeletonRoot");
    }

    public override void OnInspectorGUI ()
    {
        serializedObject.Update ();
        
        EditorGUILayout.BeginHorizontal ();

        EditorGUILayout.PropertyField(m_SkeletonRootProp);

        EditorGUILayout.LabelField ("Bone", k_BoneWidth);
        EditorGUILayout.LabelField ("Relative Density", k_SliderWidth);
        EditorGUILayout.LabelField ("Relative Mass", k_RelativeMassWidth);
        EditorGUILayout.LabelField ("Calculated Mass", k_CalculatedMassWidth);
            
        EditorGUILayout.EndHorizontal ();

        for (int i = 0; i < m_BoneMassesProp.arraySize; i++)
        {
            SerializedProperty boneMassProp = m_BoneMassesProp.GetArrayElementAtIndex (i);

            SerializedProperty boneProp = boneMassProp.FindPropertyRelative ("bone");
            SerializedProperty relativeDensityProp = boneMassProp.FindPropertyRelative ("relativeDensity");
            SerializedProperty weightedMassProp = boneMassProp.FindPropertyRelative ("mass");

            EditorGUILayout.BeginHorizontal ();
            
            EditorGUILayout.LabelField (boneProp.objectReferenceValue.name, k_BoneWidth);
            EditorGUILayout.PropertyField (relativeDensityProp, GUIContent.none, k_SliderWidth);
            EditorGUILayout.LabelField (weightedMassProp.floatValue.ToString(), k_RelativeMassWidth);
            EditorGUILayout.LabelField ((relativeDensityProp.floatValue * weightedMassProp.floatValue).ToString(), k_CalculatedMassWidth);
            
            EditorGUILayout.EndHorizontal ();
        }

        serializedObject.ApplyModifiedProperties ();
    }

    public void OnSceneGUI()
    {
        float size = HandleUtility.GetHandleSize(m_CentreOfMassProp.vector3Value) * 0.5f;
        Vector3 snap = Vector3.one * 0.01f;

        EditorGUI.BeginChangeCheck();

        Quaternion oldTargetOrientation = Quaternion.identity;
        Matrix4x4 oldCom = Matrix4x4.TRS(m_CentreOfMassProp.vector3Value, oldTargetOrientation, new Vector3(1,1,1));
        Quaternion newTargetOrientation = Quaternion.identity;
        Vector3 newTargetPosition = Handles.PositionHandle(m_CentreOfMassProp.vector3Value, newTargetOrientation);
        // todo; detect active tool and show rotation handle if necessary

        Matrix4x4 newCom = Matrix4x4.TRS(newTargetPosition, newTargetOrientation, new Vector3(1,1,1));

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Centre Of Mass Position");

            // TODO: put root movement code here.
            GameObject skeletonRoot = m_SkeletonRootProp.objectReferenceValue as GameObject;
            Matrix4x4 worldFromModel = skeletonRoot.transform.localToWorldMatrix;

            Matrix4x4 newFromOld = oldCom.inverse * newCom;
            Matrix4x4 offsetRoot = newFromOld * worldFromModel;

            // todo can we just write the 4x4 somehow?
            // todo we need to put in undo markers here?
            skeletonRoot.transform.position = offsetRoot.GetColumn(3);
            skeletonRoot.transform.rotation = Quaternion.LookRotation(offsetRoot.GetColumn(2), offsetRoot.GetColumn(1));

            Debug.Log("moving");
        }
    }
}
