using System;
using UnityEngine;

namespace UIAB
{
    public class UIABMore : ShapeRenderer
    {
        // More
        // Send More-Shapes specific data to the shader
        // For more details on how it is drawn check Shader/CGIncludes/Shapes/More.cginc
        // If you wish to change the shape visual in code,
        // use the Getter/Setter and then call RenderShader()

        private const string CLASS_NAME = "More";
        private const string DATA_PREFIX = "_" + CLASS_NAME;

        private const float HEART_SIZE_MOD = 0.25f + 1.41421356237f;
        private const float EGG_SIZE_MOD = 1.73205080757f - 1f;

        [SerializeField] private MoreShapeType shapeType = MoreShapeType.Heart;
        private Vector4 shaderData;
        private float size;
        // Moon
        [SerializeField] [Range(-1f,1f)] private float moonD = 0.125f;
        [SerializeField] private float moonRb = 90f;
        // Egg
        [SerializeField] [Range(0f, 1f)] private float eggRb = 0.25f;
        // Cross
        [SerializeField] private float crossB2 = 12f;
        [SerializeField] [Range(-1f, 1f)] private float crossR = 0f;
        // X
        [SerializeField] private float xR = 8f;
        // Checkmark
        [SerializeField] private float checkmarkR = 8f;
        private float checkmarkH;

        #region GET/SET
        public MoreShapeType GetShapeType() { return shapeType; }
        #endregion

        // Process shader data before sending to the shader
        protected override void UpdateClassProperties()
        {
            base.UpdateClassProperties();

            size = Mathf.Min(MainRect.width, MainRect.height);
            switch (shapeType)
            {
                case MoreShapeType.Heart:
                    shaderData = new Vector4(0, 1f / (size * HEART_SIZE_MOD), size, size * HEART_SIZE_MOD);
                    break;
                case MoreShapeType.Moon:
                    shaderData = new Vector4(1f, (moonD == 0 ? 0.001f : moonD) * (size + moonRb), size, moonRb);
                    break;
                case MoreShapeType.Egg:
                    size = Mathf.Min(MainRect.width, MainRect.height * EGG_SIZE_MOD);
                    eggRb = Mathf.Max(0, eggRb);
                    shaderData = new Vector4(2, size, eggRb * size / EGG_SIZE_MOD, 0.5f * EGG_SIZE_MOD * size);
                    break;
                case MoreShapeType.Cross:
                    crossB2 = Mathf.Max(crossB2, 0);
                    shaderData = new Vector4(3, (1f + crossR) * size, Mathf.Clamp(crossR * size + crossB2, 0, size), crossR * size);
                    break;
                case MoreShapeType.X:
                    xR = Mathf.Max(xR, 0);
                    shaderData = new Vector4(4, 2f * (size - xR), xR, 0);
                    break;
                case MoreShapeType.Checkmark:
                    checkmarkR = Mathf.Max(checkmarkR, 0);
                    checkmarkH = -Mathf.Min((MainRect.width - 0.5f * checkmarkR) / 3f, (MainRect.height - 0.5f * checkmarkR) / 2f);
                    shaderData = new Vector4(5, checkmarkH, checkmarkR, 0);
                    break;
            }
            shaderData.x = (int)shapeType;
        }

        // This sets the data into the shader. Any new variable used by the shader must be sent to it beforehand.
        // SetShaderProperty will handle whether send to the Compute shader or the Vertex
        protected override void SetShaderData()
        {
            base.SetShaderData();

            SetShaderProperty(DATA_PREFIX + "_Type_Data", shaderData);
        }

        protected override string GetKernelID()
        {
            return "DrawMore";
        }
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            shapeType = MoreShapeType.Heart;
            moonD = 0.125f;
            moonRb = 90f;
            eggRb = 0.25f;
            crossB2 = 12f;
            crossR = 0f;
            xR = 8f;
            checkmarkR = 8f;
        }
#endif
        protected override float GetPerimeter()
        {
            //TBD
            return 1f;
        }
    }

    [Serializable]
    public enum MoreShapeType { Heart = 0, Moon = 1, Egg = 2, Cross = 3, X = 4, Checkmark = 5 }
}