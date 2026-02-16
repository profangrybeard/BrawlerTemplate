using UnityEditor;
using UnityEngine;

namespace UIAB
{
    [System.Serializable]
    public struct UIABDistance : ICustomInspector, IValidatable, ILerpable<UIABDistance>
    {
        public static readonly UIABDistance Default = new UIABDistance(false, -45, 5, new Vector2(5f, -5f));

        [SerializeField] private bool useAngle;
        [SerializeField] private Vector2 offset;
        [SerializeField] [Range(-180,180)] private int angle;
        [SerializeField] private float distance;

        public float Distance { get { return distance; } }

        #region GET/SET
        public void UseAngle(bool useAngle) { this.useAngle = useAngle; }
        public bool IsUseAngle() { return useAngle; }
        public void SetOffset(Vector2 offset) { this.offset = offset; }
        public Vector2 GetOffset() { return offset; }
        public void SetAngle(int angle) { this.angle = angle; }
        public int GetAngle() { return angle; }
        public void SetDistance(float distance) { this.distance = distance; }
        public float GetDistance() { return distance; }
        #endregion

        public Vector2 GetActualOffset()
        {
            if (!useAngle) return offset;
            return distance * new Vector2 (Mathf.Sin(Mathf.Deg2Rad * (90 - angle)), Mathf.Cos(Mathf.Deg2Rad * (90 - angle)));
        }

        public UIABDistance(bool useAngle = false, int angle = -45, float distance = 5f, Vector2 offset = new Vector2())
        {
            this.useAngle = useAngle;
            this.angle = angle;
            this.distance = distance;
            this.offset = offset;
        }

        #region INSPECTOR
#if UNITY_EDITOR
        public void DrawProperties(SerializedProperty property, string label = "", params bool[] filter)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("useAngle"), new GUIContent("Use Angle"));
            if (useAngle)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("angle"), new GUIContent("Angle"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("distance"), new GUIContent("Distance"));
            }
            else
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("offset"), new GUIContent("Distance"));
            }
        }
#endif
        #endregion

        public void Validate()
        {

        }

        public UIABDistance Lerp(UIABDistance target, float lerp)
        {
            UIABDistance newDistance = new UIABDistance();
            newDistance.useAngle = useAngle;
            newDistance.angle = (int)Mathf.Lerp(angle, target.angle, lerp);
            newDistance.distance = Mathf.Lerp(distance, target.distance, lerp);
            newDistance.offset = Vector2.Lerp(offset, target.offset, lerp);

            return newDistance;
        }
    }
}