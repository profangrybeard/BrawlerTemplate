using System;
using UnityEngine;

namespace UIAB
{
    [CreateAssetMenu(fileName = "UIABStyle", menuName = "UIAB/Style Preset", order = 51)]
    public class UIABStylePreset : ScriptableObject
    {
        [SerializeField] private string uniqueId = "";
        [SerializeField] private UIABStyleData data;

        public UIABStyleData GetData() { return data; }

        public string Id { get { return uniqueId; } }

        public void SaveStyle(UIABColor fillColor, UIABOutline outline, UIABEffects effects, UIABExtraSettings extraSettings, Vector4 shapeData)
        {
            uniqueId = (uniqueId == "") ? Guid.NewGuid().ToString() : uniqueId;
            data = new UIABStyleData(fillColor, outline, effects, extraSettings, shapeData);

            ApplyToAll(uniqueId);
        }
        
        public void LoadStyle(ref UIABColor fillColor, ref UIABOutline outline, ref UIABEffects effects, ref UIABExtraSettings extraSettings, ref Vector4 shapeData)
        {
            fillColor = data.GetFillColor(); 
            outline = data.GetOutline();
            effects = data.GetEffects();
            extraSettings = data.GetExtraSettings();
            shapeData = data.GetShapeData();
        }
        
        public static void ApplyToAll(string uniqueId)
        {
            ShapeRenderer[] shapeRenderers = GetAllInstances();
            for (int i = 0; i < shapeRenderers.Length; i++)
            {
                shapeRenderers[i].ApplyStyleIfUsing(uniqueId);
            }
        }
        
        public static ShapeRenderer[] GetAllInstances()
        {
#if UNITY_2023
            return FindObjectsByType<ShapeRenderer>(FindObjectsSortMode.None);
#else
            return FindObjectsByType<ShapeRenderer>(FindObjectsSortMode.None);
#endif
        }

#if UNITY_EDITOR
        private void Reset()
        {
            uniqueId = Guid.NewGuid().ToString();
            data.Reset();
        }
#endif
    }
}