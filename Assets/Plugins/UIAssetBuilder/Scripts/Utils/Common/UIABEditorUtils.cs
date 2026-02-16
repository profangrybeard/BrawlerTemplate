using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UIAB
{
    public static class UIABEditorUtils
    {
#if UNITY_EDITOR
        public static readonly string EXPORTS_PATH = "/Plugins/UIAssetBuilder/Exports/";
        public static readonly string STYLES_PATH = "/Plugins/UIAssetBuilder/Styles/";

        public const int BUTTON_HEIGHT = 32;
        public const int SECTION_DEFAULT_SPACE = 8;
        private const int HEADER_HEIGHT = 25;
        private const int SUBHEADER_HEIGHT = 20;
        private const int SECTION_HEADER_LABEL_GAP = 29;
        private const int SECTION_DEFAULT_VERTICAL_PADDING = SECTION_DEFAULT_SPACE / 2;

        #region INSPECTOR_UTILS
        public static void DrawProperty(SerializedProperty property, string label, ICustomInspector drawer, params bool[] filter)
        {
            if (property != null)
            {
                //ICustomInspector drawer = null;// property.boxedValue as ICustomInspector;
                if (drawer != null)
                {
                    drawer.DrawProperties(property, label, filter);
                }
                else
                {
                    if (label == "")
                        EditorGUILayout.PropertyField(property);
                    else
                        EditorGUILayout.PropertyField(property, new GUIContent(label));
                }
            }
            else
            {
                Debug.LogError($"Property {label} no found");
            }
        }
        public static void DrawProperty(SerializedProperty property, string title, ref bool showSection, ICustomInspector drawer, SerializedProperty toggle = null, bool isSubheader = false, params bool[] filter)
        {
            ShowOptions(title, toggle, ref showSection, () => { DrawProperty(property, "", drawer, filter); }, isSubheader);
        }
        public static void ShowOptions(string title, SerializedProperty toggleState, SerializedProperty showSection, Action showProperties, bool isSubheader = false)
        {
            ClickableSectionHeader(title, toggleState, showSection, isSubheader);

            if (showSection.boolValue)
            {
                if (toggleState != null) EditorGUI.BeginDisabledGroup(!toggleState.boolValue);
                EditorGUILayout.BeginVertical(Styles.Instance.sectionLayoutStyle);
                showProperties.Invoke();
                EditorGUILayout.EndVertical();
                //GUILayout.Space(SECTION_DEFAULT_VERTICAL_PADDING);
                if (toggleState != null) EditorGUI.EndDisabledGroup();
            }
        }
        public static void ShowOptions(string title, SerializedProperty toggleState, ref bool showSection, Action showProperties, bool isSubheader = false)
        {
            ClickableSectionHeader(title, toggleState, ref showSection, isSubheader);

            if (showSection)
            {
                if (toggleState != null) EditorGUI.BeginDisabledGroup(!toggleState.boolValue);
                EditorGUILayout.BeginVertical(Styles.Instance.sectionLayoutStyle);
                showProperties.Invoke();
                EditorGUILayout.EndVertical();
                //GUILayout.Space(SECTION_DEFAULT_VERTICAL_PADDING);
                if (toggleState != null) EditorGUI.EndDisabledGroup();
            }
        }
        private static void ClickableSectionHeader(string title, SerializedProperty toggleState, SerializedProperty showSection, bool isSubheader = false)
        {
            ClickableSection(title, toggleState, showSection, isSubheader ? SUBHEADER_HEIGHT : HEADER_HEIGHT, isSubheader);
        }
        private static void ClickableSectionHeader(string title, SerializedProperty toggleState, ref bool showSection, bool isSubheader = false)
        {
            ClickableSection(title, toggleState, ref showSection, isSubheader ? SUBHEADER_HEIGHT : HEADER_HEIGHT, isSubheader);
        }
        private static void ClickableSection(string title, SerializedProperty toggleState, SerializedProperty showSection, int height, bool isSubheader = false)
        {
            EditorGUILayout.BeginHorizontal(isSubheader ? Styles.Instance.sectionSubheaderLayoutStyle : Styles.Instance.sectionHeaderLayoutStyle);
            if (toggleState != null) toggleState.boolValue = EditorGUILayout.Toggle(toggleState.boolValue, Styles.Instance.leftCenterToggleStyle, GUILayout.Width(height), GUILayout.Height(height));
            else GUILayout.Space(SECTION_HEADER_LABEL_GAP);
            if (GUILayout.Button(title, Styles.Instance.cetralizedLabelStyle, GUILayout.Height(height))) { showSection.boolValue = !showSection.boolValue; }
            EditorGUILayout.EndHorizontal();
        }
        private static void ClickableSection(string title, SerializedProperty toggleState, ref bool showSection, int height, bool isSubheader = false)
        {
            EditorGUILayout.BeginHorizontal(isSubheader ? Styles.Instance.sectionSubheaderLayoutStyle : Styles.Instance.sectionHeaderLayoutStyle);
            if (toggleState != null) toggleState.boolValue = EditorGUILayout.Toggle(toggleState.boolValue, Styles.Instance.leftCenterToggleStyle, GUILayout.Width(height), GUILayout.Height(height));
            else GUILayout.Space(SECTION_HEADER_LABEL_GAP);
            if (GUILayout.Button(title, Styles.Instance.cetralizedLabelStyle, GUILayout.Height(height))) { showSection = !showSection; }
            EditorGUILayout.EndHorizontal();
        }
        public static void AddSectionInnerSpace() 
        {
            GUILayout.Space(2 * SECTION_DEFAULT_VERTICAL_PADDING);
        }
        #endregion
        #region SAVE_FILE
        public static string SaveRenderTextureToFile(RenderTexture renderTexture, string path)
        {
            Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBAFloat, false, true); ;
            var oldRt = RenderTexture.active;
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();
            RenderTexture.active = oldRt;

            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();
            string file = "Assets" + path.Split(Application.dataPath)[1];
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(file);
            EditorGUIUtility.PingObject(Selection.activeObject);
            return file;
        }
        public static void SaveStyleToFile(UIABStylePreset stylePreset, string path)
        {
            string relativePath = "Assets" + path.Split(Application.dataPath)[1];
            AssetDatabase.CreateAsset(stylePreset, relativePath);

            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(relativePath); ;
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        #endregion
        #region FIND_FILE
        public static string GetAssetPath(string assetStaticPath)
        {
            List<string> foundPaths = new List<string>();
            var allAssetPaths = AssetDatabase.GetAllAssetPaths();
            var fileName = assetStaticPath;
            for (int i = 0; i < allAssetPaths.Length; ++i)
            {
                if (allAssetPaths[i].EndsWith(fileName))
                    foundPaths.Add(allAssetPaths[i]);
            }

            if (foundPaths.Count == 1)
                return foundPaths[0];

            if (foundPaths.Count == 0)
            {
                Debug.LogError($"No path found for asset {assetStaticPath}!");
            }
            else if (foundPaths.Count > 1)
            {
                Debug.LogError($"Multiple paths found for asset {assetStaticPath}, use more precise static path!");

                for (int i = 0; i < foundPaths.Count; i++)
                {
                    string path = foundPaths[i];
                    Debug.LogError($"Path {i + 1}: {path}");
                }
            }

            return null;
        }
        #endregion

#endif
    }

        public sealed class Styles
    {
        private static Styles instance = null;
        private static readonly object padlock = new object();
        public static Styles Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Styles();
                    }
                    return instance;
                }
            }
        }

        #region Styles
        public GUIStyle leftCenterToggleStyle { get; private set; }
        public GUIStyle cetralizedLabelStyle { get; private set; }
        public GUIStyle sectionLayoutStyle { get; private set; }
        public GUIStyle subSectionLayoutStyle { get; private set; }
        public GUIStyle sectionHeaderLayoutStyle { get; private set; }
        public GUIStyle sectionSubheaderLayoutStyle { get; private set; }
        public GUIStyle simpleSectionHeaderLayoutStyle { get; private set; }
        #endregion

        private Styles()
        {
            leftCenterToggleStyle = GUI.skin.GetStyle("Toggle");
            leftCenterToggleStyle.alignment = TextAnchor.MiddleLeft;
            leftCenterToggleStyle.stretchHeight = true;
            cetralizedLabelStyle = GUI.skin.GetStyle("BoldLabel");
            cetralizedLabelStyle.alignment = TextAnchor.MiddleLeft;
            sectionLayoutStyle = GUI.skin.GetStyle("Box");
            sectionLayoutStyle.padding = new RectOffset(10, 10, 0, 0);
            subSectionLayoutStyle = GUI.skin.GetStyle("TextArea");
            subSectionLayoutStyle.padding = new RectOffset(10, 10, 0, 0);
            subSectionLayoutStyle.border = new RectOffset(0, 0, 0, 0);
            subSectionLayoutStyle.onHover.background = null;
            subSectionLayoutStyle.hover.background = null;
            sectionHeaderLayoutStyle = GUI.skin.GetStyle("Button");
            sectionSubheaderLayoutStyle = GUI.skin.GetStyle("TextField");
            simpleSectionHeaderLayoutStyle = GUI.skin.GetStyle("Box");
        }
    }
}