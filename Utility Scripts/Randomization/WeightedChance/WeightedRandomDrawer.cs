
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ShoelaceStudios.Utilities
{
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(WeightedRandom<>), true)]
    public class WeightedRandomDrawer : PropertyDrawer
    {
        private const float BarHeight = 20f;
        private const float RowHeight = 22f;
        private const float Padding = 2f;
        private const float MinFieldWidth = 60f;
        private const float MaxFieldWidth = 80f;
        private const float ButtonWidth = 50f;

        // Track foldout state per property path
        private readonly Dictionary<string, bool> foldouts = new();

        #region --- Property Height ---

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!TryGetEntries(property, out var entriesProp))
                return EditorGUIUtility.singleLineHeight * 2 + Padding * 2;

            bool expanded = GetFoldout(property);
            if (!expanded || entriesProp.arraySize == 0)
                return EditorGUIUtility.singleLineHeight * 2 + Padding * 2 + BarHeight / 3f;

            int estimatedLines = Mathf.CeilToInt(entriesProp.arraySize * 1.5f);
            return EditorGUIUtility.singleLineHeight + Padding * 2 + BarHeight + RowHeight * estimatedLines +
                   EditorGUIUtility.singleLineHeight + Padding * 2;
        }

        #endregion

        #region --- OnGUI ---

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (!TryGetEntries(property, out var entriesProp))
            {
                EditorGUI.LabelField(position, label.text + " (No entries)");
                EditorGUI.EndProperty();
                return;
            }

            bool expanded = DrawFoldout(property, position, label, out float yStart);

            if (!expanded)
            {
                DrawCollapsedBar(entriesProp, new Rect(position.x, yStart, position.width, BarHeight / 3f));
                EditorGUI.EndProperty();
                return;
            }

            float y = yStart;

            // Full cumulative bar (draggable)
            y = DrawFullBar(entriesProp, new Rect(position.x, y, position.width, BarHeight)) + Padding;

            // Entry fields with Add/Remove buttons
            y = DrawEntryFields(entriesProp, new Rect(position.x, y, position.width, position.height - y)) + Padding;

            // Normalize button
            DrawNormalizeButton(entriesProp, new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight));

            EditorGUI.EndProperty();
        }

        #endregion

        #region --- Foldout Helpers ---

        private bool GetFoldout(SerializedProperty property)
        {
            string key = property.propertyPath;
            return foldouts.ContainsKey(key) && foldouts[key];
        }

        private bool DrawFoldout(SerializedProperty property, Rect position, GUIContent label, out float y)
        {
            string key = property.propertyPath;
            if (!foldouts.ContainsKey(key)) foldouts[key] = true;
            bool expanded = foldouts[key];

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            expanded = EditorGUI.Foldout(foldoutRect, expanded, label);
            foldouts[key] = expanded;

            y = position.y + EditorGUIUtility.singleLineHeight + Padding;
            return expanded;
        }

        #endregion

        #region --- Draw Bars ---

        private void DrawCollapsedBar(SerializedProperty entriesProp, Rect rect)
        {
            DrawBarSegments(entriesProp, rect, showLabels: true);
        }

        private float DrawFullBar(SerializedProperty entriesProp, Rect rect)
        {
            DrawDraggableBar(entriesProp, rect);
            return rect.yMax;
        }

        private void DrawDraggableBar(SerializedProperty entriesProp, Rect rect)
        {
            float total = GetTotalWeight(entriesProp);
            if (total <= 0f) total = entriesProp.arraySize;

            float x = rect.x;
            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                var entry = entriesProp.GetArrayElementAtIndex(i);
                var weightProp = entry.FindPropertyRelative("Weight");
                var colorProp = entry.FindPropertyRelative("Color");

                float width = (weightProp.floatValue / total) * rect.width;
                Rect segmentRect = new Rect(x, rect.y, width, rect.height);

                EditorGUI.DrawRect(segmentRect, colorProp?.colorValue ?? DefaultColor(i));

                // Drag handle
                EditorGUIUtility.AddCursorRect(segmentRect, MouseCursor.ResizeHorizontal);
                if (Event.current.type == EventType.MouseDrag && segmentRect.Contains(Event.current.mousePosition))
                {
                    float delta = Event.current.delta.x / rect.width * total;
                    weightProp.floatValue = Mathf.Max(0f, weightProp.floatValue + delta);
                    Event.current.Use();
                }

                x += width;
            }

            // Draw labels underneath
            DrawBarSegments(entriesProp, rect, showLabels: false);
        }

        private void DrawBarSegments(SerializedProperty entriesProp, Rect barRect, bool showLabels)
        {
            float total = GetTotalWeight(entriesProp);
            if (total <= 0f) total = entriesProp.arraySize;

            float x = barRect.x;
            float minLabelWidth = 15f;

            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                var entry = entriesProp.GetArrayElementAtIndex(i);
                var weightProp = entry.FindPropertyRelative("Weight");
                var colorProp = entry.FindPropertyRelative("Color");
                var valueProp = entry.FindPropertyRelative("Value");

                float width = (weightProp.floatValue / total) * barRect.width;
                Rect segmentRect = new Rect(x, barRect.y, width, barRect.height);
                Color col = colorProp?.colorValue ?? DefaultColor(i);
                EditorGUI.DrawRect(segmentRect, col);

                if (showLabels && width > minLabelWidth)
                {
                    string name = GetPropertyValueString(valueProp);
                    Vector2 labelSize = GUI.skin.label.CalcSize(new GUIContent(name));
                    Rect labelRect = new Rect(x + width / 2f - labelSize.x / 2f, barRect.y + barRect.height,
                        labelSize.x, labelSize.y);
                    GUI.Label(labelRect, name);
                }

                x += width;
            }
        }

        #endregion

        #region --- Entry Fields ---

        private float DrawEntryFields(SerializedProperty entriesProp, Rect area)
        {
            float x = area.x;
            float y = area.y;
            float maxWidth = area.width;
            float spacing = 4f;

            int fieldCount = 4; // Value, Weight, Color, Remove button
            float fieldWidth = Mathf.Clamp((maxWidth - spacing * (fieldCount + 1)) / fieldCount, MinFieldWidth, MaxFieldWidth);

            float rowX = x;

            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                var entry = entriesProp.GetArrayElementAtIndex(i);
                var valueProp = entry.FindPropertyRelative("Value");
                var weightProp = entry.FindPropertyRelative("Weight");
                var colorProp = entry.FindPropertyRelative("Color");

                // Wrap if necessary
                if (rowX + fieldWidth * fieldCount + spacing * 2 > x + maxWidth)
                {
                    rowX = x;
                    y += RowHeight + Padding;
                }

                DrawEntryFieldsRow(entry, valueProp, weightProp, colorProp, ref rowX, y, fieldWidth, spacing, entriesProp, i);
            }

            // Add button
            y += RowHeight + Padding;
            if (GUI.Button(new Rect(x, y, ButtonWidth, RowHeight), "Add"))
            {
                AddEntry(entriesProp);
            }

            return y + RowHeight + Padding;
        }

        private void DrawEntryFieldsRow(SerializedProperty entry, SerializedProperty valueProp,
            SerializedProperty weightProp, SerializedProperty colorProp, ref float rowX, float y, float fieldWidth,
            float spacing, SerializedProperty entriesProp, int index)
        {
            // Value
            EditorGUI.PropertyField(new Rect(rowX, y, fieldWidth, RowHeight), valueProp, GUIContent.none);
            rowX += fieldWidth + spacing;

            // Weight
            weightProp.floatValue = Mathf.Max(0f, EditorGUI.FloatField(new Rect(rowX, y, fieldWidth, RowHeight),
                weightProp.floatValue));
            rowX += fieldWidth + spacing;

            // Color
            if (colorProp != null)
            {
                colorProp.colorValue =
                    EditorGUI.ColorField(new Rect(rowX, y, fieldWidth, RowHeight), GUIContent.none,
                        colorProp.colorValue, false, false, false);
            }
            rowX += fieldWidth + spacing;

            // Remove
            if (GUI.Button(new Rect(rowX, y, fieldWidth, RowHeight), "X"))
            {
                entriesProp.DeleteArrayElementAtIndex(index);
                rowX = 0; // reset for wrap
            }
            rowX += fieldWidth + spacing;
        }

        #endregion

        #region --- Helpers ---

        private void DrawNormalizeButton(SerializedProperty entriesProp, Rect rect)
        {
            if (GUI.Button(rect, "Normalize Weights"))
                NormalizeEntries(entriesProp);
        }

        private void AddEntry(SerializedProperty entriesProp)
        {
            int idx = entriesProp.arraySize;
            entriesProp.InsertArrayElementAtIndex(idx);
            var newEntry = entriesProp.GetArrayElementAtIndex(idx);
            newEntry.FindPropertyRelative("Weight").floatValue = 1f;
            newEntry.FindPropertyRelative("Color").colorValue = DefaultColor(idx);
        }

        private void NormalizeEntries(SerializedProperty entriesProp)
        {
            float total = GetTotalWeight(entriesProp);
            if (total <= 0f) return;

            for (int i = 0; i < entriesProp.arraySize; i++)
                entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Weight").floatValue /=
                    total;
        }

        private float GetTotalWeight(SerializedProperty entriesProp)
        {
            float total = 0f;
            for (int i = 0; i < entriesProp.arraySize; i++)
                total += Mathf.Max(0f,
                    entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Weight").floatValue);
            return total;
        }

        private bool TryGetEntries(SerializedProperty property, out SerializedProperty entriesProp)
        {
            entriesProp = property.FindPropertyRelative("entries");
            return entriesProp != null;
        }

        private string GetPropertyValueString(SerializedProperty prop)
        {
            if (prop == null) return "Entry";
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Enum:
                    return prop.enumNames[prop.enumValueIndex];
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.String:
                    return prop.propertyPath; // fallback
                default:
                    return prop.displayName;
            }
        }

        private Color DefaultColor(int index)
        {
            Color[] palette =
            {
                Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta, Color.gray
            };
            return palette[index % palette.Length];
        }

        #endregion
        #endif
    }
}
