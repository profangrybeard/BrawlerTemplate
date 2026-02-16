using UnityEditor;
using UnityEngine;

namespace UIAB
{
    [CustomEditor(typeof(ShapeRenderer))]
    public class ShapeRendererEditor : ShaderRendererEditor
    {
        private string path = UIABEditorUtils.STYLES_PATH;

        private bool showAppearanceSection = true;
        private bool showStyleSection = false;

        private ShapeRenderer shapeRenderer = null;
        private SerializedProperty useCustomDimensions;
        private SerializedProperty dimension;
        private SerializedProperty fillColor;
        private SerializedProperty outline;
        private SerializedProperty effects;
        private SerializedProperty extraSettings;
        private SerializedProperty stylePreset;
        private SerializedProperty raycastTarget;
        private SerializedProperty maskable;

        private bool ShowOutlineSection;
        private bool ShowEffectsSection;
        private bool ShowExtraSettingsSection;

        private UIABColor color;
        private Vector3 fillColorGradientStart;
        private Vector3 fillColorGradientEnd;
        private Vector3 fillColorGradientMid;
        private int gradientStartControlId = -1;
        private int gradientEndControlId = -1;
        private int gradientMidControlId = -1;

        protected virtual void CustomAppearanceProperties(SerializedObject serializedObject) { }
        protected virtual Vector4 NewStyleShaderData() { return Vector4.zero; }

        public override void Init(SerializedObject serializedObject)
        {
            base.Init(serializedObject);

            shapeRenderer = (ShapeRenderer)target;
            useCustomDimensions = serializedObject.FindProperty("useCustomDimensions");
            dimension = serializedObject.FindProperty("dimension");
            fillColor = serializedObject.FindProperty("fillColor");
            outline = serializedObject.FindProperty("outline");
            effects = serializedObject.FindProperty("effects");
            extraSettings = serializedObject.FindProperty("extraSettings");
            stylePreset = serializedObject.FindProperty("stylePreset");
            raycastTarget = serializedObject.FindProperty("m_RaycastTarget");
            maskable = serializedObject.FindProperty("m_Maskable");

            gradientStartControlId = GUIUtility.GetControlID(FocusType.Passive);
            gradientEndControlId = GUIUtility.GetControlID(FocusType.Passive);
            gradientMidControlId = GUIUtility.GetControlID(FocusType.Passive);
        }
        
        public override void DrawProperties(SerializedObject serializedObject)
        {

            GUILayout.Space(UIABEditorUtils.SECTION_DEFAULT_SPACE);
            UIABEditorUtils.ShowOptions("Appearance", null, ref showAppearanceSection, () => { AppearanceSection(serializedObject); });
            UIABEditorUtils.DrawProperty(outline, "Outline", ref ShowOutlineSection, shapeRenderer.GetOutline(), outline.FindPropertyRelative("useOutline"), false, shapeRenderer.HasDashedOutline );
            UIABEditorUtils.DrawProperty(effects, "Effects", ref ShowEffectsSection, shapeRenderer.GetEffects(), effects.FindPropertyRelative("useEffects"));
            UIABEditorUtils.ShowOptions("Extra Settings", null, ref ShowExtraSettingsSection, () => { ExtraSettingsSection(); });
            UIABEditorUtils.ShowOptions("Style", null, ref showStyleSection, StyleSection);

            base.DrawProperties(serializedObject);
        }

        private void AppearanceSection(SerializedObject serializedObject)
        {
            EditorGUILayout.PropertyField(useCustomDimensions, new GUIContent("Use Custom Dimensions"));
            EditorGUI.BeginDisabledGroup(!useCustomDimensions.boolValue);
            EditorGUILayout.PropertyField(dimension);
            EditorGUI.EndDisabledGroup();
            UIABEditorUtils.AddSectionInnerSpace();
            CustomAppearanceProperties(serializedObject);
            UIABEditorUtils.DrawProperty(fillColor, "Fill Color", shapeRenderer.GetFillColorType(), true); //filter[0] = Show Gradient Options
        }
        
