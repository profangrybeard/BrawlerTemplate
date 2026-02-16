using UnityEditor;
using UnityEngine;

namespace UIAB
{
    [CustomEditor(typeof(UIABPolygon)), CanEditMultipleObjects]
    public class PolygonEditor : ShapeRendererEditor
    {
        private SerializedProperty sides;
        private SerializedProperty angle;
        private SerializedProperty roundness;
        private void Awake()
        {

        }

        private void OnEnable()
        {
            sides = serializedObject.FindProperty("sides");
            angle = serializedObject.FindProperty("angle");
            roundness = serializedObject.FindProperty("roundness");

            Init(serializedObject);
        }
        protected override void CustomAppearanceProperties(SerializedObject serializedObject)
        {
            base.CustomAppearanceProperties(serializedObject);
            EditorGUILayout.PropertyField(sides);
            EditorGUILayout.PropertyField(angle);
            EditorGUILayout.PropertyField(roundness, new GUIContent("Corners"));
            UIABEditorUtils.AddSectionInnerSpace();
        }
        protected override Vector4 NewStyleShaderData()
        {
            return Vector4.one * roundness.floatValue;
        }

        private void OnSceneGUI()
        {
            currentEvent = Event.current;

            base.DrawSceneGUI(currentEvent);
        }
    }
}