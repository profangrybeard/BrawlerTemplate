using UnityEditor;

namespace UIAB
{
    public interface ICustomInspector
    {

#if UNITY_EDITOR
        public abstract void DrawProperties(SerializedProperty property, string label = "", params bool[] filter);
#endif
    }
}