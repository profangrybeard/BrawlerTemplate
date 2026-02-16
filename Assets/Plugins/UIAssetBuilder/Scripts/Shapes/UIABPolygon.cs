using UnityEngine;

namespace UIAB
{
    public class UIABPolygon : ShapeRenderer
    {
        // Polygon
        // Send Polygon specific data to the shader
        // For more details on how it is drawn check Shader/CGIncludes/Shapes/Polygon.cginc
        // If you wish to change the shape visual in code,
        // use the Getter/Setter and then call RenderShader()

        private const string CLASS_NAME = "Polygon";
        private const string DATA_PREFIX = "_" + CLASS_NAME;

        private const float MIN_ANGLE = 2f;

        [SerializeField][Range(3, 20)] private int sides = 5;
        [SerializeField][Range(0, 0.999f)] private float angle = 0f;
        [SerializeField] private float roundness = 0f;

        private float size = 1f;
        private float offsetMod = 1f;
        private float m = 0f;
        private float an = 0f;
        private float en = 0f;

        #region GET/SET
        public void SetSides(int sides) { this.sides = sides; }
        public int GetSides() { return sides; }
        public void SetAngle(float angle) { this.angle = angle; }
        public float GetAngle() { return angle; }
        public void SetCorners(float roundness) { this.roundness = roundness; }
        public float GetCorners() { return roundness; }
        #endregion
        
        // Process shader data before sending to the shader
        protected override void UpdateClassProperties()
        {
            base.UpdateClassProperties();

            m = MIN_ANGLE + angle * (sides - MIN_ANGLE);
            an = Mathf.PI / sides;
            en = Mathf.PI / m;

            float sideAngle = Mathf.PI * (sides - 2) / sides;
            offsetMod = 1 / Mathf.Sin(sideAngle / 2);

            size = Mathf.Min(MainRect.width, MainRect.height);

            roundness = Mathf.Clamp(roundness, 0f, size);
        }

        // This sets the data into the shader. Any new variable used by the shader must be sent to it beforehand.
        // SetShaderProperty will handle whether send to the Compute shader or the Vertex
        protected override void SetShaderData()
        {
            base.SetShaderData();

            SetShaderProperty(DATA_PREFIX + "_Size_Roundness_InnerAngle_OffsetMod", new Vector4(size, roundness, an, offsetMod));
            SetShaderProperty(DATA_PREFIX + "_Helper_Data", new Vector4(Mathf.Cos(an), Mathf.Sin(an), Mathf.Cos(en), Mathf.Sin(en)));
        }
        
        // Actual distance from angled corners
        protected override Vector4 GetOutlineOffset(float offset)
        {
            float angle = Mathf.PI * (sides - 2) / sides;
            offset *= 1 / Mathf.Sin(angle / 2);
            return base.GetOutlineOffset(offset);
        }
        protected override void ApplyShapeStyle(Vector4 shapeData)
        {
            roundness = shapeData.x;
        }
        protected override Vector4 SaveShapeStyle()
        {
            return Vector4.one * roundness;
        }
        protected override string GetKernelID()
        {
            return "DrawPolygon";
        }
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            sides = 5;
            angle = 0f;
            roundness = 0f;
        }
#endif
        protected override float GetPerimeter()
        {
            //TBD
            return 1f;
        }
    }
}