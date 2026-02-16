using UnityEditor;
using UnityEngine;

namespace UIAB
{
    [System.Serializable]
    public struct ShadowEffect : IShaderProperty, ICustomInspector, IValidatable, ILerpable<ShadowEffect>
    {
        public static readonly ShadowEffect Default = new ShadowEffect(false, new Color(0f,0f,0f,0.5f), UIABDistance.Default, 0f, 0f);

        [SerializeField] private bool useEffect;
        [SerializeField] private Color color;
        [SerializeField] private UIABDistance distance;
        [SerializeField] private float blur;
        [SerializeField] private float spread;

        private Vector2 effectDistance;
        public int Use { get => useEffect ? 1 : 0; }

        #region GET/SET
        public void UseEffect(bool useEffect) { this.useEffect = useEffect; }
        public bool IsUseEffect() { return useEffect; }
        public void SetColor(Color color) { this.color = color; }
        public Color GetColor() { return color; }
        public void SetDistance(UIABDistance distance) { this.distance = distance; }
        public UIABDistance GetDistance() { return distance; }
        public void SetBlur(float blur) { this.blur = blur; }
        public float GetBlur() { return blur; }
        public void SetSpread(float spread) { this.spread = spread; }
        public float GetSpread() { return spread; }
        #endregion
        public Vector2 OuterPoint
        {
            get
            {
                effectDistance = distance.GetActualOffset();
                return useEffect ? new Vector2(Mathf.Abs(effectDistance.x), Mathf.Abs(effectDistance.y)) + Vector2.one * (blur - spread) : Vector2.zero;
            }
        }

        public ShadowEffect(bool useEffect = false, Color color = new Color(), UIABDistance distance = new UIABDistance(), float blur = 0f, float spread = 0f)
        {
            this.useEffect = useEffect;
            this.distance = distance;
            this.blur = blur;
            this.spread = spread;
            this.color = color;

            effectDistance = Vector2.zero;
            showEffectSection = false;
        }
        
        public void SetProperties(ShaderRenderer shaderRenderer, string prefix = "")
        {
            if (useEffect)
            {
                effectDistance = distance.GetActualOffset();
                shaderRenderer.SetShaderProperty(prefix + "_Color", color);
                shaderRenderer.SetShaderProperty(prefix + "_Distance_Blur_Spread", new Vector4(effectDistance.x, effectDistance.y, blur, spread));
            }
        }
        
        public void Validate()
        {
            distance.Validate();
            blur = Mathf.Max(blur, 0);
            spread = Mathf.Max(spread, 0);
        }

        #region INSPECTOR
#pragma warning disable 0414
        [SerializeField][HideInInspector] private bool showEffectSection;
#pragma warning restore 0414
        #if UNITY_EDITOR
        public void DrawProperties(SerializedProperty property, string label = "", params bool[] filter)
        {
            UIABDistance uIABDistance = distance;
            UIABEditorUtils.ShowOptions(label, property.FindPropertyRelative("useEffect"), property.FindPropertyRelative("showEffectSection"), () => { EffectsSection(property, uIABDistance); }, true);
        }
        private static void EffectsSection(SerializedProperty property, ICustomInspector distanceInspector)
        {
            UIABEditorUtils.DrawProperty(property.FindPropertyRelative("distance"), "Position", distanceInspector);
            UIABEditorUtils.AddSectionInnerSpace();
            EditorGUILayout.PropertyField(property.FindPropertyRelative("blur"), new GUIContent("Blur"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("spread"), new GUIContent("Spread"));
            UIABEditorUtils.AddSectionInnerSpace();
            EditorGUILayout.PropertyField(property.FindPropertyRelative("color"), new GUIContent("Color"));
        }
        #endif
        #endregion
        
        public ShadowEffect Lerp(ShadowEffect target, float lerp)
        {
            ShadowEffect effect = new ShadowEffect();
            effect.useEffect = useEffect;
            effect.distance = distance.Lerp(target.distance, lerp);
            effect.blur = Mathf.Lerp(blur, target.blur, lerp);
            effect.spread = Mathf.Lerp(spread, target.spread, lerp);
            effect.color = Color.Lerp(color, target.color, lerp);
            return effect;
        }
    }
}