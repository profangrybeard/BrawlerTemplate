using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAB
{
    [System.Serializable]
    public struct UIABExtraSettings : IShaderProperty, ICustomInspector, IValidatable, ILerpable<UIABExtraSettings>
    {
        public static UIABExtraSettings Default = new UIABExtraSettings(tint: Color.white);

        [SerializeField] private Vector2 centerOffset;
        [SerializeField] private Color tint;

        #region GET/SET
        public void SetCenterOffset(Vector2 offset) { centerOffset = offset; }
        public Vector2 GetCenterOffset() { return centerOffset; }
        public void SetTint(Color tint) { this.tint = tint; }
        public Color GetTint() { return tint; }
        #endregion

        public UIABExtraSettings(Vector2 centerOffset = new Vector2(), Color tint = new Color())
        {
            this.centerOffset = centerOffset;
            this.tint = tint;
        }

        public void SetProperties(ShaderRenderer shaderRenderer, string prefix = "")
        {
            shaderRenderer.SetShaderProperty("_Extra_CenterOffset", centerOffset);
            shaderRenderer.SetShaderProperty("_Extra_Tint", tint);
        }

        #region INSPECTOR
        #if UNITY_EDITOR
        public void DrawProperties(SerializedProperty property, string label = "", params bool[] filter)
        {
            UIABEditorUtils.DrawProperty(property.FindPropertyRelative("tint"), "Tint", null); ;
            UIABEditorUtils.DrawProperty(property.FindPropertyRelative("centerOffset"), "Offset Center", null);
        }
        #endif
        #endregion
        
        public void Validate()
        {

        }

        public UIABExtraSettings Lerp(UIABExtraSettings target, float lerp)
        {
            UIABExtraSettings newExtraSettings = new UIABExtraSettings();
            newExtraSettings.centerOffset = Vector2.Lerp(centerOffset, target.centerOffset, lerp);
            newExtraSettings.tint = Color.Lerp(tint, target.tint, lerp);
            return newExtraSettings;
        }
    }
}