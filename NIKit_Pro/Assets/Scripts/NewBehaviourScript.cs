using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NewBehaviourScript : MonoBehaviour
{
    // Use this for initialization
    void Start ()
    {
        UIMgr.Instance.ShowForms<StartWnd>(StartWnd.FormsName);
    }

}
