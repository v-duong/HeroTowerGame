using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
[CustomEditor(typeof(AbilityBase))]
[CanEditMultipleObjects]
public class AbilityEffectEditor : Editor {
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty tProp = serializedObject.GetIterator();
        if (tProp.NextVisible(true))
        {
            do
            {
                EditorGUI.BeginDisabledGroup(tProp.name == "m_Script");
                if (tProp.name == "baseAbilityEffects")
                    DrawEffectProp(tProp.Copy());
                else
                    EditorGUILayout.PropertyField(tProp, true);
                EditorGUI.EndDisabledGroup();
            }
            while (tProp.NextVisible(false));
        }

        serializedObject.ApplyModifiedProperties();
    }


    public void DrawEffectProp(SerializedProperty effectProp)
    {
        if (EditorGUILayout.PropertyField(effectProp, false))
        {

            EditorGUI.indentLevel++;
            effectProp.NextVisible(true);
            EditorGUILayout.PropertyField(effectProp, false); //size
            while (true)
            {
                if (!effectProp.NextVisible(true))
                    break;

                if (effectProp.hasChildren)
                {
                    effectProp = HandleChildren(effectProp, effectProp.GetEndProperty());
                }
                else
                    EditorGUILayout.PropertyField(effectProp, false);
            }

        }
    }

    public SerializedProperty HandleChildren(SerializedProperty prop, SerializedProperty endprop)
    {
        SerializedProperty prev;
        SerializedProperty entry = null;
        bool flag = false;
        if (prop.type == "AbilityEffectEntry")
        {
            entry = prop.FindPropertyRelative("effect");
            string s = "Effect";
            if (entry != null && entry.objectReferenceValue != null)
            {
                s = GetObjectField(entry, "name").ToString();
                SerializedProperty size = prop.FindPropertyRelative("effectVariables");
                size.Next(true);
                size.arraySize = (int)GetObjectField(entry, "numOfVars");
                flag = true;
            }
            EditorGUILayout.PropertyField(prop, new GUIContent(s), false);
        }
        else
            EditorGUILayout.PropertyField(prop, false);
        
        EditorGUI.indentLevel++;
        do
        {

            prev = prop.Copy();
            if (!prop.NextVisible(true) || SerializedProperty.EqualContents(prop, endprop))
                break;

            if (prop.hasChildren)
            {
                if (flag)
                    prop = HandleChildren2(prop, prop.GetEndProperty(), entry);
                else
                    prop = HandleChildren(prop, prop.GetEndProperty());
            }
            else
                EditorGUILayout.PropertyField(prop, false);
        } while (!SerializedProperty.EqualContents(prop, endprop));
        EditorGUI.indentLevel--;
        return prev;
    }

    public SerializedProperty HandleChildren2(SerializedProperty prop, SerializedProperty endprop, SerializedProperty p)
    {
        SerializedProperty prev;
        EditorGUILayout.PropertyField(prop, false);

        EditorGUI.indentLevel++;
        do
        {
            prev = prop.Copy();
            if (!prop.NextVisible(true) || SerializedProperty.EqualContents(prop, endprop))
                break;

            if (prop.hasChildren)
                prop = HandleChildren(prop, prop.GetEndProperty());
            else
                EditorGUILayout.PropertyField(prop, false);
        } while (!SerializedProperty.EqualContents(prop, endprop));
        EditorGUI.indentLevel--;

        return prev;
    }

    public object GetObjectField(SerializedProperty p, string s)
    {
        var target = p.objectReferenceValue;
        var objType = target.GetType();
        var field = objType.GetField(s);
        var v = field.GetValue(target);
        return v;
    }
}
*/

[CustomPropertyDrawer(typeof(AbilityEffectEntry))]
public class AbilityEffectEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        var s = property.FindPropertyRelative("effect");
        var effectvars = property.FindPropertyRelative("effectVariables");
        Object e = null;
        if (s != null)
        {
            e = s.objectReferenceValue;
            if (e != null)
            {
                effectvars.arraySize = (int)GetObjectProperty(e, "EffectVarCount");
            }
        }

        EditorGUI.PropertyField(position, property, new GUIContent(e ? e.name : "No Effect"), true);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float rows = 0;
        if (property.isExpanded)
            rows = 4;
        return base.GetPropertyHeight(property, label) * rows + 15;
    }

    public object GetObjectField(Object p, string s)
    {
        var objType = p.GetType();
        var field = objType.GetField(s);
        var v = field.GetValue(p);
        return v;
    }

    public object GetObjectProperty(Object p, string s)
    {
        var objType = p.GetType();
        var field = objType.GetProperty(s);
        var v = field.GetValue(p);
        return v;
    }
}