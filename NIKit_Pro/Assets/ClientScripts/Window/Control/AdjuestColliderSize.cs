/// <summary>
/// AdjuestColliderSize.cs
/// Create by fucj 2014-12-08
/// 调整碰撞盒大小
/// </summary>

using UnityEngine;
using System.Collections;

public class AdjuestColliderSize : MonoBehaviour {

    BoxCollider box_collider;
    UIPanel panel;

	// Use this for initialization
	void Start () {
        box_collider = gameObject.GetComponent<BoxCollider>();
        panel = gameObject.GetComponent<UIPanel>();

        AdjuestCollider();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// 调整碰撞盒大小
    /// </summary>
    void AdjuestCollider()
    {
        if (box_collider == null ||
            panel == null)
            return;

        box_collider.size = new Vector3(panel.GetViewSize().x, panel.GetViewSize().y, 1.0f);
    }
}
