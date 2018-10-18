/// <summary>
/// ThreadedAction.cs
/// Created by lic 2017-03-22
/// 进程
/// </summary>

using System;
using System.Collections;
using System.Threading;

public class ThreadedAction
{
    private bool _isDone = false;

    private Action mCallback;

    public ThreadedAction(Action action, Action callBack = null)
    {
        mCallback = callBack;

        Thread thread = new Thread (() => {
            if (action != null)
                action ();
            _isDone = true;
        });

        thread.Start ();
    }

    public ThreadedAction(ParameterizedThreadStart action, object _para, Action callBack = null)
    {
        mCallback = callBack;

        Thread thread = new Thread (() => {
            if (action != null)
                action (_para);
            _isDone = true;
        });

        thread.Start ();
    }

    public IEnumerator WaitForComplete()
    {
        while (!_isDone)
        {
            if(mCallback != null)
                mCallback ();

            yield return null;
        }
    }
}
