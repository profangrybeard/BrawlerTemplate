using UnityEditor;
using UnityEngine;

namespace UIAB
{
    [System.Serializable]
    public struct UIABEffects : IShaderProperty, ICustomInspector, IValidatable, ILerpable<UIABEffects>
    {
        public static UIABEffects Default = new UIABEffects(false, ShadowEffect.Default, ShadowEffect.Default, CenteredGlowEffect.Default, GlowEffect.Default);

        private const string DATA_PREFIX = "_Effect";

        [SerializeField] private bool useEffects;
        [SerializeField] private ShadowEffect innerShadow;
        [SerializeField] private ShadowEffect dropShadow;
        [SerializeField] private CenteredGlowEffect innerGlow;
        [SerializeField] private GlowEffect outerGlow;

        #region GET/SET
        public void SetUseEffects(bool useEffects) { this.useEffects = useEffects; }
        public bool IsUseEffects() { return useEffects; }
        public void SetInnerShadow(ShadowEffect innerShadow) { this.innerShadow = innerShadow; }
        public ShadowEffect GetInnerShadow() { return innerShadow; }
        public void SetDropShadow(ShadowEffect dropShadow) { this.dropShadow = dropShadow; }
        public ShadowEffect GetDropShadow() { return dropShadow; }
        public void SetInnerGlow(CenteredGlowEffect innerGlow) { this.innerGlow = innerGlow; }
        public CenteredGlowEffect GetInnerGlow() { return innerGlow; }
        public void SetOuterGlow(GlowEffect outerGlow) { this.outerGlow = outerGlow; }
        public GlowEffect GetOuterGlow() { return outerGlow; }
        #endregion

        public UIABEffects(bool useEffects, ShadowEffect innerShadow, ShadowEffect dropShadow, CenteredGlowEffect innerGlow, GlowEffect outerGlow)
        {
            this.useEffects = false;
            this.innerShadow = innerShadow;
            this.dropShadow = dropShadow;
            this.innerGlow = innerGlow;
            this.outerGlow = outerGlow;
        }

        public void SetProperties(ShaderRenderer shaderRenderer, string prefix = "")
        {
            if(useEffects)
            {
                shaderRenderer.SetShaderProperty(DATA_PREFIX + "_Use", new Vector4(innerShadow.Use, dropShadow.Use, innerGlow.Use, outerGlow.Use));
                innerShadow.SetProperties(shaderRenderer, DATA_PREFIX + "_InnerShadow");
                dropShadow.SetProperties(shaderRenderer, DATA_PREFIX + "_DropShadow");
                innerGlow.SetProperties(shaderRenderer, DATA_PREFIX + "_InnerGlow");
                outerGlow.SetProperties(shaderRenderer, DATA_PREFIX + "_OuterGlow");
            }
            else
            {
                shaderRenderer.SetShaderProperty(DATA_PREFIX + "_Use", Vector4.zero);
            }
        }

        public Vector2 OuterPoint 
        { get 
            {
                Vector2 dropShadowOuter = dropShadow.OuterPoint;
                return useEffects ? new Vector2(Mathf.Max(outerGlow.OuterPoint, dropShadowOuter.x), Mathf.Max(outerGlow.OuterPoint, dropShadowOuter.y)) : Vector2.zero; 
            } 
        }

        #region INSPECTOR
        #if UNITY_EDITOR
        public void DrawProperties(SerializedProperty property, string label = "", params bool[] filter)
        {
            UIABEditorUtils.DrawProperty(property.FindPropertyRelative("innerShadow"), "Inner Shadow", innerShadow);
            UIABEditorUtils.DrawProperty(property.FindPropertyRelative("dropShadow"), "Drop Shadow", dropShadow);
            UIABEditorUtils.DrawProperty(property.FindPropertyRelative("innerGlow"), "Inner Glow", innerGlow);
            UIABEditorUtils.DrawProperty(property.FindPropertyRelative("outerGlow"), "Outer Glow", outerGlow);
        }
        #endif
        #endregion
        
        public void Validate()
        {
            innerShadow.Validate();
            dropShadow.Validate();
            innerGlow.Validate();
            outerGlow.Validate();
        }

        public UIABEffects Lerp(UIABEffects target, float lerp)
        {
            UIABEffects newEffects = new UIABEffects();
            newEffects.useEffects = useEffects;
            newEffects.innerShadow = innerShadow.Lerp(target.innerShadow, lerp);
            newEffects.dropShadow = dropShadow.Lerp(target.dropShadow, lerp);
            newEffects.innerGlow = innerGlow.Lerp(target.innerGlow, lerp);
            newEffects.outerGlow = outerGlow.Lerp(target.outerGlow, lerp);
            return newEffects;
        }
    }
}