using UnityEngine;

namespace UIAB
{
    public class UIABCircle : ShapeRenderer
    {
        // Circle
        // Send circle specific data to the shader
        // For more details on how it is drawn check Shader/CGIncludes/Shapes/Circle.cginc
        // If you wish to change the shape visual in code,
        // use the Getter/Setter and then call RenderShader()

        private const string CLASS_NAME = "Circle";
        private const string DATA_PREFIX = "_" + CLASS_NAME;

        // This sets the data into the shader. Any new variable used by the shader must be sent to it beforehand.
        // SetShaderProperty will handle whether send to the Compute shader or the Vertex
        protected override void SetShaderData()
        {
            base.SetShaderData();

            SetShaderProperty(DATA_PREFIX + "_Dimension_Type", new Vector4(MainRect.width, MainRect.height, Mathf.Abs(MainRect.width - MainRect.height) < 0.01f ? 1 : 0));
        }

        protected override string GetKernelID()
        {
            return "DrawCircle";
        }
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
        }
#endif
        protected override float GetPerimeter()
        {
            float a = MainRect.width;
            float b = MainRect.height;
            float h = Mathf.Pow((a - b), 2) / Mathf.Pow((a + b), 2);
            return Mathf.PI * (a + b) * (1 + (3 * h) / (10 + Mathf.Sqrt(4 - 3 * h)));
        }
        public override bool HasDashedOutline => true;
    }
}