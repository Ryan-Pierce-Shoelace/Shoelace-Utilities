#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ShoelaceStudios.Utilities
{
    public static class EntryHelper
    {
        public static void DrawEntryRow(SerializedProperty entry, float x, float y, float fieldWidth,
            float rowHeight, float padding, int index, SerializedProperty entriesProp)
        {
            var valueProp = entry.FindPropertyRelative("Value");
            var weightProp = entry.FindPropertyRelative("Weight");
            var colorProp = entry.FindPropertyRelative("Color");

            Rect valueRect = new Rect(x, y, fieldWidth, rowHeight);
            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

            Rect weightRect = new Rect(x + fieldWidth + padding, y, fieldWidth, rowHeight);
            weightProp.floatValue = Mathf.Max(0f, EditorGUI.FloatField(weightRect, weightProp.floatValue));

            Rect colorRect = new Rect(x + 2 * (fieldWidth + padding), y, fieldWidth, rowHeight);
            if (colorProp != null)
                colorProp.colorValue = EditorGUI.ColorField(colorRect, GUIContent.none, colorProp.colorValue);

            Rect removeRect = new Rect(x + 3 * (fieldWidth + padding), y, 60f, rowHeight);
            if (GUI.Button(removeRect, "X"))
                entriesProp.DeleteArrayElementAtIndex(index);
        }

        public static void AddEntry(SerializedProperty entriesProp)
        {
            int idx = entriesProp.arraySize;
            entriesProp.InsertArrayElementAtIndex(idx);
            var newEntry = entriesProp.GetArrayElementAtIndex(idx);
            newEntry.FindPropertyRelative("Weight").floatValue = 1f;
            newEntry.FindPropertyRelative("Color").colorValue = BarHelper.DefaultColor(idx);
        }

        public static void NormalizeEntries(SerializedProperty entriesProp)
        {
            float total = 0f;
            for (int i = 0; i < entriesProp.arraySize; i++)
                total += Mathf.Max(0f,
                    entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Weight").floatValue);

            if (total <= 0f) return;

            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                var w = entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Weight");
                w.floatValue /= total;
            }
        }

        public static string GetPropertyValueString(SerializedProperty prop)
        {
            if (prop == null) return "Entry";
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Enum:
                    return prop.enumNames[prop.enumValueIndex];
                case SerializedPropertyType.Integer:
                    return prop.intValue.ToString();
                case SerializedPropertyType.Float:
                    return prop.floatValue.ToString("0.###");
                case SerializedPropertyType.String:
                    return prop.stringValue;
                default:
                    return prop.displayName;
            }
        }
    }
}
#endif