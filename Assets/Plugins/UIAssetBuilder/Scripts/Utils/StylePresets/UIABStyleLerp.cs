using UnityEngine;

namespace UIAB
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ShapeRenderer))]
    public class UIABStyleLerp : MonoBehaviour
    {
        [SerializeField] [Range(0f,1f)] float lerp;
        [SerializeField] UIABStylePreset startStyle;
        [SerializeField] UIABStyleData startData;
        [SerializeField] UIABStylePreset finalStyle;
        [SerializeField] UIABStyleData finalData;

        private UIABStyleData currentStyleData;

        private float previousLerp = -1f;
        private ShapeRenderer shapeRenderer;

        private UIABStyleData StartData { get { return startStyle == null ? startData : startStyle.GetData(); } }
        private UIABStyleData FinalData { get { return finalStyle == null ? finalData : finalStyle.GetData(); } }
        public UIABStyleData CustomStartData { get { return startData; } }
        public UIABStyleData CustomFinalData { get { return finalData; } }

        private void Start()
        {
            Lerp(lerp);
        }
        private void Update()
        {
            Lerp(lerp);
        }
        public void Lerp(float lerp, bool forceUpdate = false)
        {
            if (shapeRenderer == null) { shapeRenderer = GetComponent<ShapeRenderer>(); }

            if (previousLerp == lerp && !forceUpdate) return;
            previousLerp = lerp;

            currentStyleData.PersistentLerp(StartData, FinalData, lerp);
            shapeRenderer.SetFillColor(currentStyleData.GetFillColor());
            shapeRenderer.SetOutline(currentStyleData.GetOutline());
            shapeRenderer.SetEffects(currentStyleData.GetEffects());
            shapeRenderer.SetExtraSettings(currentStyleData.GetExtraSettings());
            shapeRenderer.RenderShader();
        }
        
        private void OnEnable()
        {
            shapeRenderer = GetComponent<ShapeRenderer>();
        }

        private void OnValidate()
        {
            Lerp(lerp, true);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            lerp = 0;
            startStyle = null;
            startData.Reset();
            finalStyle = null;
            finalData.Reset();
        }
#endif
    }
}