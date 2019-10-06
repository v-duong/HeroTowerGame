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

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void Save()
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        Debug.Log(Application.persistentDataPath);
        using (FileStream file = File.Create(Application.persistentDataPath + "/playerData.bin"))
        {
            RijndaelManaged rm = new RijndaelManaged();
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(DerivePass, DeriveSalt);
            rm.Key = pdb.GetBytes(32);
            rm.IV = pdb.GetBytes(16);
            using (CryptoStream cs = new CryptoStream(file, rm.CreateEncryptor(), CryptoStreamMode.Write))
            {
                GZipStream gZip = new GZipStream(cs, CompressionMode.Compress);
                SaveData s = new SaveData();

                s.SaveAll();

                binaryFormatter.Serialize(gZip, s);
                gZip.Close();
            }
        }
    }

    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/playerData.bin"))
        {
            using (FileStream file = File.Open(Application.persistentDataPath + "/playerData.bin", FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                RijndaelManaged rm = new RijndaelManaged();
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(DerivePass, DeriveSalt);
                rm.Key = pdb.GetBytes(32);
                rm.IV = pdb.GetBytes(16);
                using (CryptoStream cs = new CryptoStream(file, rm.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    GZipStream gZip = new GZipStream(cs, CompressionMode.Decompress);

                    SaveData s = (SaveData)binaryFormatter.Deserialize(gZip);

                    s.LoadAll();

                    gZip.Close();
                }
            }
        }
    }
}