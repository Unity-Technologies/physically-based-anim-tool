using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CentredSkinnedMesh))]
public class CentredSkinnedMeshEditor : Editor
{
    SerializedProperty m_BoneMassesProp;

    static readonly GUILayoutOption k_BoneWidth = GUILayout.Width (150f);
    static readonly GUILayoutOption k_SliderWidth = GUILayout.Width (150f);
    static readonly GUILayoutOption k_RelativeMassWidth = GUILayout.Width (80f);
    static readonly GUILayoutOption k_CalculatedMassWidth = GUILayout.Width (80f);

    void OnEnable ()
    {
        m_BoneMassesProp = serializedObject.FindProperty ("m_BoneMasses");
    }

    public override void OnInspectorGUI ()
    {
        serializedObject.Update ();
        
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
}
