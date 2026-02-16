using System;
using UnityEngine;

namespace UIAB
{
    [RequireComponent(typeof(CanvasRenderer))]
    public abstract class ShaderRenderer : DynamicTextureRenderer
    {
        // This class renders the shader into the texture/
        // The rendering can be via a compute shader, which will paint the texture pixel by pixel,
        // or via a vertex shader, which create sharper images
        private const string CLASS_NAME = "ShaderRenderer";

        [SerializeField] private bool useComputeShader = false;
        private bool previousShaderMode = false;

        // SHADER
        [SerializeField] [HideInInspector] private ComputeShader shader;
        [SerializeField] [HideInInspector] private Material shapeMaterial;
        [SerializeField] [Range(0f, 1f)] private float sharpness = 0f;

        private int kernelID = 0;
        private int groupSizeX = 1, groupSizeY = 1;
        private bool isShaderInit = false;
        private Material materialInstance;
        private Material MaterialInstance {  get
            {
                if (materialInstance == null)
                {
                    materialInstance = new Material(shapeMaterial);
                    material = materialInstance;
                }
                return materialInstance;
            } }

        #region GET/SET
        public void SetRasterize(bool rasterize) { useComputeShader = rasterize; }
        public bool IsRasterize() { return useComputeShader; }
        public void SetSharpness(float sharpness) { this.sharpness = sharpness; }
        public float GetSharpness() { return sharpness; }
        #endregion

        protected override float Resolution { get => useComputeShader ? base.Resolution : 1f; }
        protected abstract string GetKernelID();
        protected abstract string GetTextureID();
        protected abstract Vector4 GetTextureExpansion();
        protected abstract Vector2 GetRectOffset();
        protected virtual void ReleaseBuffer(){}
        protected virtual void UpdateClassProperties(){}
        
        // This sets the data into the shader. Any new variable used by the shader must be sent to it beforehand.
        // SetShaderProperty will handle whether send to the Compute shader or the Vertex
        protected virtual void SetShaderData()
        {
            SetShaderProperty("_Sharpness", sharpness);
            if (useComputeShader && shader && mainTexture) //This avoid multiple shapes using compute shader to draw on each other
            {
                if (mainTexture && mainTexture.width > 0 && mainTexture.height > 0)
                {
                    shader.SetTexture(kernelID, GetTextureID(), mainTexture);
                    SetShaderProperty($"_{GetTextureID()}_Dimension", new Vector4(TextureDimension.x, TextureDimension.y));
                }
            }
        }
        // Request for the image to be redrawn. Necessary to make any change visible
        public void RenderShader()
        {
            if (isShaderInit && previousShaderMode == useComputeShader)
            {
                if (!CheckDimensionChange())
                {
                    UpdateClassProperties();
                    RedrawShader();
                }
            }
            else
            {
                previousShaderMode = useComputeShader;
                Init();
            }
        }
        protected override void Init()
        {
            base.Init();

            CreateTexture(GetTextureExpansion(), GetRectOffset());
            UpdateClassProperties();
            InitShader();
            DrawShader();
        }
        protected void RedrawShader()
        {
            SetShaderData();
            DrawShader();
        }
        private void InitShader()
        {
            if (shader == null)
            {
                return;
            };
            
            kernelID = shader.FindKernel(GetKernelID());

            uint threadsX, threadsY;
            shader.GetKernelThreadGroupSizes(kernelID, out threadsX, out threadsY, out _);
            groupSizeX = Mathf.CeilToInt(TextureDimension.x / (float)threadsX);
            groupSizeY = Mathf.CeilToInt(TextureDimension.y / (float)threadsY);

            if (mainTexture && mainTexture.width > 0 && mainTexture.height > 0)
            {
                shader.SetTexture(kernelID, GetTextureID(), mainTexture);
                SetShaderProperty($"_{GetTextureID()}_Dimension", new Vector4(TextureDimension.x, TextureDimension.y));
                SetShaderProperty("_Sharpness", sharpness);
                SetShaderProperty("_Resolution", Resolution);

                MaterialInstance.SetInt("_UseComputeShader", useComputeShader ? 1 : 0);

                SetShaderData();

                isShaderInit = true;
                InitDone();
            }
        }
        private void DrawShader()
        {
            if (shader == null)
            {
                return;
            }
            if (useComputeShader && groupSizeX > 0 && groupSizeY > 0 && mainTexture)
            {
                shader.Dispatch(kernelID, groupSizeX, groupSizeY, 1);
            }
        }
        private void HandleDimensionChange()
        {
            Init();
        }

        #region UNITY_FUNCTIONS
        protected override void OnEnable()
        {
            base.OnEnable();

            // When copy/pasting a shape the material instance is copied over. This guarantee it is always unique
            materialInstance = null; 
            OnMeshPopulated = DrawShader;
            OnDimensionChanged = HandleDimensionChange;
            Init();

#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= ReleaseBuffer;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += ReleaseBuffer;
#endif
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            RenderShader();
        }
#endif
        protected override void OnDidApplyAnimationProperties()
        {
            RenderShader();
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            isShaderInit = false;
            useComputeShader = false;
            previousShaderMode = false;
            sharpness = 0f;
        }
#endif
#endregion
        #region SHADER_PROPERTIES
        public void SetShaderProperty(string propertyId, int value)
        {
            // Int values do not work on computer shader
            if (useComputeShader) shader?.SetFloat(propertyId, value);
            else MaterialInstance.SetInt(propertyId, value);
        }
        public void SetShaderProperty(string propertyId, float value)
        {
            if (useComputeShader) shader?.SetFloat(propertyId, value);
            else MaterialInstance.SetFloat(propertyId, value);
        }
        public void SetShaderProperty(string propertyId, Vector4 value)
        {
            if (useComputeShader) shader?.SetVector(propertyId, value);
            else MaterialInstance.SetVector(propertyId, value);
        }
        #endregion
    }
}
