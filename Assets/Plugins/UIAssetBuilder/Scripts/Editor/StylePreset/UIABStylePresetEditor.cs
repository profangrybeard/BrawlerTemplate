using UnityEditor;
using UnityEngine;

namespace UIAB
{
    [CustomEditor(typeof(UIABStylePreset))]
    public class UIABStylePresetEditor : Editor
    {
        private bool showAppearanceSection = true;

        private UIABStylePreset m_StylePreset;

        private SerializedProperty uniqueId;
        private SerializedProperty fillColor;
        private SerializedProperty outline;
        private SerializedProperty effects;
        private SerializedProperty extraSettings;
        private SerializedProperty corners;

        private bool ShowOutlineSection;
        private bool ShowEffectsSection;
        private bool ShowExtraSettingsSection;

        private bool initialized = false;

        public override void OnInspectorGUI()
        {
            if (!initialized) Init(serializedObject);

            serializedObject.Update();
            DrawInspector(serializedObject);
            serializedObject.ApplyModifiedProperties();
        }

        public void DrawInspector(SerializedObject serializedObject)
        {
            DrawProperties(serializedObject);
        }

        public void Init(SerializedObject serializedObject)
        {
            initialized = true;

            m_StylePreset = (UIABStylePreset)target;

            SerializedProperty data = serializedObject.FindProperty("data");
            uniqueId = serializedObject.FindProperty("uniqueId");
            fillColor = data.FindPropertyRelative("fillColor");
            outline = data.FindPropertyRelative("outline");
            effects = data.FindPropertyRelative("effects");
            extraSettings = data.FindPropertyRelative("extraSettings");
            corners = data.FindPropertyRelative("corners");
        }

        public void DrawProperties(SerializedObject serializedObject)
        {
            UIABEditorUtils.ShowOptions("Appearance", null, ref showAppearanceSection, AppearanceSection);
            UIABEditorUtils.DrawProperty(outline, "Outline", ref ShowOutlineSection, m_StylePreset.GetData().GetOutline(), outline.FindPropertyRelative("useOutline"));
            UIABEditorUtils.DrawProperty(effects, "Effects", ref ShowEffectsSection, m_StylePreset.GetData().GetEffects(), effects.FindPropertyRelative("useEffects"));
            UIABEditorUtils.DrawProperty(extraSettings, "Extra Settings", ref ShowExtraSettingsSection, m_StylePreset.GetData().GetExtraSettings());
            UIABEditorUtils.AddSectionInnerSpace();

            if (GUILayout.Button("Apply All", GUILayout.Height(UIABEditorUtils.BUTTON_HEIGHT)))
            {
                UIABStylePreset.ApplyToAll(uniqueId.stringValue);
            }
        }

        private void AppearanceSection()
        {
            UIABEditorUtils.DrawProperty(fillColor, "Fill Color", m_StylePreset.GetData().GetFillColor());
            EditorGUILayout.PropertyField(corners, new GUIContent("Corners"));
        }
    }
}