using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIAB
{
    [ExecuteInEditMode]
    public class DynamicTextureRenderer : MaskableGraphic
    {
        // This class is responsible for creating the texture and rect where the shape in drawn
        // TextureRect and MainRect are different. MainRect hold the position and size on screen,
        // while TextureRect holds the dimension of the texture.
        // It is important to note that TextureRect size and Texture size are not always the same.
        // The Texture always fits inside the TextureRect, but can be scaled throught the Resolution

        private const string CLASS_NAME = "DTRenderer";
        private static readonly RenderTextureFormat DEFAULT_TEXT_FORMAT = RenderTextureFormat.Default;

        [SerializeField] [Range(0.1f, 10f)] private float resolution = 1;
        [SerializeField] private bool useCustomDimensions = false;
        [SerializeField] private Vector2 dimension = new Vector2(100, 100);

        [SerializeField] [HideInInspector] private RenderTexture renderTexture;

        private float previousResolution;
        private bool previousUseCustomDimensions;
        private Vector2 previousDimension;
        private bool isIniting = false;

        private Rect mainRect;      // Actual rect size as seem on screen
        private Rect textureRect;   // Used to adjust texture resolution to rect bounds

        private Vector2Int actualTextureDimension;
        private Vector2 rectOffset;
        private Vector4 textureExpansion = Vector4.zero; // x: left, y: down, z: right, w: up
        private Action onMeshPopulated;
        private Action onDimensionChanged;

        protected virtual float Resolution { get => resolution; }
        public RenderTexture RenderTexture { get => renderTexture; }
        public Rect MainRect { get { if (mainRect.width == 0f) mainRect = GetMainRect(rectOffset); return mainRect; } }
        public Vector2Int TextureDimension { get => actualTextureDimension; }
        public Vector2 TextureOrigin { get => rectTransform.position - (Vector3)(textureRect.size * 0.5f); }
        protected Action OnMeshPopulated { set => onMeshPopulated = value; }
        protected Action OnDimensionChanged { set => onDimensionChanged = value; }

        #region GET/SET
        public void SetDimension(Vector2 dimension) { this.dimension = dimension; }
        public Vector2 GetDimension() { return dimension; }
        public void SetResolution(float resolution) { this.resolution = resolution; }
        public float GetResolution() { return resolution; }
        public void SetUseCustomDimensions(bool useCustomDimensions) { this.useCustomDimensions = useCustomDimensions; }
        public bool IsUseCustomDimensions() { return useCustomDimensions; }
        #endregion

        public override Texture mainTexture
        {
            get
            {
                return renderTexture;
            }
        }
        
        // Update the (rect center) Offset and (texture size) Expansion
        // It will trigger a new draw request for the shader
        public void UpdateOffsetAndExpansion(Vector2 offset, Vector4 expansion)
        {
            rectOffset = offset;
            if (!UpdateTextureExpansion(expansion, isIniting)) SetAllDirty();
        }
        
        // Initialize values
        protected virtual void Init()
        {
            isIniting = true;
            previousResolution = Resolution;
            previousUseCustomDimensions = useCustomDimensions;
            previousDimension = dimension;
        }
        protected void InitDone()
        {
            isIniting = false;
        }
        
        // Create texture. 
        // textureExpansion: extend texture size
        // rectOffset: offsets the center of the drawing rect
        protected void CreateTexture(Vector4 textureExpansion, Vector2 rectOffset)
        {
            UpdateTextureExpansion(textureExpansion, true);

            UpdateTextureRect(rectOffset);

            Vector2Int newTextureDimension = new Vector2Int(Mathf.CeilToInt(textureRect.width * Resolution), Mathf.CeilToInt(textureRect.height * Resolution));

            if (renderTexture == null || newTextureDimension.x != actualTextureDimension.x || newTextureDimension.y != actualTextureDimension.y)
            {
                actualTextureDimension = newTextureDimension;

                if (actualTextureDimension.x > 0 && actualTextureDimension.y > 0)
                {
                    if (renderTexture)
                    {
                        renderTexture.Release();
                        renderTexture.width = actualTextureDimension.x;
                        renderTexture.height = actualTextureDimension.y;
                    }
                    else
                    {
                        renderTexture = new RenderTexture(actualTextureDimension.x, actualTextureDimension.y, 0);
                    }

                    renderTexture.format = DEFAULT_TEXT_FORMAT;
                    renderTexture.enableRandomWrite = true;
                    renderTexture.filterMode = FilterMode.Point;
                    renderTexture.Create();
                }
            }
            SetAllDirty();
        }
        
        // Check if the rect dimension changed since last time it was drawn
        protected bool CheckDimensionChange()
        {
            if (useCustomDimensions != previousUseCustomDimensions)
            {
                dimension = RectTransformUtility.PixelAdjustRect(rectTransform, canvas).size;
                HandleDimensionChange();
                return true;
            }
            if (useCustomDimensions && Vector2.Distance(previousDimension, dimension) > 0.1f)
            {
                HandleDimensionChange();
                return true;
            }
            if (previousResolution != Resolution)
            {
                HandleDimensionChange();
                return true;
            }
            return false;
        }

        private void HandleDimensionChange()
        {
            previousUseCustomDimensions = useCustomDimensions;
            previousDimension = dimension;
            previousResolution = Resolution;

            onDimensionChanged?.Invoke();
        }
        private void UpdateTextureRect(Vector2 rectOffset)
        {
            float maxHorizontalOffset = Mathf.Max(textureExpansion.x, textureExpansion.z);
            float maxVerticalOffset = Mathf.Max(textureExpansion.y, textureExpansion.w);

            mainRect = GetMainRect(rectOffset);

            if (textureRect == null) textureRect = new Rect(mainRect);
            textureRect.width = mainRect.width + 2 * maxHorizontalOffset;
            textureRect.height = mainRect.height + 2 * maxVerticalOffset;
            textureRect.center = mainRect.center;
        }
        private Rect GetMainRect(Vector2 rectOffset)
        {
            Rect rect = GetPixelAdjustedRect();
            if (useCustomDimensions)
            {
                rect.x = -rectTransform.pivot.x * dimension.x;
                rect.y = -rectTransform.pivot.y * dimension.y;
                rect.width = dimension.x;
                rect.height = dimension.y;
            }
            rect.x += rectOffset.x;
            rect.y += rectOffset.y;

            return rect;
        }
        private bool UpdateTextureExpansion(Vector4 expansion = new Vector4(), bool isFirst = false) //(left, down, right, up
        {
            if (Vector4.Distance(expansion, textureExpansion) < 0.01f) return false;

            textureExpansion = expansion;

            if (!isFirst) onDimensionChanged?.Invoke();
            return !isFirst;
        }
        
        #region UNITY_FUNCTIONS
        // Create the texture inside the rect
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            UpdateTextureRect(rectOffset);

            var color32 = Color.white;
            vh.Clear();
            vh.AddVert(new Vector3(textureRect.xMin, textureRect.yMin), color32, new Vector2(0, 0));
            vh.AddVert(new Vector3(textureRect.xMin, textureRect.yMax), color32, new Vector2(0, 1));
            vh.AddVert(new Vector3(textureRect.xMax, textureRect.yMax), color32, new Vector2(1, 1));
            vh.AddVert(new Vector3(textureRect.xMax, textureRect.yMin), color32, new Vector2(1, 0));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);

            onMeshPopulated();
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            dimension.x = Mathf.Max(dimension.x, 0);
            dimension.y = Mathf.Max(dimension.y, 0);
        }
#endif
        protected override void OnEnable()
        {
            base.OnEnable();

            // When copy/pasting a shape the renderTexture is copied over.
            // This guarantee it is always unique/
            renderTexture = null;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            if (!useCustomDimensions)
            {
                dimension = RectTransformUtility.PixelAdjustRect(rectTransform, canvas).size;
                HandleDimensionChange();
            }
        }
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            resolution = 1f;
            useCustomDimensions = false;
            dimension = new Vector2(100, 100);
        }
#endif
#endregion
    }

}