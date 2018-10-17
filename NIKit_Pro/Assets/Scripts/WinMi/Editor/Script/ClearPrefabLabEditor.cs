using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WinMi
{
    public class ClearPrefabLabEditor : EditorWindow
    {
        /// <summary>
        /// 在Assets视图下选中制作好的UI预制体,调用这个方法清理prefab中的所有UILabel为空字符串
        /// </summary>
        [MenuItem("Tools/ClearPrefabLabels(清理Prefab中UILabel)")]
        public static void ClearPrefabLabel()
        {
            UILabel[] list = Selection.GetFiltered<UILabel>(SelectionMode.Deep);
            foreach (var item in list)
            {
                item.text = "";

            }

            Debug.Log("清理Prefab中UILabel");
        }
    }
}
