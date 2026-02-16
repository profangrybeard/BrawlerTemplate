using UnityEditor;
using UnityEngine;

namespace UIAB
{
    [CustomEditor(typeof(UIABProgress))]
    public class ProgressEditor : ShapeRendererEditor
    {
        private SerializedProperty shape;
        private SerializedProperty progress;
        private SerializedProperty thickness;
        private void Awake()
        {

        }

        private void OnEnable()
        {
            shape = serializedObject.FindProperty("shape");
            progress = serializedObject.FindProperty("progress");
            thickness = serializedObject.FindProperty("thickness");

            Init(serializedObject);
        }
        protected override void CustomAppearanceProperties(SerializedObject serializedObject)
        {
            base.CustomAppearanceProperties(serializedObject);
            EditorGUILayout.PropertyField(shape);
            EditorGUILayout.PropertyField(progress);
            if (shape.intValue != 2)
            {
                EditorGUILayout.PropertyField(thickness);
            }
            UIABEditorUtils.AddSectionInnerSpace();
        }

        private void OnSceneGUI()
        {
            currentEvent = Event.current;

            base.DrawSceneGUI(currentEvent);
        }
    }
}