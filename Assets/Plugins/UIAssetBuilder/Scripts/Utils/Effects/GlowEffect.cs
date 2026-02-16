using UnityEditor;
using UnityEngine;

namespace UIAB
{
    [System.Serializable]
    public struct GlowEffect : IShaderProperty, ICustomInspector, IValidatable, ILerpable<GlowEffect>
    {
        public static readonly GlowEffect Default = new GlowEffect(false, Color.white, true, 2f, 0f, 5f);

        [SerializeField] private bool useEffect;
        [SerializeField] private Color color;
        [SerializeField] private bool isLinear;
        [SerializeField][Range(0.1f, 5f)] private float exponentialDecrease;
        [SerializeField][Range(0f, 1f)] private float choke;
        [SerializeField] private float size;

        public int Use { get => useEffect ? 1 : 0; }
        public float OuterPoint { get => useEffect ? size : 0; }

        #region GET/SET
        public void UseEffect(bool useEffect) { this.useEffect = useEffect; }
        public bool IsUseEffect() { return useEffect; }
        public void SetColor(Color color) { this.color = color; }
        public Color GetColor() { return color; }
        public void SetLinear(bool isLinear) { this.isLinear = isLinear; }
        public bool IsLinear() { return isLinear; }
        public void SetExponentialDecrease(float exponentialDecrease) { this.exponentialDecrease = exponentialDecrease; }
        public float GetExponentialDecrease() { return exponentialDecrease; }
        public void SetChoke(float choke) { this.choke = choke; }
        public float GetChoke() { return choke; }
        public void SetSize(float size) { this.size = size; }
        public float GetSize() { return size; }
        #endregion

        public GlowEffect(bool useEffect = false, Color color = new Color(), bool isLinear = true, float exponentialDecrease = 2f, float choke = 0f, float size = 5f)
        {
            this.useEffect = useEffect;
            this.isLinear = isLinear;
            this.exponentialDecrease = exponentialDecrease;
            this.choke = choke;
            this.size = size;
            this.color = color;

            showEffectSection = false;
        }
        
        public void SetProperties(ShaderRenderer shaderRenderer, string prefix = "")
        {
            if (useEffect)
            {
                shaderRenderer.SetShaderProperty(prefix + "_Color", color);
                shaderRenderer.SetShaderProperty(prefix + "_IsLinear_Choke_Size", new Vector4(isLinear ? 1 : exponentialDecrease, choke, size));
            }
        }
        
        public void Validate()
        {
            size = Mathf.Max(size, 0);
        }

        #region INSPECTOR
#pragma warning disable 0414
        [SerializeField][HideInInspector] private bool showEffectSection;
#pragma warning restore 0414
        #if UNITY_EDITOR
        public void DrawProperties(SerializedProperty property, string label = "", params bool[] filter)
        {
            bool isLinear = this.isLinear;
            UIABEditorUtils.ShowOptions(label, property.FindPropertyRelative("useEffect"), property.FindPropertyRelative("showEffectSection"), () => { EffectsSection(property, isLinear); }, true);
        }
        private static void EffectsSection(SerializedProperty property, bool isLinear)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("size"), new GUIContent("Size"));
            UIABEditorUtils.AddSectionInnerSpace();
            EditorGUILayout.PropertyField(property.FindPropertyRelative("isLinear"), new GUIContent("Use Linear Fade"));
            if (!isLinear)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("exponentialDecrease"), new GUIContent("Exponential Fade"));
            }
            EditorGUILayout.PropertyField(property.FindPropertyRelative("choke"), new GUIContent("Choke"));
            UIABEditorUtils.AddSectionInnerSpace();
            EditorGUILayout.PropertyField(property.FindPropertyRelative("color"), new GUIContent("Color"));
        }
        #endif
        #endregion
        public GlowEffect Lerp(GlowEffect target, float lerp)
        {
            GlowEffect effect = new GlowEffect();
            effect.useEffect = useEffect;
            effect.isLinear = isLinear;
            effect.exponentialDecrease = Mathf.Lerp(exponentialDecrease, target.exponentialDecrease, lerp);
            effect.choke = Mathf.Lerp(choke, target.choke, lerp);
            effect.size = Mathf.Lerp(size, target.size, lerp);
            effect.color = Color.Lerp(color, target.color, lerp);
            return effect;
        }

    }
}