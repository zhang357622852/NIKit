using UnityEngine;
using System.Collections;

public class PlayDropAnim : MonoBehaviour {

	// Use this for initialization
	void OnDropAnimOver ()
    {
        DropEffect de = transform.parent.GetComponent<DropEffect>();

        if(de == null)
            return;

        de.OnDropFinish();
	}
	
	// Update is called once per frame
	void OnPickAnimOver ()
    {
        DropEffect de = transform.parent.GetComponent<DropEffect>();

        if(de == null)
            return;

        de.OnPickFinish();
	}
}
