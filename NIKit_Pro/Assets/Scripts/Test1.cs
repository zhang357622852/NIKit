using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test1 : MonoBehaviour
{
    public GameObject mSprite;

    private float mA = 0;
    private Vector2 mPos;
    private float s = 0;
    private float v0 = 0f;

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
        mPos = mSprite.transform.localPosition;

        if (mPos.x > 2000f)
            mA = -0;

        s = v0 * Time.deltaTime + 1f / 2 * mA * Mathf.Pow(Time.deltaTime, 2);
        v0 = v0 + mA * Time.deltaTime;
        mPos = new Vector2(mPos.x + s, mPos.y);

        mSprite.transform.Translate(mPos);
	}
}
