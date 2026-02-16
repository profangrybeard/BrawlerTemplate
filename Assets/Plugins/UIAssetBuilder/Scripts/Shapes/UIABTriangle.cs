using UnityEngine;

namespace UIAB
{
    public class UIABTriangle : ShapeRenderer
    {
        // Triangle
        // Send Triangle specific data to the shader
        // For more details on how it is drawn check Shader/CGIncludes/Triangle/Rectangle.cginc
        // If you wish to change the shape visual in code,
        // use the Getter/Setter and then call RenderShader()

        private const string CLASS_NAME = "Triangle";
        private const string DATA_PREFIX = "_" + CLASS_NAME;

        [SerializeField] private bool forceEquilateral = false;
        [SerializeField] private float corners = 0;

        private float actualCorners;
        private float cornersMax;

        #region GET/SET
        public void SetCorners(float corners) { this.corners = corners; }
        public float GetCorners() { return corners; }
        public void SetEquilateral(bool forceEquilateral) { this.forceEquilateral = forceEquilateral; }
        public bool IsEquilateral() { return forceEquilateral; }
        #endregion

        // Process shader data before sending to the shader
        protected override void UpdateClassProperties()
        {
            base.UpdateClassProperties();

            if (forceEquilateral)
            {
                cornersMax = MainRect.width;
            }
            else
            {
                cornersMax = Mathf.Min(MainRect.width, MainRect.height - 0.01f);
            }

            corners = Mathf.Max(corners, 0);
            actualCorners = Mathf.Min(corners, cornersMax);
        }

        // This sets the data into the shader. Any new variable used by the shader must be sent to it beforehand.
        // SetShaderProperty will handle whether send to the Compute shader or the Vertex
        protected override void SetShaderData()
        {
            base.SetShaderData();

            SetShaderProperty(DATA_PREFIX + "_Dimension_Corner_Type", new Vector4(MainRect.width, -2f * MainRect.height, actualCorners, forceEquilateral ? 1 : 0));
        }

        // Actual distance from angled corners
        protected override Vector4 GetOutlineOffset(float offset)
        {
            if (forceEquilateral)
            {
                if (!UseRoundEdge)
                    offset *= Mathf.Sqrt(3f);

                float heightAdjust = 0.5f * MainRect.height - MainRect.width / Mathf.Sqrt(3f);
                return Vector4.one * offset - new Vector4(0, heightAdjust, 0, heightAdjust);
            }
            else
            {
                if (UseRoundEdge)
                    return base.GetOutlineOffset(offset);

                float h = new Vector2(0.5f * MainRect.width, MainRect.height).magnitude;
                float wOffset = offset * (h + 0.5f * MainRect.width) / MainRect.height;
                float hOffset = 2.0f * wOffset * MainRect.height / MainRect.width - offset;

                return new Vector4(wOffset, hOffset, wOffset, hOffset);
            }
        }
        protected override void ApplyShapeStyle(Vector4 shapeData)
        {
            corners = shapeData.x;
        }
        protected override Vector4 SaveShapeStyle()
        {
            return Vector4.one * corners;
        }
        protected override string GetKernelID()
        {
            return "DrawTriangle";
        }
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            forceEquilateral = false;
            corners = 0;
        }
#endif
        protected override float GetPerimeter()
        {
            // Triangle outline in development...
            return 1f;
        }
    }
}