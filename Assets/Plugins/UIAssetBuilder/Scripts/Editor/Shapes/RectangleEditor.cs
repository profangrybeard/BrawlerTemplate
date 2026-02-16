using UnityEditor;
using UnityEngine;

namespace UIAB
{
    [CustomEditor(typeof(UIABRectangle)), CanEditMultipleObjects]
    public class RectangleEditor : ShapeRendererEditor
    {
        private const float HANDLE_SIZE = 0.1f;
        private const float HANDLE_OFFSET = 0.2f;
        private const float CORNER_SENSITIVITY = 0.2f;
        private const float HANDLES_THRESHOLD = 1f;

        // SCENE
        private RectTransform rectTransform;
        private int selectedId = -1;
        private bool isDragging;
        private float handleSize;
        private float handleSizeModifier;
        private float cornerStartValue;
        private float canvasScale;
        private Vector4 independentCornersStartValue;
        private Vector3 dragStartPosition;
        private Vector3 dragCurrentPosition;
        private int tlControlId = -1;
        private int trControlId = -1;
        private int blControlId = -1;
        private int brControlId = -1;
        // COMMON
        private UIABRectangle rectangle;
        private SerializedProperty corners;
        private SerializedProperty independentCorners;
        private SerializedProperty useIndependentCorners;
        private void Awake()
        {

        }

