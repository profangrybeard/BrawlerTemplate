namespace UIAB
{
    public interface IShaderProperty
    {
        public abstract void SetProperties(ShaderRenderer shaderRenderer, string prefix = "");
    }
}