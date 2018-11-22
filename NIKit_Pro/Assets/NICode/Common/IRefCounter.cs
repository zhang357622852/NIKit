/*
 * 引用计数可以用在对象需要重复使用和缓冲池配合使用
 * 例如：一张图片资源Teture2D对象被多次引用，当计数RefCount=0就可以真正释放掉
 * 属性refCount没有声明set,主要是为了在实现的时候用private限制
 */
using System.Collections;
using System.Collections.Generic;

    /// <summary>
    /// Reference Count
    /// </summary>
    public interface IRefCounter
    {
        int refCount { get;}

        void Retain();

        void Release();
    }

