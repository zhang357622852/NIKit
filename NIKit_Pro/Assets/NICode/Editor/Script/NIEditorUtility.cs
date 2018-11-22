/// <summary>
/// NIEditorUtility.cs
/// Created by WinMi 2017/12/9
/// Editor静态工具类
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography;

public static class NIEditorUtility
{
    private static string authorIconPath = "Assets/NICode/Editor/GUI/authorIcon.png";

    private static Texture2D authorTexture2d = (Texture2D)AssetDatabase.LoadMainAssetAtPath(authorIconPath);

    public static void DrawAuthorSummary()
    {
        GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(65));
        GUILayout.Box(new GUIContent(authorTexture2d, "头像o(*￣︶￣*)o"));
        GUILayout.Space(10);
        GUILayout.BeginVertical();
        GUIStyle style = new GUIStyle();
        style.fontSize = 15;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        GUILayout.Space(12);
        GUILayout.TextArea("Author: WinMi", style);
        GUILayout.Space(12);
        GUILayout.TextArea("Descript:", style);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    public static Texture2D GetAuthorImage()
    {
        return authorTexture2d;
    }

    public static void DrawTitle(string text, int fontSize = 20)
    {
        GUILayout.BeginVertical();
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);

                GUIStyle style = new GUIStyle();
                style.fontSize = fontSize;
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.white;
                GUILayout.Label(text, style);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MaxHeight(10f));
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    public static string BrowseFolder()
    {
        return null;
    }

    /// <summary>
    /// Gets the Md5.
    /// </summary>
    /// <returns>The M d5.</returns>
    /// <param name="sDataIn">S data in.</param>
    public static string GetStrMD5(string sDataIn)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] bytValue, bytHash;
        bytValue = System.Text.Encoding.UTF8.GetBytes(sDataIn);
        bytHash = md5.ComputeHash(bytValue);
        md5.Clear();

        string sTemp = "";

        foreach (byte b in bytHash)
        {
            sTemp += Convert.ToString(b, 16);
        }

        return sTemp.ToLower();
    }

    /// <summary>
    /// 得到文件MD5
    /// </summary>
    public static string GetFileMD5(string filePath)
    {
        try
        {
            FileStream fs = new FileStream(filePath, FileMode.Open);
            int len = (int)fs.Length;
            byte[] data = new byte[len];
            fs.Read(data, 0, len);
            fs.Close();
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(data);
            string fileMD5 = "";

            foreach (byte b in result)
            {
                fileMD5 += Convert.ToString(b, 16);
            }

            return fileMD5.ToLower();
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e.Message);
            return "";
        }
    }

    /// <summary>
    /// 压缩文件和文件夹
    /// </summary>
    /// <param name="_fileOrDirectoryArray">文件夹路径和文件名,可以多个</param>
    /// <param name="_outputPathName">压缩后的输出路径文件名</param>
    /// <param name="_password">压缩密码</param>
    /// <returns></returns>
    public static bool Zip(string[] _fileOrDirectoryArray, string _outputPathName, string _password = null)
    {
        // 传入参数不合格
        if ((null == _fileOrDirectoryArray) || string.IsNullOrEmpty(_outputPathName))
            return false;

        ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(_outputPathName));
        zipOutputStream.SetLevel(6);    // 压缩质量和压缩速度的平衡点

        if (!string.IsNullOrEmpty(_password))
            zipOutputStream.Password = _password;

        for (int index = 0; index < _fileOrDirectoryArray.Length; ++index)
        {
            bool result = false;
            string fileOrDirectory = _fileOrDirectoryArray[index];

            if (Directory.Exists(fileOrDirectory))
                result = ZipDirectory(fileOrDirectory, string.Empty, zipOutputStream);
            else if (File.Exists(fileOrDirectory))
                result = ZipFile(fileOrDirectory, string.Empty, zipOutputStream);

            if (!result)
                return false;
        }

        // 关闭流文件
        zipOutputStream.Finish();
        zipOutputStream.Close();

        return true;
    }

    /// <summary>
    /// 压缩文件
    /// </summary>
    /// <param name="_filePathName">文件路径名</param>
    /// <param name="_parentRelPath">要压缩的文件的父相对文件夹</param>
    /// <param name="_zipOutputStream">压缩输出流</param>
    /// <param name="_zipCallback">ZipCallback对象，负责回调</param>
    /// <returns></returns>
    private static bool ZipFile(string _filePathName, string _parentRelPath, ZipOutputStream _zipOutputStream)
    {
        ZipEntry entry = null;
        FileStream fileStream = null;
        try
        {
            string entryName = _parentRelPath + '/' + Path.GetFileName(_filePathName);
            entry = new ZipEntry(entryName);
            entry.DateTime = System.DateTime.Now;

            fileStream = File.OpenRead(_filePathName);
            byte[] buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, buffer.Length);
            fileStream.Close();

            entry.Size = buffer.Length;

            _zipOutputStream.PutNextEntry(entry);
            _zipOutputStream.Write(buffer, 0, buffer.Length);
        }
        catch (System.Exception)
        {
            return false;
        }
        finally
        {
            if (null != fileStream)
            {
                fileStream.Close();
                fileStream.Dispose();
            }
        }

        return true;
    }

    /// <summary>
    /// 压缩文件夹
    /// </summary>
    /// <param name="_path">要压缩的文件夹</param>
    /// <param name="_parentRelPath">要压缩的文件夹的父相对文件夹</param>
    /// <param name="_zipOutputStream">压缩输出流</param>
    /// <returns></returns>
    private static bool ZipDirectory(string _path, string _parentRelPath, ZipOutputStream _zipOutputStream)
    {
        ZipEntry entry = null;
        try
        {
            string entryName = Path.Combine(_parentRelPath, Path.GetFileName(_path) + '/');
            entry = new ZipEntry(entryName);
            entry.DateTime = System.DateTime.Now;
            entry.Size = 0;

            _zipOutputStream.PutNextEntry(entry);
            _zipOutputStream.Flush();

            string[] files = Directory.GetFiles(_path);
            for (int index = 0; index < files.Length; ++index)
                ZipFile(files[index], Path.Combine(_parentRelPath, Path.GetFileName(_path)), _zipOutputStream);
        }
        catch (System.Exception)
        {
            return false;
        }

        string[] directories = Directory.GetDirectories(_path);
        for (int index = 0; index < directories.Length; ++index)
        {
            if (!ZipDirectory(directories[index], Path.Combine(_parentRelPath, Path.GetFileName(_path)), _zipOutputStream))
                return false;
        }

        return true;
    }
}
