using UnityEditor;
using UnityEngine;

namespace UIAB
{
    [CustomEditor(typeof(UIABCircle)), CanEditMultipleObjects]
    public class CircleEditor : ShapeRendererEditor
    {
        //public CircleInspector inspector;
        private void Awake()
        {

        }

        private void OnEnable()
        {
            Init(serializedObject);
        }

        private void OnSceneGUI()
        {
            currentEvent = Event.current;

            base.DrawSceneGUI(currentEvent);
        }
    }
}