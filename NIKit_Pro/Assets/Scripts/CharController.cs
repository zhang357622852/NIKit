using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{
    public float mGravity = -10;
    public Vector2 mSpeed;
    public BoxCollider2D mBoxCol;
    public Rect mBoxColRect;

    // Use this for initialization
    void Start ()
    {
        mBoxCol = GetComponent<BoxCollider2D>();



        //Debug.Log("===x=====" + mBoxCol.bounds.min.x);
        //Debug.Log("==y======" + mBoxCol.bounds.min.y);
    }

	// Update is called once per frame
	void Update ()
    {
        DrawBoxCol();
        //mSpeed.y += mGravity * Time.deltaTime;
        //Vector2 newPos = mSpeed * Time.deltaTime;
        //transform.Translate(newPos, Space.World);
    }

    private void DrawBoxCol()
    {
        mBoxColRect = new Rect(mBoxCol.bounds.min.x, mBoxCol.bounds.min.y, mBoxCol.size.x, mBoxCol.size.y);
        Debug.DrawLine(new Vector2(mBoxColRect.xMin, mBoxColRect.center.y), new Vector2(mBoxColRect.xMax, mBoxColRect.center.y), Color.yellow);
        Debug.DrawLine(new Vector2(mBoxColRect.center.x, mBoxColRect.yMin), new Vector2(mBoxColRect.center.x, mBoxColRect.yMax), Color.yellow);
    }
}