        private void OnEnable()
        {
            rectangle = (UIABRectangle) target;
            rectTransform = rectangle.rectTransform;
            corners = serializedObject.FindProperty("corners");
            independentCorners = serializedObject.FindProperty("independentCorners");
            useIndependentCorners = serializedObject.FindProperty("useIndependentCorners");

            tlControlId = GUIUtility.GetControlID(FocusType.Passive);
            trControlId = GUIUtility.GetControlID(FocusType.Passive);
            blControlId = GUIUtility.GetControlID(FocusType.Passive);
            brControlId = GUIUtility.GetControlID(FocusType.Passive);

            Init(serializedObject);
        }
        #region INSPECTOR
        protected override void CustomAppearanceProperties(SerializedObject serializedObject)
        {
            base.CustomAppearanceProperties(serializedObject);
            Vector4 cornersValue = independentCorners.vector4Value;
            bool useIndependentCornersValue = useIndependentCorners.boolValue;
            EditorGUILayout.BeginHorizontal();
            if (useIndependentCornersValue)
            {
                GUILayout.Label("Corners", GUILayout.Width(EditorGUIUtility.labelWidth));
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField(corners.floatValue);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUILayout.PropertyField(corners, new GUIContent("Corners"));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button(new GUIContent(useIndependentCornersValue ? "Independent" : "Uniform"), GUILayout.Width(100f), GUILayout.ExpandHeight(true)))
            {
                useIndependentCorners.boolValue = !useIndependentCornersValue;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginDisabledGroup(!useIndependentCornersValue);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            cornersValue.z = EditorGUILayout.FloatField(cornersValue.z);
            cornersValue.x = EditorGUILayout.FloatField(cornersValue.x);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            cornersValue.w = EditorGUILayout.FloatField(cornersValue.w);
            cornersValue.y = EditorGUILayout.FloatField(cornersValue.y);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            independentCorners.vector4Value = cornersValue;
            UIABEditorUtils.AddSectionInnerSpace();
        }
        protected override Vector4 NewStyleShaderData()
        {
            if (useIndependentCorners.boolValue)
                return independentCorners.vector4Value;
            else
                return corners.floatValue * Vector4.one;
        }
        #endregion
        #region SCENE_GUI
        private void OnSceneGUI()
        {
            currentEvent = Event.current;

            base.DrawSceneGUI(currentEvent);

            if (Selection.objects.Length > 1) return;

            //DRAW HANDLERS
            if (currentEvent.type == EventType.Repaint || currentEvent.type == EventType.Layout)
            {
                Vector2 center = (Vector2)rectangle.transform.position + rectangle.MainRect.size * (Vector2.one * 0.5f - rectTransform.pivot);
                handleSizeModifier = HandleUtility.GetHandleSize(center);

                canvasScale = rectangle.transform.lossyScale.x;

                if (handleSizeModifier > canvasScale * HANDLES_THRESHOLD * Mathf.Min(rectangle.MainRect.width, rectangle.MainRect.height)) return; //based on dimension
                
                handleSize = HANDLE_SIZE * handleSizeModifier;

                DrawHandleRepaint(tlControlId, GetPositionById(tlControlId, center, rectangle.MainRect, HANDLE_OFFSET * handleSizeModifier * Vector2.one, canvasScale), handleSize, currentEvent.type);
                DrawHandleRepaint(trControlId, GetPositionById(trControlId, center, rectangle.MainRect, HANDLE_OFFSET * handleSizeModifier * Vector2.one, canvasScale), handleSize, currentEvent.type);
                DrawHandleRepaint(blControlId, GetPositionById(blControlId, center, rectangle.MainRect, HANDLE_OFFSET * handleSizeModifier * Vector2.one, canvasScale), handleSize, currentEvent.type);
                DrawHandleRepaint(brControlId, GetPositionById(brControlId, center, rectangle.MainRect, HANDLE_OFFSET * handleSizeModifier * Vector2.one, canvasScale), handleSize, currentEvent.type);

                if (isDragging && currentEvent.type == EventType.Repaint)
                {
                    if (IsCornerHandleId(selectedId))
                    {
                        float diff = Vector2.Dot(dragCurrentPosition - dragStartPosition, (center - (Vector2)dragStartPosition).normalized);
                        UpdateCornerValues(selectedId, diff * CORNER_SENSITIVITY * handleSizeModifier / canvasScale);

                        ((UIABRectangle)target).SetCorners(corners.floatValue, independentCorners.vector4Value);
                        ((UIABRectangle)target).RenderShader();
                    }
                    HandleUtility.Repaint();
                }
            }
            //ON MOUSE DOWN: CLICKING ON HANDLERS
            else if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                selectedId = HandleUtility.nearestControl;
                if (IsCornerHandleId(selectedId))
                {
                    independentCornersStartValue = independentCorners.vector4Value;
                    cornerStartValue = corners.floatValue;
                    dragStartPosition = GetMousePosition(currentEvent.mousePosition);
                    Event.current.Use();
                    HandleUtility.Repaint();
                }
            }
            //ON MOUSE DRAG: IF CLICKED ON A HANDLER, LOOK FOR TARGETS ON DRAG POSITION
            else if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 0)
            {
                if (IsCornerHandleId(selectedId))
                {
                    isDragging = true;
                    dragCurrentPosition = GetMousePosition(currentEvent.mousePosition);
                    Event.current.Use();
                    HandleUtility.Repaint();
                }
            }
            //ON MOUSE UP: IF FOUND A TARGET, THEN SET AS NEW REFERENCE FOR THAT EDGE
            else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
            {
                selectedId = -1;
                isDragging = false;
            }
            //ON LEAVE WINDOW: CANCEL EVERYTHING IF MOUSE IS OUT OF THE SCENE VIEW
            else if (currentEvent.type == EventType.MouseLeaveWindow)
            {
                selectedId = -1;
                isDragging = false;
            }
        }
        private Vector3 GetMousePosition(Vector3 eventMousePosition)
        {
            return HandleUtility.GUIPointToWorldRay(eventMousePosition).origin;
        }
        private Vector3 GetPositionById(int id, Vector3 center, Rect rect, Vector2 offset, float canvasScale)
        {
            if (id == trControlId)
                return center + Vector3.Scale(new Vector2( 1,  1), 0.5f * canvasScale * rect.size - offset);
            if (id == tlControlId)
                return center + Vector3.Scale(new Vector2(-1,  1), 0.5f * canvasScale * rect.size - offset);
            if (id == brControlId)
                return center + Vector3.Scale(new Vector2( 1, -1), 0.5f * canvasScale * rect.size - offset);
            if (id == blControlId)
                return center + Vector3.Scale(new Vector2(-1, -1), 0.5f * canvasScale * rect.size - offset);

            return center;
        }
        private void DrawHandleRepaint(int id, Vector3 position, float size, EventType currentEvent)
        {
            if (selectedId == id)
            {
                Handles.color = Color.white;
                Handles.DrawSolidDisc(position, Vector3.forward, size);
                Handles.color = Color.gray;
                Handles.CircleHandleCap(id, position, Quaternion.identity, size, currentEvent);
            }
            else
            {
                Handles.color = Color.gray;
                Handles.DrawSolidDisc(position, Vector3.forward, size);
                Handles.color = Color.white;
                Handles.CircleHandleCap(id, position, Quaternion.identity, size, currentEvent);
            }
        }
        private bool IsCornerHandleId(int id)
        {
            return id == tlControlId || id == trControlId || id == blControlId || id == brControlId;
        }
        private void UpdateCornerValues(int selectedId, float diff)
        {
            if(useIndependentCorners.boolValue)
            {
                Vector4 independentCornersDiff = Vector4.zero;
                if (selectedId == trControlId) independentCornersDiff = new Vector4(1, 0, 0, 0);
                else if (selectedId == brControlId) independentCornersDiff = new Vector4(0, 1, 0, 0);
                else if (selectedId == tlControlId) independentCornersDiff = new Vector4(0, 0, 1, 0);
                else if (selectedId == blControlId) independentCornersDiff = new Vector4(0, 0, 0, 1);

                independentCorners.vector4Value = independentCornersStartValue + independentCornersDiff * diff;
            }
            else
            {
                corners.floatValue = cornerStartValue + diff;
            }
        }
        private static void ApplyModifiedProperties(SerializedObject serializedObject)
        {
            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}