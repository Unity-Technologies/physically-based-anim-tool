using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class AnimationWindowInfo
{
    static Type s_AnimationWindowType;
    static Type s_WindowStateType;
    static FieldInfo s_AnimWindowStateField;
    static FieldInfo s_AnimEditorField;

    static object s_AnimWindowState;

    //static float s_CurrentTime;
    //static EditorCurveBinding s_ComXBinding;
    //static EditorCurveBinding s_ComYBinding;
    //static EditorCurveBinding s_ComZBinding;
    //static AnimationCurve s_Curve;
    //static Keyframe[] s_XKeyframes;
    //static Keyframe[] s_YKeyframes;
    //static Keyframe[] s_ZKeyframes;
    static AnimationClip s_AnimationClip;
    static PropertyInfo s_RootGameObjectProp;
    static GameObject s_RootGameObject;

    /*static AnimationCurve s_RootTXCurve;
    static AnimationCurve s_RootTYCurve;
    static AnimationCurve s_RootTZCurve;
    static AnimationCurve s_RootQXCurve;
    static AnimationCurve s_RootQYCurve;
    static AnimationCurve s_RootQZCurve;
    static AnimationCurve s_RootQWCurve;
    static RootMotionCurves s_RootMotionCurves;*/

    static EditorCurveBinding s_RootTXCurveBinding = EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.x");
    static EditorCurveBinding s_RootTYCurveBinding = EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.y");
    static EditorCurveBinding s_RootTZCurveBinding = EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.z");
    static EditorCurveBinding s_RootQXCurveBinding = EditorCurveBinding.FloatCurve("", typeof(Animator), "RootQ.x");
    static EditorCurveBinding s_RootQYCurveBinding = EditorCurveBinding.FloatCurve("", typeof(Animator), "RootQ.y");
    static EditorCurveBinding s_RootQZCurveBinding = EditorCurveBinding.FloatCurve("", typeof(Animator), "RootQ.z");
    static EditorCurveBinding s_RootQWCurveBinding = EditorCurveBinding.FloatCurve("", typeof(Animator), "RootQ.w");


    static AnimationWindowInfo()
    {
        GetTypeInfo();
    }

    /*[MenuItem("Tools/Print")]
    public static void PrintInfo()
    {
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        //Object animWindow = GetOpenAnimationWindow();

        if (s_AnimationWindowType == null)
        {
            s_AnimationWindowType = Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
        }

        FieldInfo animEditorField = s_AnimationWindowType.GetField("m_AnimEditor", flags);
        
        //Object animEditor = animEditorField.GetValue(animWindow) as Object;
        
        Type animEditorType = animEditorField.FieldType;
        FieldInfo clipPopupField = animEditorType.GetField("m_ClipPopup", flags);

        PropertyInfo animationWindowSelectionItemProp = animEditorType.GetProperty("selection", flags);
        Type animationWindowSelectionItemType = animationWindowSelectionItemProp.PropertyType;

        PropertyInfo rootGameObjectProp = animationWindowSelectionItemType.GetProperty("rootGameObject", flags);
        
        Type clipPopupType = clipPopupField.FieldType;

        MethodInfo getOrderedClipsMethod = clipPopupType.GetMethod("GetOrderedClipList", flags);

        FieldInfo stateField = animEditorType.GetField("m_State", flags);

        //Object state = stateField.GetValue(animEditor) as Object;
        
        Type stateType = stateField.FieldType;

        PropertyInfo currentTimeProp = stateType.GetProperty("currentTime", flags);

        //float currentTime = (float)currentTimeProp.GetValue(state);
        
        //Debug.Log(currentTime);

        PrintInfoAboutType(animationWindowSelectionItemProp.PropertyType);
    }

    static void PrintInfoAboutType(Type type)
    {
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        string log = "Fields\n";
        FieldInfo[] fields = type.GetFields(flags);
        for (int i = 0; i < fields.Length; i++)
        {
            log += fields[i].FieldType + " " + fields[i].Name + "\n";
        }

        log += "\nProperties\n";
        PropertyInfo[] props = type.GetProperties(flags);
        for (int i = 0; i < props.Length; i++)
        {
            log += props[i].PropertyType + " " + props[i].Name + "\n";
        }

        log += "\nMethods\n";
        MethodInfo[] methods = type.GetMethods(flags);
        for (int i = 0; i < methods.Length; i++)
        {
            log += methods[i].ReturnType + " " + methods[i].Name + "\n";
        }
        
        Debug.Log(log);
    }*/

    public static void GetTypeInfo()
    {
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        s_AnimationWindowType = System.Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
        s_AnimEditorField = s_AnimationWindowType.GetField("m_AnimEditor", flags);

        Type animEditorType = s_AnimEditorField.FieldType;
        s_AnimWindowStateField = animEditorType.GetField("m_State", flags);
        s_WindowStateType = s_AnimWindowStateField.FieldType;

    }

    public static AnimationClip GetClip()
    {
        if (s_WindowStateType == null)
            GetTypeInfo();

        Object[] openAnimationWindows = Resources.FindObjectsOfTypeAll(s_AnimationWindowType);
        object animationWindow = openAnimationWindows.Length > 0 ? openAnimationWindows[0] : null;

        if (animationWindow == null)
            Debug.Log("No animation window currently open.");

        var animEditor = s_AnimEditorField.GetValue(animationWindow);

        if (animEditor == null)
            Debug.Log("No animation editor found in animation window.");

        object animWindowState = s_AnimWindowStateField.GetValue(animEditor);
        s_AnimationClip = s_WindowStateType.GetProperty("activeAnimationClip").GetValue(animWindowState, null) as AnimationClip;
        return s_AnimationClip;
    }

    public static RootMotionCurves GetRootMotionCurves()
    {
        GetClip();

        return new RootMotionCurves
        {
            rootTXCurve = AnimationUtility.GetEditorCurve(s_AnimationClip, s_RootTXCurveBinding),
            rootTYCurve = AnimationUtility.GetEditorCurve(s_AnimationClip, s_RootTYCurveBinding),
            rootTZCurve = AnimationUtility.GetEditorCurve(s_AnimationClip, s_RootTZCurveBinding),
            rootQXCurve = AnimationUtility.GetEditorCurve(s_AnimationClip, s_RootQXCurveBinding),
            rootQYCurve = AnimationUtility.GetEditorCurve(s_AnimationClip, s_RootQYCurveBinding),
            rootQZCurve = AnimationUtility.GetEditorCurve(s_AnimationClip, s_RootQZCurveBinding),
            rootQWCurve = AnimationUtility.GetEditorCurve(s_AnimationClip, s_RootQWCurveBinding)
        };
    }

    public static void WriteRootMotionCurves(RootMotionCurves rootMotionCurves)
    {
        AnimationUtility.SetEditorCurve(s_AnimationClip, s_RootTXCurveBinding, rootMotionCurves.rootTXCurve);
        AnimationUtility.SetEditorCurve(s_AnimationClip, s_RootTYCurveBinding, rootMotionCurves.rootTYCurve);
        AnimationUtility.SetEditorCurve(s_AnimationClip, s_RootTZCurveBinding, rootMotionCurves.rootTZCurve);
        AnimationUtility.SetEditorCurve(s_AnimationClip, s_RootQXCurveBinding, rootMotionCurves.rootQXCurve);
        AnimationUtility.SetEditorCurve(s_AnimationClip, s_RootQYCurveBinding, rootMotionCurves.rootQYCurve);
        AnimationUtility.SetEditorCurve(s_AnimationClip, s_RootQZCurveBinding, rootMotionCurves.rootQZCurve);
        AnimationUtility.SetEditorCurve(s_AnimationClip, s_RootQWCurveBinding, rootMotionCurves.rootQWCurve);
    }

    public static void WriteRootTransformCurves(TransformCurves rootTransformCurves)
    {
        // TODO: complete me
        AnimationUtility.SetEditorCurve(s_AnimationClip, s_RootTXCurveBinding, rootTransformCurves.m_PosX);
        AnimationUtility.SetEditorCurve(s_AnimationClip, s_RootTYCurveBinding, rootTransformCurves.m_PosY);
        AnimationUtility.SetEditorCurve(s_AnimationClip, s_RootTZCurveBinding, rootTransformCurves.m_PosZ);

        AnimationUtility.SetEditorCurve(s_AnimationClip, s_RootQXCurveBinding, rootTransformCurves.m_RotX);
        AnimationUtility.SetEditorCurve(s_AnimationClip, s_RootQYCurveBinding, rootTransformCurves.m_RotY);
        AnimationUtility.SetEditorCurve(s_AnimationClip, s_RootQZCurveBinding, rootTransformCurves.m_RotZ);
        AnimationUtility.SetEditorCurve(s_AnimationClip, s_RootQWCurveBinding, rootTransformCurves.m_RotW);
    }
}
