using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static readonly byte[] DeriveSalt = new byte[] { 0xff, 0xaf, 0x04, 0x56, 0x11, 0xcd, 0xd6, 0x12, 0x8e, 0xbb, 0x29, 0xa0, 0x00, 0xa1, 0xff, 0x5c };
    private static readonly string DerivePass = "2IlDSVglmu";
    public static SaveManager Instance { get; private set; }
    private static SaveData _currentSave;

    public static SaveData CurrentSave
    {
        get
        {
            if (_currentSave == null)
            {
                _currentSave = new SaveData();
            }
            return _currentSave;
        }
        private set
        {
            _currentSave = value;
        }
    }


    public static void SaveAll()
    {
        CurrentSave.SaveAll();
        WriteSaveFile();
    }

    public static void Save()
    {
        WriteSaveFile();
    }

    private static void WriteSaveFile()
    {
        Debug.Log(Application.persistentDataPath);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (FileStream file = File.Create(Application.persistentDataPath + "/playerData.bin"))
        {
            using (RijndaelManaged rm = new RijndaelManaged())
            {
                using (Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(DerivePass, DeriveSalt))
                {
                    rm.Key = pdb.GetBytes(32);
                    rm.IV = pdb.GetBytes(16);
                }
                using (CryptoStream cs = new CryptoStream(file, rm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    GZipStream gZip = new GZipStream(cs, CompressionMode.Compress);
                    binaryFormatter.Serialize(gZip, CurrentSave);
                    gZip.Close();
                }
            }
        }
    }

    public static bool Load()
    {
        if (File.Exists(Application.persistentDataPath + "/playerData.bin"))
        {
            using (FileStream file = File.Open(Application.persistentDataPath + "/playerData.bin", FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                using (RijndaelManaged rm = new RijndaelManaged())
                {
                    using (Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(DerivePass, DeriveSalt))
                    {
                        rm.Key = pdb.GetBytes(32);
                        rm.IV = pdb.GetBytes(16);
                    }

                    using (CryptoStream cs = new CryptoStream(file, rm.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        GZipStream gZip = new GZipStream(cs, CompressionMode.Decompress);

                        SaveData s = (SaveData)binaryFormatter.Deserialize(gZip);

                        s.LoadAll();

                        CurrentSave = s;

                        gZip.Close();

                        return true;
                    }
                }
            }
        }
        else
        {
            return false;
        }
    }
}