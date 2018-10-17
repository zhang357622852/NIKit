using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace WinMi
{
    public class AutoSortPrefabEditor : EditorWindow
    {
        /// <summary>
        /// 在Assets视图下选中制作好的UI预制体,调用这个方法就会给UIPanel的depth排序
        /// </summary>
        //[MenuItem("MyTools/AutoSortPrefab(预制体UIPanel排序)")]
        public static void AutoSortPrefabDepth()
        {
            Transform[] list = Selection.GetFiltered<Transform>(SelectionMode.TopLevel);
            foreach (var item in list)
            {
                UIPanel[] panels = item.GetComponentsInChildren<UIPanel>(true);
                if (panels == null)
                    return;

                List<UIPanel> panelList = new List<UIPanel>();
                foreach (var panelItem in panels)
                {
                    panelList.Add(panelItem);
                }

                panelList.Sort(
                    (a, b) => { return a.depth.CompareTo(b.depth); }
                    );


                int depthIndex = 0;
                int lastOldDepth = 0;
                for (int i = 0; i < panelList.Count; i++)
                {
                    if (i == 0)
                    {
                        lastOldDepth = panelList[i].depth;
                        panelList[i].depth = depthIndex;
                    }
                    else
                    {
                        if (panelList[i].depth == lastOldDepth)
                            panelList[i].depth = depthIndex;
                        else
                        {
                            lastOldDepth = panelList[i].depth;
                            depthIndex++;
                            panelList[i].depth = depthIndex;
                        }
                    }
                }

                Debug.Log(string.Format("====================预制体 {0}  已排序===================", item.name));
            }

        }
    }
}
