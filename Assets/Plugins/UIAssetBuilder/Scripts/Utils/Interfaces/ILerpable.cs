namespace UIAB
{
    public interface ILerpable<T>
    {
        public abstract T Lerp(T target, float lerp);
    }
}