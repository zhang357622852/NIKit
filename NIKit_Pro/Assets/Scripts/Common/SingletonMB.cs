using UnityEngine;

/// <summary>
/// Singleton pattern By MonoBrhaviour
/// </summary>
public abstract class SingletonMB<T> : MonoBehaviour where T : Component
{
    protected static T mInstance;

    public static T Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = FindObjectOfType<T>();

                if (mInstance == null)
                {
                    GameObject obj = new GameObject();
                    //obj.hideFlags = HideFlags.HideAndDontSave;
                    mInstance = obj.AddComponent<T>();
                }
            }

            return mInstance;
        }
    }

    /// <summary>
    /// On awake, we initialize our instance. Make sure to call base.Awake() in override if you need awake.
    /// </summary>
    protected virtual void Awake()
    {
        mInstance = this as T;
    }
}


