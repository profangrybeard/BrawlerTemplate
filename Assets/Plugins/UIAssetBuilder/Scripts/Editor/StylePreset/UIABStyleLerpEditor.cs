using UnityEditor;
using UnityEngine;

namespace UIAB
{
    [CustomEditor(typeof(UIABStyleLerp))]
    public class UIABStyleLerpEditor : Editor
    {
        private UIABStyleLerp m_StyleLerp;

        private SerializedProperty lerp;
        private SerializedProperty startStyle;
        private SerializedProperty finalStyle;

        private SerializedProperty startData;
        private SerializedProperty startDataFillColor;
        private SerializedProperty startDataOutline;
        private SerializedProperty startDataEffects;
        private SerializedProperty startDataExtraSettings;
        private SerializedProperty startDataCorners;

        private SerializedProperty finalData;
        private SerializedProperty finalDataFillColor;
        private SerializedProperty finalDataOutline;
        private SerializedProperty finalDataEffects;
        private SerializedProperty finalDataExtraSettings;
        private SerializedProperty finalDataCorners;

        private bool showStartStyleSection;
        private bool showFinalStyleSection;
        private bool showCustomStartStyleSection;
        private bool showCustomFinalStyleSection;

        private bool showCustomStartStyleAppearanceSection;
        private bool showCustomStartStyleOutlineSection;
        private bool showCustomStartStyleEffectsSection;
        private bool showCustomStartStyleExtraSettingsSection;

        private bool showCustomFinalStyleAppearanceSection;
        private bool showCustomFinalStyleOutlineSection;
        private bool showCustomFinalStyleEffectsSection;
        private bool showCustomFinalStyleExtraSettingsSection;

        private bool initialized = false;
        public override void OnInspectorGUI()
        {
            if (!initialized) Init(serializedObject);

            serializedObject.Update();
            DrawInspector(serializedObject);
            serializedObject.ApplyModifiedProperties();
        }
        private void DrawInspector(SerializedObject serializedObject)
        {
            DrawProperties(serializedObject);
        }
        private void Init(SerializedObject serializedObject)
        {
            initialized = true;

            m_StyleLerp = (UIABStyleLerp)target;

            lerp = serializedObject.FindProperty("lerp");
            startStyle = serializedObject.FindProperty("startStyle");
            finalStyle = serializedObject.FindProperty("finalStyle");

            startData = serializedObject.FindProperty("startData");
            startDataFillColor = startData.FindPropertyRelative("fillColor");
            startDataOutline = startData.FindPropertyRelative("outline");
            startDataEffects = startData.FindPropertyRelative("effects");
            startDataExtraSettings = startData.FindPropertyRelative("extraSettings");
            startDataCorners = startData.FindPropertyRelative("corners");

            finalData = serializedObject.FindProperty("finalData");
            finalDataFillColor = finalData.FindPropertyRelative("fillColor");
            finalDataOutline = finalData.FindPropertyRelative("outline");
            finalDataEffects = finalData.FindPropertyRelative("effects");
            finalDataExtraSettings = finalData.FindPropertyRelative("extraSettings");
            finalDataCorners = finalData.FindPropertyRelative("corners");
        }
        private void DrawProperties(SerializedObject serializedObject)
        {
            UIABEditorUtils.AddSectionInnerSpace();
            EditorGUILayout.PropertyField(lerp, new GUIContent("Value"));
            UIABEditorUtils.AddSectionInnerSpace();
            UIABEditorUtils.ShowOptions("Start Style", null, ref showStartStyleSection, StartStyleSection);
            UIABEditorUtils.ShowOptions("Final Style", null, ref showFinalStyleSection, FinalStyleSection);
        }

        private void StartStyleSection()
        {
            EditorGUILayout.PropertyField(startStyle);
            if (startStyle.objectReferenceValue == null)
            {
                UIABEditorUtils.ShowOptions("Custom Style", null, ref showCustomStartStyleSection, ShowCustomStartStyleSection, true);
            }
        }
        private void FinalStyleSection()
        {
            EditorGUILayout.PropertyField(finalStyle);
            if (finalStyle.objectReferenceValue == null)
            {
                UIABEditorUtils.ShowOptions("Custom Style", null, ref showCustomFinalStyleSection, ShowCustomFinalStyleSection, true);
            }
        }
        private void ShowCustomStartStyleSection()
        {
            UIABEditorUtils.ShowOptions("Appearance", null, ref showCustomStartStyleAppearanceSection, CustomStartStyleAppearanceSection);
            UIABEditorUtils.DrawProperty(startDataOutline, "Outline", ref showCustomStartStyleOutlineSection, m_StyleLerp.CustomStartData.GetOutline(), startDataOutline.FindPropertyRelative("useOutline"));
            UIABEditorUtils.DrawProperty(startDataEffects, "Effects", ref showCustomStartStyleEffectsSection, m_StyleLerp.CustomStartData.GetEffects(), startDataEffects.FindPropertyRelative("useEffects"));
            UIABEditorUtils.DrawProperty(startDataExtraSettings, "Extra Settings", ref showCustomStartStyleExtraSettingsSection, m_StyleLerp.CustomStartData.GetExtraSettings());
        }
        private void CustomStartStyleAppearanceSection()
        {
            UIABEditorUtils.DrawProperty(startDataFillColor, "Fill Color", m_StyleLerp.CustomStartData.GetFillColor());
            EditorGUILayout.PropertyField(startDataCorners, new GUIContent("Corners"));
        }
        private void ShowCustomFinalStyleSection()
        {
            UIABEditorUtils.ShowOptions("Appearance", null, ref showCustomFinalStyleAppearanceSection, CustomFinalStyleAppearanceSection);
            UIABEditorUtils.DrawProperty(finalDataOutline, "Outline", ref showCustomFinalStyleOutlineSection, m_StyleLerp.CustomFinalData.GetOutline(), finalDataOutline.FindPropertyRelative("useOutline"));
            UIABEditorUtils.DrawProperty(finalDataEffects, "Effects", ref showCustomFinalStyleEffectsSection, m_StyleLerp.CustomFinalData.GetEffects(), finalDataEffects.FindPropertyRelative("useEffects"));
            UIABEditorUtils.DrawProperty(finalDataExtraSettings, "Extra Settings", ref showCustomFinalStyleExtraSettingsSection, m_StyleLerp.CustomFinalData.GetExtraSettings());
        }
        private void CustomFinalStyleAppearanceSection()
        {
            UIABEditorUtils.DrawProperty(finalDataFillColor, "Fill Color", m_StyleLerp.CustomFinalData.GetFillColor());
            EditorGUILayout.PropertyField(finalDataCorners, new GUIContent("Corners"));
        }
    }
}