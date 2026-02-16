using UnityEditor;
using UnityEngine;

namespace UIAB
{
    [CustomEditor(typeof(UIABMore))]
    public class MoreEditor : ShapeRendererEditor
    {
        private UIABMore more;

        private SerializedProperty shapeType;
        // Moon
        private SerializedProperty moonD;
        private SerializedProperty moonRb;
        // Egg
        private SerializedProperty eggRb;
        // Cross
        private SerializedProperty crossB2;
        private SerializedProperty crossR;
        // X
        private SerializedProperty xR;
        // Checkamark
        private SerializedProperty checkmarkR;
        private void Awake()
        {

        }

        private void OnEnable()
        {
            more = (UIABMore)target;

            shapeType = serializedObject.FindProperty("shapeType");
            moonD = serializedObject.FindProperty("moonD");
            moonRb = serializedObject.FindProperty("moonRb");
            eggRb = serializedObject.FindProperty("eggRb");
            crossB2 = serializedObject.FindProperty("crossB2");
            crossR = serializedObject.FindProperty("crossR");
            xR = serializedObject.FindProperty("xR");
            checkmarkR = serializedObject.FindProperty("checkmarkR");

            Init(serializedObject);
        }
        protected override void CustomAppearanceProperties(SerializedObject serializedObject)
        {
            base.CustomAppearanceProperties(serializedObject);
            EditorGUILayout.PropertyField(shapeType);
            switch (more.GetShapeType())
            {
                case MoreShapeType.Heart:
                    break;
                case MoreShapeType.Moon:
                    EditorGUILayout.PropertyField(moonD, new GUIContent("Inner Circle Position"));
                    EditorGUILayout.PropertyField(moonRb, new GUIContent("Inner Radius"));
                    break;
                case MoreShapeType.Egg:
                    EditorGUILayout.PropertyField(eggRb, new GUIContent("Eggness"));
                    break;
                case MoreShapeType.Cross:
                    EditorGUILayout.PropertyField(crossB2, new GUIContent("Thickness"));
                    EditorGUILayout.PropertyField(crossR, new GUIContent("Roundness"));
                    break;
                case MoreShapeType.X:
                    EditorGUILayout.PropertyField(xR, new GUIContent("Thickness"));
                    break;
                case MoreShapeType.Checkmark:
                    EditorGUILayout.PropertyField(checkmarkR, new GUIContent("Thickness"));
                    break;
                default:
                    break;
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