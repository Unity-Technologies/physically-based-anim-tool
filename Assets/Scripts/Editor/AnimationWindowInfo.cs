using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class AnimationWindowInfo
{
    static Type s_AnimationWindowType;
    static Type s_AnimEditorType;
    static Type s_WindowStateType;
    static FieldInfo s_AnimWindowStateField;
    static PropertyInfo s_CurrentTimeProp;
    static FieldInfo s_AnimEditorField;
    static object s_AnimationWindow;
    static object s_AnimEditor;
    static object s_AnimWindowState;
    static AnimationClip s_Clip;
    static float s_CurrentTime;
    static EditorCurveBinding s_ComXBinding;
    static EditorCurveBinding s_ComYBinding;
    static EditorCurveBinding s_ComZBinding;
    static AnimationCurve s_Curve;
    static Keyframe[] s_XKeyframes;
    static Keyframe[] s_YKeyframes;
    static Keyframe[] s_ZKeyframes;

    /*static UnityEngine.Object GetOpenAnimationWindow()
    {
        UnityEngine.Object[] openAnimationWindows = Resources.FindObjectsOfTypeAll(GetAnimationWindowType());
        if (openAnimationWindows.Length > 0)
        {
            return openAnimationWindows[0];
        }
        return null;
    }*/

    [MenuItem("Tools/Print")]
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
        Type clipPopupType = clipPopupField.FieldType;

        MethodInfo getOrderedClipsMethod = clipPopupType.GetMethod("GetOrderedClipList", flags);

        FieldInfo stateField = animEditorType.GetField("m_State", flags);

        //Object state = stateField.GetValue(animEditor) as Object;
        
        Type stateType = stateField.FieldType;

        PropertyInfo currentTimeProp = stateType.GetProperty("currentTime", flags);

        //float currentTime = (float)currentTimeProp.GetValue(state);
        
        //Debug.Log(currentTime);

        PrintInfoAboutType(clipPopupField.FieldType);
    }

    public static void PrintInfoAboutType(Type type)
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
    }

    static void GetTypeInfo()
    {
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        
        s_AnimationWindowType = System.Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
        s_AnimEditorField = s_AnimationWindowType.GetField("m_AnimEditor", flags);
 
        s_AnimEditorType = s_AnimEditorField.FieldType;
        s_AnimWindowStateField = s_AnimEditorType.GetField("m_State", flags);
        s_WindowStateType = s_AnimWindowStateField.FieldType;
        s_CurrentTimeProp = s_WindowStateType.GetProperty("currentTime", flags);
    }

    static void GetBindingInfo()
    {
        // TODO: someHierarchyTransform cannot be set from selection.
        Transform someHierarchyTransform = Selection.activeTransform;
        Animator animator = someHierarchyTransform.GetComponentInChildren<Animator>();
        if (animator == null)
            animator = someHierarchyTransform.GetComponent<Animator>();
        if (animator == null)
            animator = someHierarchyTransform.GetComponentInParent<Animator>();
        CentredSkinnedMesh centredSkinnedMesh = someHierarchyTransform.GetComponentInChildren<CentredSkinnedMesh>();
        if (centredSkinnedMesh == null)
            centredSkinnedMesh = someHierarchyTransform.GetComponent<CentredSkinnedMesh>();
        if (centredSkinnedMesh == null)
            centredSkinnedMesh = someHierarchyTransform.GetComponentInParent<CentredSkinnedMesh>();
        
        string path = AnimationUtility.CalculateTransformPath(centredSkinnedMesh.transform, animator.transform);
        
        // TODO: check the inPropertyName parameter for these.
        s_ComXBinding = EditorCurveBinding.FloatCurve(path, typeof(CentredSkinnedMesh), "com.position.x");
        s_ComYBinding = EditorCurveBinding.FloatCurve(path, typeof(CentredSkinnedMesh), "com.position.y");
        s_ComZBinding = EditorCurveBinding.FloatCurve(path, typeof(CentredSkinnedMesh), "com.position.z");
    }

    static void GetWindowInfo()
    {
        if(s_CurrentTimeProp == null)
            GetTypeInfo();
        
        Object[] openAnimationWindows = Resources.FindObjectsOfTypeAll(s_AnimationWindowType);
        s_AnimationWindow = openAnimationWindows.Length > 0 ? openAnimationWindows[0] : null;

        if (s_AnimationWindow == null)
        {
            Debug.Log("No animation window currently open.");
            return;
        }
        
        s_AnimEditor = s_AnimEditorField.GetValue(s_AnimationWindow);

        if (s_AnimEditor == null)
        {
            Debug.Log("No animation editor found in animation window.");
            return;
        }
        
        s_AnimWindowState = s_AnimWindowStateField.GetValue(s_AnimEditor);

        if (s_AnimWindowState == null)
        {
            Debug.Log("No animation window state found in animation editor.");
        }
    }

    // TODO: this needs to be called when clip changes.  cannot be lazy!
    static void GetCurrentClipInfo()
    {
        if(s_AnimWindowState == null)
            GetWindowInfo();
        
        if(s_ComZBinding.type == null)
            GetBindingInfo();

        AnimationClip clip = (AnimationClip)s_WindowStateType.InvokeMember("get_activeAnimationClip", BindingFlags.InvokeMethod | BindingFlags.Public, null, s_AnimWindowStateField.GetValue(s_AnimEditor), null);

        if (clip == null)
        {
            Debug.Log("The current clip could not be found.");
            return;
        }

        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, s_ComXBinding);

        if (curve == null)
        {
            Debug.Log("Couldn't find COM x curve on clip.");
            return;
        }
        
        s_XKeyframes = curve.keys;

        curve = AnimationUtility.GetEditorCurve(clip, s_ComYBinding);

        if (curve == null)
        {
            Debug.Log("Couldn't find COM y curve on clip.");
            return;
        }
        
        s_YKeyframes = curve.keys;

        curve = AnimationUtility.GetEditorCurve(clip, s_ComZBinding);

        if (curve == null)
        {
            Debug.Log("Couldn't find COM z curve on clip.");
            return;
        }
        
        s_ZKeyframes = curve.keys;
    }

    static void GetCurrentTime()
    {
        if(s_AnimWindowState == null)
            GetWindowInfo();
        
        s_CurrentTime = (float)s_CurrentTimeProp.GetValue(s_AnimWindowState);
    }

    public static bool GetPreviousKeyframes(out Keyframe xPrevious, out Keyframe yPrevious, out Keyframe zPrevious)
    {
        if(s_ZKeyframes == null)
            GetCurrentClipInfo();

        xPrevious = new Keyframe();
        yPrevious = new Keyframe();
        zPrevious = new Keyframe();

        GetCurrentTime();
        
        if (s_XKeyframes.Length < 2 || s_YKeyframes.Length < 2 || s_ZKeyframes.Length < 2)
            return false;

        bool foundXKey = false;
        for (int i = 1; i < s_XKeyframes.Length; i++)
        {
            Keyframe keyFrame = s_XKeyframes[i];

            if (Mathf.Approximately(keyFrame.time, s_CurrentTime))
            {
                xPrevious = s_XKeyframes[i - 1];
                foundXKey = true;
            }
        }

        bool foundYKey = false;
        for (int i = 1; i < s_YKeyframes.Length; i++)
        {
            Keyframe keyFrame = s_YKeyframes[i];

            if (Mathf.Approximately(keyFrame.time, s_CurrentTime))
            {
                yPrevious = s_YKeyframes[i - 1];
                foundYKey = true;
            }
        }

        bool foundZKey = false;
        for (int i = 1; i < s_ZKeyframes.Length; i++)
        {
            Keyframe keyFrame = s_ZKeyframes[i];

            if (Mathf.Approximately(keyFrame.time, s_CurrentTime))
            {
                zPrevious = s_ZKeyframes[i - 1];
                foundZKey = true;
            }
        }

        return foundXKey && foundYKey && foundZKey;
    }

    public static bool GetCurrentKeyframes(out Keyframe xCurrent, out Keyframe yCurrent, out Keyframe zCurrent)
    {
        if(s_ZKeyframes == null)
            GetCurrentClipInfo();

        xCurrent = new Keyframe();
        yCurrent = new Keyframe();
        zCurrent = new Keyframe();

        GetCurrentTime();
        
        bool foundXKey = false;
        for (int i = 0; i < s_XKeyframes.Length; i++)
        {
            Keyframe keyFrame = s_XKeyframes[i];

            if (Mathf.Approximately(keyFrame.time, s_CurrentTime))
            {
                xCurrent = s_XKeyframes[i];
                foundXKey = true;
            }
        }

        bool foundYKey = false;
        for (int i = 0; i < s_YKeyframes.Length; i++)
        {
            Keyframe keyFrame = s_YKeyframes[i];

            if (Mathf.Approximately(keyFrame.time, s_CurrentTime))
            {
                yCurrent = s_YKeyframes[i];
                foundYKey = true;
            }
        }

        bool foundZKey = false;
        for (int i = 0; i < s_ZKeyframes.Length; i++)
        {
            Keyframe keyFrame = s_ZKeyframes[i];

            if (Mathf.Approximately(keyFrame.time, s_CurrentTime))
            {
                zCurrent = s_ZKeyframes[i];
                foundZKey = true;
            }
        }

        return foundXKey && foundYKey && foundZKey;
    }

    public static bool GetNextKeyframes(out Keyframe xNext, out Keyframe yNext, out Keyframe zNext)
    {
        if(s_ZKeyframes == null)
            GetCurrentClipInfo();

        xNext = new Keyframe();
        yNext = new Keyframe();
        zNext = new Keyframe();

        GetCurrentTime();
        
        if (s_XKeyframes.Length < 2 || s_YKeyframes.Length < 2 || s_ZKeyframes.Length < 2)
            return false;

        bool foundXKey = false;
        for (int i = 0; i < s_XKeyframes.Length -1; i++)
        {
            Keyframe keyFrame = s_XKeyframes[i];

            if (Mathf.Approximately(keyFrame.time, s_CurrentTime))
            {
                xNext = s_XKeyframes[i + 1];
                foundXKey = true;
            }
        }

        bool foundYKey = false;
        for (int i = 0; i < s_YKeyframes.Length - 1; i++)
        {
            Keyframe keyFrame = s_YKeyframes[i];

            if (Mathf.Approximately(keyFrame.time, s_CurrentTime))
            {
                yNext = s_YKeyframes[i + 1];
                foundYKey = true;
            }
        }

        bool foundZKey = false;
        for (int i = 0; i < s_ZKeyframes.Length - 1; i++)
        {
            Keyframe keyFrame = s_ZKeyframes[i];

            if (Mathf.Approximately(keyFrame.time, s_CurrentTime))
            {
                zNext = s_ZKeyframes[i + 1];
                foundZKey = true;
            }
        }

        return foundXKey && foundYKey && foundZKey;
    }
}
