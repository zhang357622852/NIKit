using UnityEngine;
using System.Collections;

public class UIScreenAdjustor : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
        float scale = Game.CalcWndScale();
        transform.localScale = new Vector3(scale, scale, scale);
	}
}
