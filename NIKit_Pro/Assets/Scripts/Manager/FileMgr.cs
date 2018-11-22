/// <summary>
/// FileMgr.cs
/// Copy from zhangyg 2014-10-22
/// 文件/目录的常用接口封装
/// </summary>

using System;
using System.IO;
using System.Text;
using UnityEngine;

// 文件/目录的常用接口封装
public class FileMgr
{
    // 写入文件
    public static bool WriteFile(string path, byte[] data)
    {
        // 确保目录存在
        CreateDirectory(Path.GetDirectoryName(path));

        for (int i = 0; i < 10; i++)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                fs.Write(data, 0, data.Length);
                fs.Close();

                FileInfo fi = new FileInfo(path);
                if (fi.Exists && (fi.Length > 0 || data.Length == 0))
                    return true;
            }
            catch
            {
            }

            System.Threading.Thread.Sleep(100);
        }

        NIDebug.LogError(string.Format("文件写入失败 {0}", path));
        return false;
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    public static void DeleteFile(string path)
    {
        // 文件不存在
        if (!File.Exists(path))
            return;

        // 尝试删除文件
        for (int i = 0; i < 10; i++)
        {
            try
            {
                File.Delete(path);
                if (!File.Exists(path))
                    return;
            }
            catch
            {
            }

            // 删除失败等待0.1s后重试
            System.Threading.Thread.Sleep(100);
        }

        NIDebug.LogError(string.Format("文件删除失败 {0}", path));
    }

    /// <summary>
    /// 移动文件
    /// </summary>
    public static void MoveFile(string sourceFile, string targetFile)
    {
        try
        {
            FileInfo sourceInfo = new FileInfo(sourceFile);
            FileInfo targetInfo = new FileInfo(targetFile);

            if (!sourceInfo.Exists)
            {
                Debug.LogWarning(string.Format("不存在源文件 {0}！", sourceFile));
                return;
            }

            // 如果源文件存在，则需要删除
            if (targetInfo.Exists)
                targetInfo.Delete();

            // 建立目标目录
            CreateDirectory(Path.GetDirectoryName(targetFile));

            // 移动所有文件
            File.Move(sourceInfo.FullName, targetInfo.FullName);
        }
        catch (Exception e)
        {
            NIDebug.LogException(e);
        }
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="dir">Dir.</param>
    public static void CreateDirectory(string dir)
    {
        if (Directory.Exists(dir))
            return;

        for (int i = 0; i < 10; i++)
        {
            try
            {
                Directory.CreateDirectory(dir);
                if (Directory.Exists(dir))
                    return;
            }
            catch
            {
            }

            System.Threading.Thread.Sleep(100);
        }

        NIDebug.LogError(string.Format("目录创建失败 {0}", dir));
    }

    /// <summary>
    /// 拷贝目录
    /// </summary>
    /// <param name="sourceDir">Source dir.</param>
    /// <param name="targetDir">Target dir.</param>
    public static void CopyDirectory(string sourceDir, string targetDir)
    {
        try
        {
            DirectoryInfo sourceInfo = new DirectoryInfo(sourceDir);
            DirectoryInfo targetInfo = new DirectoryInfo(targetDir);

            if(targetInfo.FullName.StartsWith(sourceInfo.FullName, StringComparison.CurrentCultureIgnoreCase))
            {
                NIDebug.Log("父目录不能拷贝到子目录！");
                return;
            }

            if(!sourceInfo.Exists)
            {
                NIDebug.Log("不存在源目录 {0}！", sourceDir);
                return;
            }

            // 建立目标目录
            if(!targetInfo.Exists)
            {
                targetInfo.Create();
            }

            // 拷贝所有文件
            FileInfo[] files = sourceInfo.GetFiles();
            for(int i = 0; i < files.Length; i++)
            {
                File.Copy(files[i].FullName, targetInfo.FullName + "/" + files[i].Name, true);
            }

            // 递归拷贝子目录
            DirectoryInfo[] dirs = sourceInfo.GetDirectories();
            for(int j = 0; j < dirs.Length; j++)
            {
                CopyDirectory(dirs[j].FullName, targetInfo.FullName + "/" + dirs[j].Name);
            }
        } catch(Exception e)
        {
            NIDebug.Log(e.ToString());
        }
    }

    /// <summary>
    /// 删除目录
    /// </summary>
    /// <param name="dir">Dir.</param>
    public static bool DeleteDirectory(string dir)
    {
        try
        {
            // 删除所有文件
            if (Directory.Exists(dir))
            {
                foreach (var f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                    File.Delete(f);

                // 删除所有目录
                Directory.Delete(dir, true);
            }
            return true;
        }
        catch (Exception e)
        {
            NIDebug.LogException(e);
            return false;
        }
    }

    // 读取文件所有内容以字符串形式返回
    public static string ReadAllText(string file)
    {
        if(File.Exists(file))
            return File.ReadAllText(file);
        else
            return string.Empty;
    }

    // 逐行读取内容
    public static string[] ReadLines(string file)
    {
        if(File.Exists(file))
            return File.ReadAllLines(file);
        else
            return new string[] {};
    }

    // 写入整型
    public static int WriteInt(Stream stream, int i)
    {
        byte[] data = BitConverter.GetBytes(i);
        stream.Write(data, 0, data.Length);
        return data.Length;
    }

    // 读取整型
    public static int ReadInt(Stream stream)
    {
        byte[] buf = new byte[sizeof(int)];
        stream.Read(buf, 0, buf.Length);
        return BitConverter.ToInt32(buf, 0);
    }

    // 写入无符号整型
    public static int WriteUInt(Stream stream, uint i)
    {
        byte[] data = BitConverter.GetBytes(i);
        stream.Write(data, 0, data.Length);
        return data.Length;
    }

    // 读取无符号整型
    public static uint ReadUInt(Stream stream)
    {
        byte[] buf = new byte[sizeof(uint)];
        stream.Read(buf, 0, buf.Length);
        return BitConverter.ToUInt32(buf, 0);
    }

    // 写入无符号短整型
    public static int WriteUShort(Stream stream, ushort i)
    {
        byte[] data = BitConverter.GetBytes(i);
        stream.Write(data, 0, data.Length);
        return data.Length;
    }

    // 读取无符号短整型
    public static ushort ReadUShort(Stream stream)
    {
        byte[] buf = new byte[sizeof(ushort)];
        stream.Read(buf, 0, buf.Length);
        return BitConverter.ToUInt16(buf, 0);
    }

    // 写入字符串
    public static int WriteString(Stream stream, string str)
    {
        byte[] data = Encoding.UTF8.GetBytes(str);
        int len = WriteInt(stream, data.Length);
        stream.Write(data, 0, data.Length);
        len += data.Length;

        return len;
    }

    // 读取字符串
    public static string ReadString(Stream stream)
    {
        int len = ReadInt(stream);
        byte[] buf = new byte[len];
        stream.Read(buf, 0, buf.Length);
        return Encoding.UTF8.GetString(buf);
    }

    // =======================================================================
    // 异步等待写入文件

    // 文件等待器
    public class FileWaiter : IYieldObject
    {
        public FileStream fs = null;
        public bool isFinish = false;
        public FileWaiter(FileStream _fs)
        {
            fs = _fs;
        }
        public bool IsDone()
        {
            return isFinish;
        }
    }

    // 异步写入文件
    public static FileWaiter WriteFileAsync(string path, byte[] data)
    {
        // 保证目录存在
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        // 写入文件
        FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        FileWaiter fw = new FileWaiter(fs);
        fs.BeginWrite(data, 0, data.Length, new AsyncCallback(EndWrite), fw);
        return fw;
    }

    // 结束写入
    static void EndWrite(IAsyncResult asyncResult)
    {
        FileWaiter fw = (FileWaiter)asyncResult.AsyncState;
        fw.fs.EndWrite(asyncResult);
        fw.fs.Close();
        fw.isFinish = true;
    }
}
