using UnityEditor;
using UnityEngine;

namespace UIAB
{
    [CustomEditor(typeof(DynamicTextureRenderer))]
    public class DynamicTextureRendererEditor : Editor
    {
        private bool initialized = false;
        protected Event currentEvent;
        public virtual void DrawProperties(SerializedObject serializedObject) { }
        protected virtual void DrawSceneGUI(Event currentEvent) { }
        public override void OnInspectorGUI()
        {
            if (!initialized) Init(serializedObject);

            serializedObject.Update();
            DrawProperties(serializedObject);
            serializedObject.ApplyModifiedProperties();
        }
        public virtual void Init(SerializedObject serializedObject)
        {
            initialized = true;
        }
    }
}