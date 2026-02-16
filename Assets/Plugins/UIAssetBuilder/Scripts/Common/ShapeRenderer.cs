using UnityEngine;

namespace UIAB
{
    public abstract class ShapeRenderer : ShaderRenderer
    {
        // This class hold common data for all shapes. It also handle styles.

        private const string CLASS_NAME = "ShapeRenderer";

        [SerializeField] private UIABColor fillColor;
        [SerializeField] private UIABOutline outline;
        [SerializeField] private UIABEffects effects;
        [SerializeField] private UIABExtraSettings extraSettings;
        [SerializeField] private UIABStylePreset stylePreset;
        //private int extraOffset = 0;

        #region GET/SET
        public void SetFillColor(UIABColor color) { this.fillColor = color; }
        public void SetFillColor(Color color) { fillColor.SetColor(color); }
        public UIABColor GetFillColorType() { return fillColor; }
        public Color GetFillColor() { return fillColor.GetColor(); }
        public void SetOutline(UIABOutline outline) { this.outline = outline; }
        public UIABOutline GetOutline() { return outline; }
        public void SetEffects(UIABEffects effects) { this.effects = effects; }
        public UIABEffects GetEffects() { return effects; }
        public void SetExtraSettings(UIABExtraSettings extraSettings) { this.extraSettings = extraSettings; }
        public UIABExtraSettings GetExtraSettings() { return extraSettings; }
        public void SetStylePreset(UIABStylePreset stylePreset) { this.stylePreset = stylePreset; }
        public UIABStylePreset GetStylePreset() { return stylePreset; }
        #endregion

        protected abstract float GetPerimeter();
        protected bool UseRoundEdge { get { return outline.UseRoundEdge; } }
        // This should match the same name as in the compute shader output
        protected override string GetTextureID() { return "ResultTexture"; }

        // Hide under-development features
        public virtual bool HasDashedOutline => false;

        // This is how the texture should expand given an offset. Can be overridden by sub-classes
        // Currently the texture expand the same on all sides to guarantee the shape is centered
        protected virtual Vector4 GetOutlineOffset(float offset)
        {
            return Vector4.one * offset;
        }
        protected override Vector4 GetTextureExpansion()
        {
            Vector2 effectOuterpoint = effects.OuterPoint;

            return GetOutlineOffset(outline.OuterPoint) + new Vector4(effectOuterpoint.x, effectOuterpoint.y, effectOuterpoint.x, effectOuterpoint.y);
        }
        protected override Vector2 GetRectOffset()
        {
            return extraSettings.GetCenterOffset();
        }

        // Updates the relevant data before sending to the shader
        protected override void UpdateClassProperties()
        {
            fillColor.SetCurrentDimension(GetDimension());
            outline.CalculateDashRatio(GetPerimeter());

            fillColor.Validate();
            outline.Validate();
            effects.Validate();
            extraSettings.Validate();
            UpdateOffsetAndExpansion(GetRectOffset(), GetTextureExpansion());
        }

        // This sets the data into the shader. Any new variable used by the shader must be sent to it beforehand.
        // SetShaderProperty will handle whether send to the Compute shader or the Vertex
        protected override void SetShaderData() // Set shader data, called on validate, before redrawing shader
        {
            base.SetShaderData();

            fillColor.SetProperties(this);
            outline.SetProperties(this);
            effects.SetProperties(this);
            extraSettings.SetProperties(this);
            //debugValues.SetProperties(this);
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            fillColor = UIABColor.Default;
            outline = UIABOutline.Default;
            effects = UIABEffects.Default;
            extraSettings = UIABExtraSettings.Default;
            stylePreset = null;
        }
#endif
        #region STYLE
        // Save current data into the current Style Preset. This will also change any other element using the same style.
        public void SaveStyle()
        {
            if (stylePreset == null) return;
            stylePreset.SaveStyle(fillColor, outline, effects, extraSettings, SaveShapeStyle());
        }

        // Change data to match current Style Preset
        public void ApplyCurrentStyle()
        {
            if (stylePreset == null) return;

            Vector4 shapeData = Vector4.zero;
            stylePreset.LoadStyle(ref fillColor, ref outline, ref effects, ref extraSettings, ref shapeData);
            ApplyShapeStyle(shapeData);
            Init();
        }

        // Apply current style if id matches the parameter
        public virtual void ApplyStyleIfUsing(string styleId)
        {
            if (stylePreset == null || stylePreset.Id != styleId) return;
            ApplyCurrentStyle();
        }

        // This should be overwritten by the shape to save shape specific data (e.g. corner roundness)
        protected virtual Vector4 SaveShapeStyle() { return Vector4.zero; }

        // This should be overwritten by the shape to load shape specific data (e.g. corner roundness)
        protected virtual void ApplyShapeStyle(Vector4 shapeData) { }
        #endregion
    }
}
