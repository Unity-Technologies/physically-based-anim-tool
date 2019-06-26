using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    public static Transform GetNext(this Transform transform)
    {
        if (transform.childCount > 0)
            return transform.GetChild(0);

        while (transform.parent != null)
        {
            int siblingIndex = transform.GetSiblingIndex();
            
            if (siblingIndex < transform.parent.childCount - 1)
                return transform.parent.GetChild(siblingIndex + 1);

            transform = transform.parent;
        }

        return null;
    }
}
