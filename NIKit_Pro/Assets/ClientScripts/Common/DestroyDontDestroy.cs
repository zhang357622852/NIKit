using UnityEngine;
using System;
using System.Collections;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class DestroyDontDestroy : MonoBehaviour
{
    void Update()
    {
        if (!UnityEditor.EditorApplication.isPlaying)
            GameObject.DestroyImmediate(gameObject);
    }
}
#endif