using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAB
{
    [System.Serializable]
    public struct UIABColor : IShaderProperty, ICustomInspector, IValidatable, ILerpable<UIABColor>
    {
        public static UIABColor Default = new UIABColor(Color.white, Color.white, Color.white, FillStyle.Solid, 0.5f * Vector2.left, 0.5f * Vector2.right, 0.5f);

        [SerializeField] private FillStyle fillStyle;
        [SerializeField] private Color color;
        [SerializeField] private Color colorEnd;
        [SerializeField] private Color colorMid;
        [SerializeField] private Vector2 gradientStart;
        [SerializeField] private Vector2 gradientEnd;
        [SerializeField] [Range(0.001f, 0.999f)] private float gradientMid;

        private Vector4 gradientCoordinates;

        #region GET/SET
        public void SetColor(Color color) { this.color = color; }
        public Color GetColor() { return color; }
        public void SetColorEnd(Color color) { this.colorEnd = color; }
        public Color GetColorEnd() { return colorEnd; }
        public void SetColorMid(Color color) { this.colorMid = color; }
        public Color GetColorMid() { return colorMid; }
        public void SetFillStyle(FillStyle fillStyle) { this.fillStyle = fillStyle; }
        public FillStyle GetFillStyle() { return fillStyle; }
        public void SetGradientCoordinates(Vector2 start, Vector2 end) { gradientStart = start; gradientEnd = end; }
        public Vector2 GetGradientStartCoordinates() { return gradientStart; }
        public Vector2 GetGradientEndCoordinates() { return gradientEnd; }
        public void SetGradientMid(float gradientMid) { this.gradientMid = Mathf.Clamp(gradientMid, 0.001f, 0.999f); }
        public float GetGradientMid() { return gradientMid; }
        #endregion

        public UIABColor(Color color, Color colorEnd, Color colorMid, FillStyle fillStyle, Vector2 gradientStart, Vector2 gradientEnd, float gradientMid)
        {
            this.color = color;
            this.colorEnd = colorEnd;
            this.colorMid = colorMid;
            this.fillStyle = fillStyle;
            this.gradientStart = gradientStart;
            this.gradientEnd = gradientEnd;
            this.gradientMid = gradientMid;

            gradientCoordinates = Vector4.zero;
        }
        public void SetProperties(ShaderRenderer shaderRenderer, string prefix = "")
        {
            shaderRenderer.SetShaderProperty(prefix + "_FillColor", color);
            shaderRenderer.SetShaderProperty(prefix + "_FillStyle", (int)fillStyle);
            if (fillStyle != FillStyle.Solid)
            {
                shaderRenderer.SetShaderProperty(prefix + "_FillColorEnd", colorEnd);
                shaderRenderer.SetShaderProperty(prefix + "_FillCoordinates", gradientCoordinates);
                if (fillStyle > FillStyle.Radial)
                {
                    shaderRenderer.SetShaderProperty(prefix + "_FillColorMid", colorMid);
                    shaderRenderer.SetShaderProperty(prefix + "_FillMidCoordinates", gradientMid);
                }
            }
        }
#if UNITY_EDITOR
        public void DrawProperties(SerializedProperty property, string label = "", params bool[] filter)
        {
            if (filter.Length > 0 && filter[0]) 
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("fillStyle"), new GUIContent("Style"));
            }
            if (fillStyle == FillStyle.Solid)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("color"), label == "" ? GUIContent.none : new GUIContent(label));
            }
            else
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("color"), new GUIContent("Color Start"));
                if (fillStyle > FillStyle.Radial) EditorGUILayout.PropertyField(property.FindPropertyRelative("colorMid"), new GUIContent("Color Mid"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("colorEnd"), new GUIContent("Color End"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("gradientStart"), new GUIContent("Gradient Start"));
                if (fillStyle > FillStyle.Radial) EditorGUILayout.PropertyField(property.FindPropertyRelative("gradientMid"), new GUIContent("Mid Color position"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("gradientEnd"), new GUIContent("Gradient End"));
            }
        }
#endif
        public void Validate()
        {
            
        }
        public void SetCurrentDimension(Vector2 dimension)
        {
            gradientCoordinates = Vector4.Scale(
                new Vector4(gradientStart.x, gradientStart.y, gradientEnd.x, gradientEnd.y),
                new Vector4(dimension.x, dimension.y, dimension.x, dimension.y));
        }
        public UIABColor Lerp(UIABColor target, float lerp)
        {
            UIABColor newColor = new UIABColor();
            newColor.color = Color.Lerp(color, target.color, lerp);
            newColor.colorEnd = Color.Lerp(colorEnd, target.colorEnd, lerp);
            newColor.colorMid = Color.Lerp(colorMid, target.colorMid, lerp);
            newColor.gradientStart = Vector2.Lerp(gradientStart, target.gradientStart, lerp);
            newColor.gradientEnd = Vector2.Lerp(gradientEnd, target.gradientEnd, lerp);
            newColor.gradientMid = Mathf.Lerp(gradientMid, target.gradientMid, lerp);
            return newColor;
        }
    }

    public enum FillStyle { Solid, Linear, Radial, Linear3Colors, Radial3Colors }
}