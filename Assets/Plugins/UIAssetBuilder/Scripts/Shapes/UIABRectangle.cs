using UnityEngine;

namespace UIAB
{
    public class UIABRectangle : ShapeRenderer
    {
        // Rectangle
        // Send Rectangle specific data to the shader
        // For more details on how it is drawn check Shader/CGIncludes/Shapes/Rectangle.cginc
        // If you wish to change the shape visual in code,
        // use the Getter/Setter and then call RenderShader()

        private const string CLASS_NAME = "Rectangle";
        private const string DATA_PREFIX = "_" + CLASS_NAME;

        [SerializeField] private float corners = 0;
        [SerializeField] private bool useIndependentCorners = false;
        [SerializeField] private Vector4 independentCorners = Vector4.zero;

        private Vector4 actualCorners;
        private float cornersMax;

        #region GET/SET
        public void SetCorners(float corners) { this.corners = corners; independentCorners = Vector4.one * corners; }
        public void SetCorners(Vector4 corners) { independentCorners = corners; }
        public float GetCorners() { return corners; }
        public Vector4 GetIndependentCorners() { return independentCorners; }
        public void SetUseIndependentCorners(bool useIndependentCorners) { this.useIndependentCorners = useIndependentCorners; }
        public bool IsUseIndependentCorners() { return useIndependentCorners; }
        #endregion

        public void SetCorners(float corners, Vector4 independentCorners)
        {
            this.corners = corners;
            this.independentCorners = independentCorners;
            UpdateClassProperties();
        }

        // Process shader data before sending to the shader
        protected override void UpdateClassProperties()
        {
            base.UpdateClassProperties();

            cornersMax = 0.5f * Mathf.Min(MainRect.width, MainRect.height);
            if (!useIndependentCorners)
            {
                corners = Mathf.Max(corners, 0);
                independentCorners = Vector4.one * corners;
            }
            else
            {
                independentCorners.x = Mathf.Max(independentCorners.x, 0);
                independentCorners.y = Mathf.Max(independentCorners.y, 0);
                independentCorners.z = Mathf.Max(independentCorners.z, 0);
                independentCorners.w = Mathf.Max(independentCorners.w, 0);
            }

            actualCorners.x = Mathf.Min(independentCorners.x, cornersMax);
            actualCorners.y = Mathf.Min(independentCorners.y, cornersMax);
            actualCorners.z = Mathf.Min(independentCorners.z, cornersMax);
            actualCorners.w = Mathf.Min(independentCorners.w, cornersMax);
        }

        // This sets the data into the shader. Any new variable used by the shader must be sent to it beforehand.
        // SetShaderProperty will handle whether send to the Compute shader or the Vertex
        protected override void SetShaderData()
        {
            base.SetShaderData();

            SetShaderProperty(DATA_PREFIX + "_Width", MainRect.width);
            SetShaderProperty(DATA_PREFIX + "_Height", MainRect.height);
            SetShaderProperty(DATA_PREFIX + "_Corners", actualCorners);
        }
        protected override void ApplyShapeStyle(Vector4 shapeData)
        {
            if (useIndependentCorners)
            {
                independentCorners = shapeData;
            }
            else
            {
                corners = shapeData.x;
            }
        }
        protected override Vector4 SaveShapeStyle()
        {
            if (useIndependentCorners)
            {
                return independentCorners;
            }
            else
            {
                return Vector4.one * corners;
            }
        }
        protected override string GetKernelID()
        {
            return "DrawRectangle";
        }
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset(); 

            corners = 0;
            useIndependentCorners = false;
            independentCorners = Vector4.zero;
        }
#endif
        protected override float GetPerimeter()
        {
            float c = (2.0f - 1.570796f);
            return 4.0f * (MainRect.width + MainRect.height) - c * (useIndependentCorners 
                ? (independentCorners.x + independentCorners.y + independentCorners.z + independentCorners.w)
                : corners);
        }
        public override bool HasDashedOutline => true;
    }
}