using UnityEditor;
using UnityEngine;

namespace UIAB
{
    [CustomEditor(typeof(UIABTriangle)), CanEditMultipleObjects]
    public class TriangleEditor : ShapeRendererEditor
    {
        private SerializedProperty forceEquilateral;
        private SerializedProperty corners;
        private void Awake()
        {

        }

        private void OnEnable()
        {
            forceEquilateral = serializedObject.FindProperty("forceEquilateral");
            corners = serializedObject.FindProperty("corners");

            Init(serializedObject);
        }
        protected override void CustomAppearanceProperties(SerializedObject serializedObject)
        {
            base.CustomAppearanceProperties(serializedObject);
            EditorGUILayout.PropertyField(forceEquilateral, new GUIContent("Force Equilateral"));
            EditorGUILayout.PropertyField(corners, new GUIContent("Corner Roundness"));
            UIABEditorUtils.AddSectionInnerSpace();
        }
        protected override Vector4 NewStyleShaderData()
        {
            return Vector4.one * corners.floatValue;
        }

        private void OnSceneGUI()
        {
            currentEvent = Event.current;

            base.DrawSceneGUI(currentEvent);
        }
    }
}