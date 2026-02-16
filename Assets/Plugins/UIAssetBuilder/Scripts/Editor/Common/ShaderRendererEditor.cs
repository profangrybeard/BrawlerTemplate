using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIAB
{
    [CustomEditor(typeof(ShaderRenderer))]
    public class ShaderRendererEditor : DynamicTextureRendererEditor
    {
        private bool showRasterizeSection = false;
        private string path = UIABEditorUtils.EXPORTS_PATH;

        private ShaderRenderer shaderRenderer;
        private SerializedProperty sharpness;
        private SerializedProperty resolution;
        private SerializedProperty useComputeShader;

        private bool createImageAsChild = false;

        public override void Init(SerializedObject serializedObject)
        {
            base.Init(serializedObject);

            shaderRenderer = (ShaderRenderer)target;

            sharpness = serializedObject.FindProperty("sharpness");
            resolution = serializedObject.FindProperty("resolution");
            useComputeShader = serializedObject.FindProperty("useComputeShader");
        }
        
        public override void DrawProperties(SerializedObject serializedObject)
        {
            base.DrawProperties(serializedObject);

            UIABEditorUtils.ShowOptions("Rasterize & Export", useComputeShader, ref showRasterizeSection, RasterizeSection);
        }
        
        private void RasterizeSection()
        {
            EditorGUILayout.PropertyField(sharpness);
            EditorGUILayout.PropertyField(resolution);
            createImageAsChild = EditorGUILayout.Toggle(new GUIContent("Create Image as Child"), createImageAsChild);
            UIABEditorUtils.AddSectionInnerSpace();
            Vector2Int res = shaderRenderer.TextureDimension;
            if (GUILayout.Button(new GUIContent($"Export PNG ({res.x}x{res.y})"), GUILayout.Height(UIABEditorUtils.BUTTON_HEIGHT)))
            {
                path = EditorUtility.SaveFilePanel("Save texture as PNG", Application.dataPath + path, Selection.activeGameObject.name + ".png", "png");
                if (path.Length != 0)
                {
                    string file = UIABEditorUtils.SaveRenderTextureToFile(shaderRenderer.RenderTexture, path);
                    if (createImageAsChild) CreateImage(file);
                }
                else
                {
                    path = UIABEditorUtils.EXPORTS_PATH;
                }
                EditorGUILayout.BeginVertical(); //Solves a bug caused by the dialogue window showing mid editor inspector call
            }
            GUILayout.Space(UIABEditorUtils.SECTION_DEFAULT_SPACE);
        }

        private void CreateImage(string file)
        {
            GameObject imageObj = new GameObject("Image");
            imageObj.transform.parent = shaderRenderer.transform;

            RectTransform rect = imageObj.AddComponent<RectTransform>();

            rect.anchorMin = shaderRenderer.rectTransform.anchorMin;
            rect.anchorMax = shaderRenderer.rectTransform.anchorMax;
            rect.sizeDelta = (Vector2)shaderRenderer.TextureDimension / shaderRenderer.GetResolution();
            rect.pivot = shaderRenderer.rectTransform.pivot;
            rect.localPosition = Vector2.zero;

            Image image = imageObj.AddComponent<Image>();
            image.sprite = (Sprite)AssetDatabase.LoadAssetAtPath(file, typeof(Sprite));
        }
    }
}