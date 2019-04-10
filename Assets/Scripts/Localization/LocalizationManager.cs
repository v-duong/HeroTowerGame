using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    private static readonly string defaultLang = "en-US";

    private static Dictionary<string, string> localizationData = new Dictionary<string, string>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadLocalization();
    }


    private void LoadLocalization(string locale = "en-US")
    {
        if (locale == null)
            locale = defaultLang;
        string path = "json/localization/" + locale;
        localizationData = JsonConvert.DeserializeObject<Dictionary<string,string>>(Resources.Load<TextAsset>(path).text);

    }

    private T DeserializeFromPath<T>(string path)
    {
        return JsonConvert.DeserializeObject<T>(Resources.Load<TextAsset>(path).text);
    }

    public string GetLocalizationText(string stringId)
    {
        string value = "";
        if (localizationData.TryGetValue(stringId, out value))
        {
            if (value == "")
                return stringId;
            return value;
        }
        else
        {
            return stringId;
        }
    }
}
