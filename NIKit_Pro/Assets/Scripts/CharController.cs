using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{
    public float mGravity = -10;
    protected Vector2 mSpeed;

    // Use this for initialization
    void Start ()
    {

	}

	// Update is called once per frame
	void Update ()
    {
        mSpeed.y += mGravity * Time.deltaTime;
        Vector2 newPos = mSpeed * Time.deltaTime;
        transform.Translate(newPos, Space.World);
	}
}
