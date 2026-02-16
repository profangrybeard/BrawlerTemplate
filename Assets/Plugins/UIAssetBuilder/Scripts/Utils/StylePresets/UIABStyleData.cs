using System;
using UnityEngine;

namespace UIAB
{
    [Serializable]
    public struct UIABStyleData : ILerpable<UIABStyleData>
    {
        [SerializeField] private UIABColor fillColor;
        [SerializeField] private UIABOutline outline;
        [SerializeField] private UIABEffects effects;
        [SerializeField] private UIABExtraSettings extraSettings;
        [SerializeField] private Vector4 corners;

        #region GET
        public UIABColor GetFillColor() { return fillColor; }
        public UIABOutline GetOutline() { return outline; }
        public UIABEffects GetEffects() { return effects; }
        public UIABExtraSettings GetExtraSettings() { return extraSettings; }
        public Vector4 GetShapeData() { return corners; }
        #endregion

        public UIABStyleData(UIABColor fillColor, UIABOutline outline, UIABEffects effects, UIABExtraSettings extraSettings, Vector4 corners)
        {
            this.fillColor = fillColor;
            this.outline = outline;
            this.effects = effects;
            this.extraSettings = extraSettings;
            this.corners = corners;
        }

        public void PersistentLerp(UIABStyleData start, UIABStyleData final, float lerp)
        {
            fillColor = start.fillColor.Lerp(final.fillColor, lerp);
            outline = start.outline.Lerp(final.outline, lerp);
            effects = start.effects.Lerp(final.effects, lerp);
            extraSettings = start.extraSettings.Lerp(final.extraSettings, lerp);
            corners = Vector4.Lerp(start.corners, final.corners, lerp);
        }

        public UIABStyleData Lerp(UIABStyleData target, float lerp)
        {
            UIABStyleData newStyle = new UIABStyleData();

            newStyle.fillColor = fillColor.Lerp(target.fillColor, lerp);
            newStyle.outline = outline.Lerp(target.outline, lerp);
            newStyle.effects = effects.Lerp(target.effects, lerp);
            newStyle.extraSettings = extraSettings.Lerp(target.extraSettings, lerp);
            newStyle.corners = Vector4.Lerp(corners, target.corners, lerp);

            return newStyle;
        }

#if UNITY_EDITOR
        public void Reset()
        {
            fillColor = UIABColor.Default;
            outline = UIABOutline.Default;
            effects = UIABEffects.Default;
            extraSettings = UIABExtraSettings.Default;
            corners = Vector4.zero;
        }
#endif
    }
}