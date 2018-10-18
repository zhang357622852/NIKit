/// <summary>
/// 加密相关接口
/// </summary>
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Encrypt
{
    // 静态初始化
    static Encrypt()
    {
        InitCRC32();
    }

    // ------------------------------------------------------------------

    private static byte[] DesKey = System.Text.Encoding.UTF8.GetBytes("77777777");
    private static byte[] DesIV = System.Text.Encoding.UTF8.GetBytes("19770531");

    public static byte[] Des(string data)
    {
        try
        {
            // Create a MemoryStream.
            MemoryStream mStream = new MemoryStream();

            // Create a new DES object.
            DES DESalg = DES.Create();
            DESalg.Mode = CipherMode.ECB;
            DESalg.Padding = PaddingMode.None;

            // Create a CryptoStream using the MemoryStream
            // and the passed key and initialization vector (IV).
            CryptoStream cStream = new CryptoStream(mStream,
                DESalg.CreateEncryptor(DesKey, DesIV),
                CryptoStreamMode.Write);

            // Convert the passed string to a byte array.
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
            int alignSize = (dataBytes.Length + 7) / 8 * 8;
            byte[] toEncrypt = new byte[alignSize];
            System.Array.Copy(dataBytes, toEncrypt, dataBytes.Length);

            // Write the byte array to the crypto stream and flush it.
            cStream.Write(toEncrypt, 0, toEncrypt.Length);
            cStream.FlushFinalBlock();

            // Get an array of bytes from the
            // MemoryStream that holds the
            // encrypted data.
            byte[] ret = mStream.ToArray();

            // Close the streams.
            cStream.Close();
            mStream.Close();

            // Return the encrypted buffer.
            return ret;
        } catch (CryptographicException e)
        {
            Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
            return null;
        }
    }

    public static string Des(byte[] data)
    {
        try
        {
            // Create a new MemoryStream using the passed
            // array of encrypted data.
            MemoryStream msDecrypt = new MemoryStream(data);

            // Create a new DES object.
            DES DESalg = DES.Create();
            DESalg.Mode = CipherMode.ECB;
            DESalg.Padding = PaddingMode.None;

            // Create a CryptoStream using the MemoryStream
            // and the passed key and initialization vector (IV).
            CryptoStream csDecrypt = new CryptoStream(msDecrypt,
                DESalg.CreateDecryptor(DesKey, DesIV),
                CryptoStreamMode.Read);

            // Create buffer to hold the decrypted data.
            byte[] fromEncrypt = new byte[data.Length];

            // Read the decrypted data out of the crypto stream
            // and place it into the temporary buffer.
            csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

            //Convert the buffer into a string and return it.
            return System.Text.Encoding.UTF8.GetString(fromEncrypt);
            // return new ASCIIEncoding().GetString(fromEncrypt);
        } catch (CryptographicException e)
        {
            Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
            return null;
        }
    }

    // ------------------------------------------------------------------

    public static string Md5(string input)
    {
        // This is one implementation of the abstract class MD5.
        MD5 md5 = MD5.Create();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(input);
        byte[] hash = md5.ComputeHash(data);

        // 返回加密后的字符串
        return BitConverter.ToString(hash).Replace("-", "");
    }

    // ------------------------------------------------------------------

    static uint[] mCRC32Table;
    static void InitCRC32()
    {
        // crc表计算
        const uint ulPolynomial = 0xEDB88320;
        uint dwCrc;
        mCRC32Table = new uint[256];
        int i, j;
        for (i = 0; i < 256; i++)
        {
            dwCrc = (uint)i;
            for (j = 8; j > 0; j--)
            {
                if ((dwCrc & 1) == 1)
                    dwCrc = (dwCrc >> 1) ^ ulPolynomial;
                else
                    dwCrc >>= 1;
            }
            mCRC32Table [i] = dwCrc;
        }
    }

    // 字节数组效验
    public static uint CRC32(ref byte[] buffer)
    {
        uint ulCRC = 0xffffffff;
        uint len;
        len = (uint)buffer.Length;
        for (uint buffptr = 0; buffptr < len; buffptr++)
        {
            uint tabPtr = ulCRC & 0xFF;
            tabPtr = tabPtr ^ buffer [buffptr];
            ulCRC = ulCRC >> 8;
            ulCRC = ulCRC ^ mCRC32Table [tabPtr];
        }
        return ulCRC ^ 0xffffffff;
    }
}
