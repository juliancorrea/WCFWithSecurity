using System;
using System.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Reflection;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace Client
{
  public static class Criptografia
  {
    public const string KEY = "RarbOcodNenfAgahLiljDkolmSnsoCpcqHrhsMwmxIyizTtzhk";

    public static string Encrypt(string text)
    {
      GeoTripleDes td = new GeoTripleDes();
      return td.Encrypt(text);
    }

    public static string Decrypt(string text)
    {
      GeoTripleDes td = new GeoTripleDes();
      return td.Decrypt(text);
    }
  }

  internal class GeoTripleDes
  {
    internal const string KEY1 = "2/3+8972j5kl2jifdajf8v0732q443564fadsf56a4d31vz654badfdasfdas";
    internal const string KEY2 = "4234c556243454265t/5gsd64af+54vc90d29didoslax0495ofls1";
    private static string _key;
    private static string _iv;
    private static string _key3;
    private static byte[] _salt;

    public GeoTripleDes()
    {
      if (string.IsNullOrEmpty(_key3))
      {
        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false);
        if (attributes.Length > 0)
        {
          GuidAttribute guid = (GuidAttribute)attributes[0];
          _key3 = guid.Value;
          _key3 = _key3.Replace("-", "");
        }
      }
      if (string.IsNullOrEmpty(_key))
      {
        _key = KEY1 + _key3;
      }
      if (string.IsNullOrEmpty(_iv))
      {
        _iv = KEY2 + _key3;
      }
      if (_salt == null)
      {
        _salt = new byte[16] { 0x40, 0x6e, 0x64, 0x72, 0x33, 0x76, 0x33, 0x31,
                               0x31, 0x30, 0x35, 0x30, 0x2e, 0x6e, 0x65, 0x74};
      }
    }

    private TripleDESCryptoServiceProvider GetAlg()
    {
      TripleDESCryptoServiceProvider alg = new TripleDESCryptoServiceProvider();
      PasswordDeriveBytes pdb = new PasswordDeriveBytes(_key, _salt, "SHA512", 100);
      alg.Key = pdb.GetBytes(alg.KeySize / 8);
      alg.IV = pdb.GetBytes(alg.BlockSize / 8);
      alg.Padding = PaddingMode.PKCS7;
      return alg;
    }

    private byte[] Execute(ICryptoTransform ct, byte[] text)
    {
      MemoryStream ms = new MemoryStream();
      CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
      cs.Write(text, 0, text.Length);
      cs.Close();
      return ms.ToArray();
    }

    public string Encrypt(string text)
    {
      TripleDESCryptoServiceProvider alg = GetAlg();
      ICryptoTransform ct = alg.CreateEncryptor(alg.Key, alg.IV);
      byte[] result = Execute(ct, Encoding.UTF8.GetBytes(text));
      return Convert.ToBase64String(result);
    }

    public string Decrypt(string text)
    {
      TripleDESCryptoServiceProvider alg = GetAlg();
      ICryptoTransform ct = alg.CreateDecryptor(alg.Key, alg.IV);
      byte[] result = Execute(ct, Convert.FromBase64String(text));
      return Encoding.UTF8.GetString(result);
    }
  }

  /// <summary>
  /// Classe de apoio para criptografia AES (Advanced Encryption Standard (AES)) 
  /// </summary>
  public static class AES
  {
    /// <summary>
    /// Niveis de criptografia AES
    /// Criptografia de 128, 192 e 256 Bits.
    /// </summary>
    public enum AESCryptographyLevel : int
    {
      /// <summary>
      /// Criptografia AES 128 Bits
      /// </summary>
      AES_128 = 128,

      /// <summary>
      /// Criptografia AES 192 Bits
      /// </summary>
      AES_192 = 192,

      /// <summary>
      /// Criptografia AES 256 Bits
      /// </summary>
      AES_256 = 256
    }

    #region Private
    // Encrypt a byte array into a byte array using a key and an IV 
    private static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
    {

      // Create a MemoryStream that is going to accept the encrypted bytes 
      MemoryStream ms = new MemoryStream();

      Rijndael alg = Rijndael.Create();
      alg.Key = Key;

      alg.IV = IV;
      CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);

      cs.Write(clearData, 0, clearData.Length);
      cs.Close();
      byte[] encryptedData = ms.ToArray();
      return encryptedData;
    }

    // Decrypt a byte array into a byte array using a key and an IV 
    private static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
    {

      MemoryStream ms = new MemoryStream();
      Rijndael alg = Rijndael.Create();
      alg.Key = Key;
      alg.IV = IV;
      CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
      cs.Write(cipherData, 0, cipherData.Length);
      cs.Close();
      byte[] decryptedData = ms.ToArray();
      return decryptedData;
    }


    #endregion

    #region Public
    /// <summary>
    /// Criptografa string usando Rijndael (128,192,256 Bits).
    /// </summary>
    /// <param name="data">dados a serem criptografados</param>
    /// <param name="password">senha de criptografia</param>
    /// <param name="bits">Nível de criptografia (128,192,256 bits)</param>
    /// <returns>dados criptografados</returns>
    public static byte[] Encrypt(byte[] data, string password, AESCryptographyLevel bits)
    {
      PasswordDeriveBytes pdb = new PasswordDeriveBytes(password,
          new byte[] { 0x00, 0x01, 0x02, 0x1C, 0x1D, 0x1E, 0x03, 0x04, 0x05, 0x0F, 0x20, 0x21, 0xAD, 0xAF, 0xA4 });

      if (bits == AESCryptographyLevel.AES_128)
      {
        byte[] encryptedData = Encrypt(data, pdb.GetBytes(16), pdb.GetBytes(16));
        return encryptedData;
      }
      else if (bits == AESCryptographyLevel.AES_192)
      {
        byte[] encryptedData = Encrypt(data, pdb.GetBytes(24), pdb.GetBytes(16));
        return encryptedData;
      }
      else if (bits == AESCryptographyLevel.AES_256)
      {
        byte[] encryptedData = Encrypt(data, pdb.GetBytes(32), pdb.GetBytes(16));
        return encryptedData;
      }
      else
      {
        throw new Exception("Nível inválido!");
      }
    }

    /// <summary>
    /// Criptografa string usando Rijndael (128,192,256 Bits).
    /// </summary>
    /// <param name="data">string a ser criptografada</param>
    /// <param name="password">senha de criptografia</param>
    /// <param name="bits">Nível de criptografia (128,192,256 bits)</param>
    /// <returns>string criptografada</returns>
    public static string Encrypt(string data, string password, AESCryptographyLevel bits)
    {
      byte[] clearBytes = Encoding.Unicode.GetBytes(data);
      byte[] result = Encrypt(clearBytes, password, bits);
      return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Descriptografa string usando Rijndael (128,192,256 Bits).
    /// </summary>
    /// <param name="data">dados a serem descriptografados</param>
    /// <param name="password">senha de descriptografia</param>
    /// <param name="bits">Nível de descriptografia (128,192,256 bits)</param>
    /// <returns>dados descriptografados</returns>
    public static byte[] Decrypt(byte[] data, string password, AESCryptographyLevel bits)
    {
      PasswordDeriveBytes pdb = new PasswordDeriveBytes(password,

          new byte[] { 0x00, 0x01, 0x02, 0x1C, 0x1D, 0x1E, 0x03, 0x04, 0x05, 0x0F, 0x20, 0x21, 0xAD, 0xAF, 0xA4 });

      if (bits == AESCryptographyLevel.AES_128)
      {
        byte[] decryptedData = Decrypt(data, pdb.GetBytes(16), pdb.GetBytes(16));
        return decryptedData;
      }
      else if (bits == AESCryptographyLevel.AES_192)
      {
        byte[] decryptedData = Decrypt(data, pdb.GetBytes(24), pdb.GetBytes(16));
        return decryptedData;
      }
      else if (bits == AESCryptographyLevel.AES_256)
      {
        byte[] decryptedData = Decrypt(data, pdb.GetBytes(32), pdb.GetBytes(16));
        return decryptedData;
      }
      else
      {
        throw new Exception("Nível inválido!");
      }
    }

    /// <summary>
    /// Descriptografa string usando Rijndael (128,192,256 Bits).
    /// </summary>
    /// <param name="data">string a ser descriptografada</param>
    /// <param name="password">senha de descriptografia</param>
    /// <param name="bits">Nível de descriptografia (128,192,256 bits)</param>
    /// <returns>string descriptografada</returns>
    public static string Decrypt(string data, string password, AESCryptographyLevel bits)
    {
      byte[] cipherBytes = Convert.FromBase64String(data);
      byte[] result = Decrypt(cipherBytes, password, bits);
      return Encoding.Unicode.GetString(result);
    }

    public static byte[] EncryptDataSet(DataSet ds, XmlWriteMode writeMode, string password, AESCryptographyLevel bits)
    {
      using (MemoryStream ms = new MemoryStream())
      {
        ds.WriteXml(ms, writeMode);
        return Encrypt(ms.ToArray(), password, bits);
      }
    }

    public static DataSet DecryptDataSet(byte[] data, XmlReadMode readMode, string passWord, AESCryptographyLevel bits)
    {
      byte[] decryptedData = Decrypt(data, passWord, bits);
      using (MemoryStream ms = new MemoryStream(decryptedData))
      {
        DataSet ds = new DataSet();
        ds.ReadXml(ms, readMode);
        return ds;
      }
    }

    #endregion
  }
}
