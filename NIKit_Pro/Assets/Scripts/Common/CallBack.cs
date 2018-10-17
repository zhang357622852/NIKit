/// <summary>
/// CallBack.cs
/// Copy from zhangyg 2014-10-22
/// 任务处理，一般用在回调中
/// </summary>

public class CallBack
{
    /// <summary>
    /// 任务处理的代理 
    /// </summary>
    public delegate void TaskCallback(object para, params object[] _params);
    
    private TaskCallback mCallback;
    private object mParam;

    public CallBack(TaskCallback cb)
    {
        this.mCallback = cb;
    }
    
    public CallBack(TaskCallback cb, object _para)
    {
        this.mCallback = cb;
        this.mParam = _para;
    }
    
    /// <summary>
    /// 调度入口 
    /// </summary>
    public void Go(params object[] _params)
    {
        mCallback(this.mParam, _params);
    }
}
