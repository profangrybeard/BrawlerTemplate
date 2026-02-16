using System;
using UnityEngine;

namespace UIAB
{
    public class UIABProgress : ShapeRenderer
    {
        // Progress
        // Send Progress specific data to the shader
        // For more details on how it is drawn check Shader/CGIncludes/Shapes/Progress.cginc
        // If you wish to change the shape visual in code,
        // use the Getter/Setter and then call RenderShader()

        private const string CLASS_NAME = "Progress";
        private const string DATA_PREFIX = "_" + CLASS_NAME;

        [SerializeField] private ShapeType shape;
        [SerializeField] [Range(0,1)] private float progress = 1f;
        [SerializeField] private float thickness = 4f;

        private int currentType = 0;
        private float currentProgress = 0f;
        private float currentSize = 0f;
        private float currentThickness = 0f;

        #region GET/SET
        public void SetShapeType(ShapeType shape) { this.shape = shape; }
        public ShapeType GetShapeType() { return shape; }
        public void SetProgressRatio(float ratio) { progress = ratio; }
        public float GetProgressRatio() { return progress; }
        public void SetThickness(float thickness) { this.thickness = thickness; }
        public float GetThickness() { return thickness; }
        #endregion

        // Process shader data before sending to the shader
        protected override void UpdateClassProperties()
        {
            base.UpdateClassProperties();

            thickness = Mathf.Max(0f, thickness);

            currentType = progress == 0f ? -1 : (int)shape;
            if (progress == 1f && shape == ShapeType.Ring) currentType = (int)ShapeType.Arc;
            currentProgress = (float)(Math.PI * progress);
            currentSize = Math.Min(MainRect.width, MainRect.height) - (currentType == (int)ShapeType.Pie ? 0f : thickness);
            currentThickness = thickness * (currentType == (int)ShapeType.Ring ? 2f : 1f);
        }

        // This sets the data into the shader. Any new variable used by the shader must be sent to it beforehand.
        // SetShaderProperty will handle whether send to the Compute shader or the Vertex
        protected override void SetShaderData()
        {
            base.SetShaderData();

            SetShaderProperty(DATA_PREFIX + "_Type_Size_Thickness_Ratio", new Vector4(currentType, currentSize, currentThickness, progress));
            SetShaderProperty(DATA_PREFIX + "_Progress", new Vector4(Mathf.Sin(currentProgress), Mathf.Cos(currentProgress), Mathf.Cos(-currentProgress), Mathf.Sin(-currentProgress)));
        }

        protected override string GetKernelID()
        {
            return "DrawProgress";
        }
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            shape = ShapeType.Arc;
            progress = 1f;
            thickness = 4f;
        }
#endif
        protected override float GetPerimeter()
        {
            return 2f * Mathf.PI * currentSize;
        }
    }

    [Serializable]
    public enum ShapeType { Arc = 0, Ring = 1, Pie = 2 }
}