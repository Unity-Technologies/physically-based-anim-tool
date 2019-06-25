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
    SerializedProperty m_animatorGO;
    
    static readonly GUILayoutOption k_BoneWidth = GUILayout.Width (150f);
    static readonly GUILayoutOption k_SliderWidth = GUILayout.Width (150f);
    static readonly GUILayoutOption k_RelativeMassWidth = GUILayout.Width (80f);
    static readonly GUILayoutOption k_CalculatedMassWidth = GUILayout.Width (80f);

    private Vector3 previousComPos;

    void OnEnable ()
    {
        m_CentreOfMassProp = serializedObject.FindProperty("m_CentreOfMass");
        m_BoneMassesProp = serializedObject.FindProperty ("m_BoneMasses");
        m_animatorGO = serializedObject.FindProperty("animatorGO");
        previousComPos = m_CentreOfMassProp.vector3Value;
    }

    public override void OnInspectorGUI ()
    {
        serializedObject.Update ();

        EditorGUILayout.PropertyField(m_animatorGO);

        EditorGUILayout.BeginHorizontal ();
            
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

        CentredSkinnedMesh go = m_animatorGO.serializedObject.targetObject as CentredSkinnedMesh;

        EditorGUI.BeginChangeCheck();
        Vector3 newTargetPosition = Handles.PositionHandle(m_CentreOfMassProp.vector3Value, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Centre Of Mass Position");
            if(m_animatorGO != null)
            {
                if(go != null)
                {
                    go.transform.position += (newTargetPosition - previousComPos);
                }
            }
        }
        previousComPos = newTargetPosition;
    }
}
