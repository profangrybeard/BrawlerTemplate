using UnityEngine;
using static UnityEngine.Mesh;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAB
{
    [System.Serializable]
    public struct UIABOutline: IShaderProperty, ICustomInspector, IValidatable, ILerpable<UIABOutline>
    {
        public static UIABOutline Default = new UIABOutline(false, UIABColor.Default, OutlineOrientation.Center, EdgeType.Sharp, 4f, 0f, OutlineStyle.Solid, 8f, 4f, 0f);

        [SerializeField] private bool useOutline;
        [SerializeField] private UIABColor color;
        [SerializeField] private OutlineOrientation orientation;
        [SerializeField] private EdgeType edgeType;
        [SerializeField] private OutlineStyle outlineStyle;
        [SerializeField] private float thickness;
        [SerializeField] private float offset;
        [SerializeField] private float dashLength;
        [SerializeField] private float gapLength;
        [SerializeField] [Range(0f,1f)] private float dashOffset;

        private float dashRatio;
        private float gapRatio;
        private Vector4 shaderData;
        private Vector4 dashData;

        public bool UseRoundEdge { get { return edgeType == EdgeType.Rounded; } }
        public float OuterPoint { get { return useOutline ? Mathf.Max(0, OutlineEdge) : 0f; } }
        public float OutlineEdge { get { return offset + GetOrientationMultiplier(orientation) * thickness; } }

        #region GET/SET
        public void UseOutline(bool useOutline) { this.useOutline = useOutline; }
        public bool IsUseOutline() { return useOutline; }
        public void SetColor(Color color) { this.color.SetColor(color); }
        public Color GetColor() { return color.GetColor(); }
        public void SetOrientation(OutlineOrientation orientation) { this.orientation = orientation; }
        public OutlineOrientation GetOrientation() { return orientation; }
        public void SetEdgeType(EdgeType edgeType) { this.edgeType = edgeType; }
        public EdgeType GetEdgeType() { return edgeType; }
        public void SetThickness(float thickness) { this.thickness = thickness; }
        public float GetThickness() { return thickness; }
        public void SetOffset(float offset) { this.offset = offset; }
        public float GetOffset() { return offset; }
        public void SetDashLength(float dashLength) { this.dashLength = dashLength; }
        public float GetDashLength() { return dashLength; }
        public void SetGapLength(float gapLength) { this.gapLength = gapLength; }
        public float GetGapLength() { return gapLength; }
        public void SetDashOffset(float dashOffset) { this.dashOffset = Mathf.Clamp01(dashOffset); }
        public float GetDashOffset() { return dashOffset; }
        #endregion

        public UIABOutline(bool useOutline = false, UIABColor color = new UIABColor(),
            OutlineOrientation orientation = new OutlineOrientation(), EdgeType edgeType = new EdgeType(),
            float thickness = 4f, float offset = 0f, OutlineStyle outlineStyle = OutlineStyle.Solid, float dashLength = 1f, float gapLength = 1f, float dashOffset = 0f)
        {
            this.useOutline = useOutline;
            this.color = color;
            this.orientation = orientation;
            this.edgeType = edgeType;
            this.thickness = thickness;
            this.offset = offset;
            this.outlineStyle = outlineStyle;
            this.dashLength = dashLength;
            this.gapLength = dashLength;
            this.dashOffset = dashOffset;

            dashRatio = 0f;
            gapRatio = 0f;
            shaderData = Vector4.zero;
            dashData = Vector4.zero;
        }
        
        public void SetProperties(ShaderRenderer shaderRenderer, string prefix = "")
        {
            if(useOutline)
            {
                color.SetProperties(shaderRenderer, "_Outline");
                shaderData.x = OutlineEdge; 
                shaderData.y = shaderData.x - thickness; 
                shaderData.z = UseRoundEdge ? 0 : 1; 
                shaderData.w = 1;
                shaderRenderer.SetShaderProperty("_Outline_Edges_Type_Has", shaderData);

                dashData.x = outlineStyle == OutlineStyle.Solid ? 0f : 1f;
                dashData.y = dashRatio;
                dashData.z = outlineStyle == OutlineStyle.UniformDashed ? dashRatio : gapRatio;
                dashData.w = dashOffset;
                shaderRenderer.SetShaderProperty("_Outline_IsDashed_DashRatio_GapRatio_DashOffset", dashData);
            }
            else
            {
                shaderRenderer.SetShaderProperty("_Outline_Edges_Type_Has", Vector4.zero);
            }
        }

        private float GetOrientationMultiplier(OutlineOrientation orientation)
        {
            switch (orientation)
            {
                case OutlineOrientation.Out: return 1f;
                case OutlineOrientation.Center: return 0.5f;
                case OutlineOrientation.In: return 0f;
            }
            return 0f;
        }

        public void CalculateDashRatio(float shapePerimeter)
        {
            shapePerimeter = Mathf.Max(shapePerimeter, 0f);
            dashRatio = 2f * dashLength / shapePerimeter;
            gapRatio = 2f * gapLength / shapePerimeter;
        }

        public void Validate()
        {
            thickness = Mathf.Max(thickness, 0);
        }

        #region INSPECTOR
        #if UNITY_EDITOR
        public void DrawProperties(SerializedProperty property, string label = "", params bool[] filter)
        {
            UIABEditorUtils.DrawProperty(property.FindPropertyRelative("color"), "Color", color);

            UIABEditorUtils.AddSectionInnerSpace();

            EditorGUILayout.PropertyField(property.FindPropertyRelative("orientation"), new GUIContent("Orientation"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("edgeType"), new GUIContent("Edge"));
            
            if (filter.Length > 0 && filter[0])
                EditorGUILayout.PropertyField(property.FindPropertyRelative("outlineStyle"), new GUIContent("Style"));

            if (outlineStyle != OutlineStyle.Solid)
            {
                UIABEditorUtils.AddSectionInnerSpace();
                EditorGUILayout.PropertyField(property.FindPropertyRelative("dashLength"), new GUIContent("Dash Length"));
                if (outlineStyle == OutlineStyle.Dashed)  EditorGUILayout.PropertyField(property.FindPropertyRelative("gapLength"), new GUIContent("Gap Length"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("dashOffset"), new GUIContent("Dash Offset"));
            }

            UIABEditorUtils.AddSectionInnerSpace();

            EditorGUILayout.PropertyField(property.FindPropertyRelative("thickness"), new GUIContent("Thickness"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("offset"), new GUIContent("Offset"));
        }
#endif
#endregion
        
        public UIABOutline Lerp(UIABOutline target, float lerp)
        {
            UIABOutline newOutline = new UIABOutline();
            newOutline.useOutline = useOutline;
            newOutline.color = color.Lerp(target.color, lerp);
            newOutline.orientation = orientation;
            newOutline.edgeType = edgeType;
            newOutline.thickness = Mathf.Lerp(thickness, target.thickness, lerp);
            newOutline.offset = Mathf.Lerp(offset, target.offset, lerp);
            newOutline.dashLength = Mathf.Lerp(dashLength, target.dashLength, lerp);
            newOutline.dashOffset = Mathf.Lerp(dashOffset, target.dashOffset, lerp);
            return newOutline;
        }
    }

    [System.Serializable]
    public enum OutlineOrientation { Center, Out, In }

    [System.Serializable]
    public enum EdgeType { Sharp, Rounded }

    [System.Serializable]
    public enum OutlineStyle { Solid, UniformDashed, Dashed }
}