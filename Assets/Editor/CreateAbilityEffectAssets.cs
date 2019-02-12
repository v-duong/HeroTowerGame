using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

public class CreateAbilityEffectAssets : MonoBehaviour
{
    [MenuItem("Assets/Create AbilityEffect Assets")]
    static void CreateAssets()
    {
        var list = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                        from assemblyType in domainAssembly.GetTypes()
                        where typeof(AbilityEffect).IsAssignableFrom(assemblyType)
                        select assemblyType).ToArray();

        foreach (Type x in list)
        {
            if (!x.IsAbstract)
            {
                var asset = ScriptableObject.CreateInstance(x);
                AssetDatabase.CreateAsset(asset, "Assets/Resources/Abilities/AbilityEffects/" + x.FullName + ".asset");
                AssetDatabase.SaveAssets();
            }
        }
    }
}