        private void StyleSection()
        {
            EditorGUILayout.PropertyField(stylePreset, new GUIContent("Style"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (shapeRenderer.GetStylePreset() != null)
            {
                if (GUILayout.Button("Apply Style"))
                {
                    shapeRenderer.ApplyCurrentStyle();
                }
                if (GUILayout.Button("Override Style"))
                {
                    shapeRenderer.SaveStyle();
                }
            }
            else
            {
                if (GUILayout.Button("Create Style"))
                {
                    path = EditorUtility.SaveFilePanel("Save New Style", Application.dataPath + path, Selection.activeGameObject.name + ".asset", "asset");
                    if (path.Length != 0)
                    {
                        UIABStylePreset newStyle = ScriptableObject.CreateInstance<UIABStylePreset>();
                        newStyle.SaveStyle(
                            shapeRenderer.GetFillColorType(),
                            shapeRenderer.GetOutline(),
                            shapeRenderer.GetEffects(),
                            shapeRenderer.GetExtraSettings(),
                            NewStyleShaderData());

                        UIABEditorUtils.SaveStyleToFile(newStyle, path);
                        shapeRenderer.SetStylePreset(newStyle);
                    }
                    else
                    {
                        path = UIABEditorUtils.STYLES_PATH;
                        EditorGUILayout.BeginVertical();
                    }
                    EditorGUILayout.BeginVertical(); //Solves a bug caused by the dialogue window showing mid editor inspector call
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        private void ExtraSettingsSection()
        {
            UIABEditorUtils.DrawProperty(extraSettings, "Extra Settings", shapeRenderer.GetExtraSettings());
            EditorGUILayout.PropertyField(raycastTarget, new GUIContent("Raycast Target"));
            EditorGUILayout.PropertyField(maskable, new GUIContent("Maskable"));
        }

        protected override void DrawSceneGUI(Event currentEvent)
        {
            base.DrawSceneGUI(currentEvent);

            color = shapeRenderer.GetFillColorType();
            FillStyle fillStyle = color.GetFillStyle();
            
            if (fillStyle == FillStyle.Solid) return;

            EditorGUI.BeginChangeCheck();
            bool is3Colors = fillStyle > FillStyle.Radial;
            Vector3 currentPosition = shapeRenderer.transform.position;
            Vector2 dimension = shapeRenderer.GetDimension();
            Color colorStart = color.GetColor();
            Color colorEnd = color.GetColorEnd();

            Vector3 gradientStart = color.GetGradientStartCoordinates() * shapeRenderer.GetDimension();
            Vector3 gradientEnd = color.GetGradientEndCoordinates() * shapeRenderer.GetDimension();

            float size = 0.2f * HandleUtility.GetHandleSize(gradientStart);

            const float GRADIENT_STEPS = 10f;
            Vector3 lineStart = fillColorGradientStart + currentPosition;
            Vector3 lineEnd = fillColorGradientEnd + currentPosition;
            Handles.color = Color.gray;
            Handles.DrawLine(lineStart, lineEnd, 16f);
            Handles.DotHandleCap(gradientStartControlId, gradientStart + currentPosition, Quaternion.identity, size, currentEvent.type);
            Handles.DotHandleCap(gradientEndControlId, gradientEnd + currentPosition, Quaternion.identity, size, currentEvent.type);
            Handles.color = colorStart;
            Handles.DotHandleCap(gradientStartControlId, gradientStart + currentPosition, Quaternion.identity, 0.8f * size, currentEvent.type);
            Handles.color = colorEnd;
            Handles.DotHandleCap(gradientEndControlId, gradientEnd + currentPosition, Quaternion.identity, 0.8f * size, currentEvent.type);

            fillColorGradientStart = Handles.DoPositionHandle(gradientStart + shapeRenderer.transform.position, Quaternion.identity) - shapeRenderer.transform.position;
            fillColorGradientEnd = Handles.DoPositionHandle(gradientEnd + currentPosition, Quaternion.identity) - currentPosition;

            float currentRatio = 0f;
            for (int i = 0; i < GRADIENT_STEPS; i++)
            {
                currentRatio = i / (GRADIENT_STEPS - 1f);
                if (is3Colors)
                {
                    if (currentRatio < color.GetGradientMid())
                    {
                        colorStart = color.GetColor();
                        colorEnd = color.GetColorMid();
                        currentRatio /= color.GetGradientMid();
                    }
                    else
                    {
                        colorStart = color.GetColorMid();
                        colorEnd = color.GetColorEnd();
                        currentRatio = (currentRatio - color.GetGradientMid()) / (1f - color.GetGradientMid());
                    }
                }
                Handles.color = Color.Lerp(colorStart, colorEnd, currentRatio);
                Handles.DrawLine(Vector3.Lerp(lineStart, lineEnd, i/GRADIENT_STEPS), Vector3.Lerp(lineStart, lineEnd, (i + 1) / GRADIENT_STEPS), 10f);
            }

            if (EditorGUI.EndChangeCheck())
            {
                color.SetGradientCoordinates(fillColorGradientStart / dimension, fillColorGradientEnd / dimension);
                shapeRenderer.SetFillColor(color);
                shapeRenderer.RenderShader();
                HandleUtility.Repaint();
            }
            else if (is3Colors)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 gradientMid = Vector3.Lerp(gradientStart, gradientEnd, color.GetGradientMid());

                Handles.color = Color.gray;
                Handles.DotHandleCap(gradientMidControlId, gradientMid + currentPosition, Quaternion.identity, size, currentEvent.type);
                Handles.color = color.GetColorMid();
                Handles.DotHandleCap(gradientMidControlId, gradientMid + currentPosition, Quaternion.identity, 0.8f * size, currentEvent.type);
                fillColorGradientMid = Handles.DoPositionHandle(gradientMid + currentPosition, Quaternion.identity) - currentPosition;

                if (EditorGUI.EndChangeCheck())
                {
                    Vector3 gradientVector = fillColorGradientEnd - fillColorGradientStart;
                    color.SetGradientMid(Vector3.Dot(gradientVector, fillColorGradientMid - fillColorGradientStart) / Vector3.Dot(gradientVector, gradientVector));
                    shapeRenderer.SetFillColor(color);
                    shapeRenderer.RenderShader();
                    HandleUtility.Repaint();
                }
            }
        }
    }
}