
/// <summary>
/// Singleton pattern
/// </summary>
public abstract class Singleton<T> where T : class, new()
{
    protected static T mInstance;

    private static readonly object mPadlock = new object();

    public static T Instance
    {
        get
        {
            if (mInstance == null)
            {
                lock (mPadlock)
                {
                    if (mInstance == null)
                    {
                        mInstance = new T();
                    }
                }
            }

            return mInstance;
        }
    }
}


