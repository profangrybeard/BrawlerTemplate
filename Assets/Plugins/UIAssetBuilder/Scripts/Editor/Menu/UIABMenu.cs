using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIAB
{
    public class UIABMenu
    {
        [MenuItem("GameObject/UI Asset Builder/Rectangle", false, 10)]
        static void CreateRactangle(MenuCommand menuCommand)
        {
            CreateShapeOnScene("Rectangle", menuCommand);
        }
        [MenuItem("GameObject/UI Asset Builder/Triangle", false, 10)]
        static void CreateTriangle(MenuCommand menuCommand)
        {
            CreateShapeOnScene("Triangle", menuCommand);
        }
        [MenuItem("GameObject/UI Asset Builder/Circle", false, 10)]
        static void CreateCircle(MenuCommand menuCommand)
        {
            CreateShapeOnScene("Circle", menuCommand);
        }
        [MenuItem("GameObject/UI Asset Builder/Polygon", false, 10)]
        static void CreatePolygon(MenuCommand menuCommand)
        {
            CreateShapeOnScene("Polygon", menuCommand);
        }
        [MenuItem("GameObject/UI Asset Builder/Progress", false, 10)]
        static void CreateProgress(MenuCommand menuCommand)
        {
            CreateShapeOnScene("Progress", menuCommand);
        }
        [MenuItem("GameObject/UI Asset Builder/More", false, 10)]
        static void CreateMore(MenuCommand menuCommand)
        {
            CreateShapeOnScene("More", menuCommand);
        }
        [MenuItem("CONTEXT/Image/Convert To Rectangle")]
        static void ConvertToRectangle(MenuCommand command)
        {
            ConvertImageToShape<UIABRectangle>(command);
        }
        [MenuItem("CONTEXT/Image/Convert To Shape/Circle")]
        static void ConvertToCircle(MenuCommand command)
        {
            ConvertImageToShape<UIABCircle>(command);
        }
        [MenuItem("CONTEXT/Image/Convert To Shape/Triangle")]
        static void ConvertToTriangle(MenuCommand command)
        {
            ConvertImageToShape<UIABTriangle>(command);
        }
        [MenuItem("CONTEXT/Image/Convert To Shape/Polygon")]
        static void ConvertToPolygon(MenuCommand command)
        {
            ConvertImageToShape<UIABPolygon>(command);
        }
        [MenuItem("CONTEXT/Image/Convert To Shape/More")]
        static void ConvertToMore(MenuCommand command)
        {
            ConvertImageToShape<UIABMore>(command);
        }

        private static void CreateShapeOnScene(string shape, MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject go = CreateShapePrefab(shape);
            go.name = shape;
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        private static GameObject CreateShapePrefab(string prefabName)
        {
            string path = UIABEditorUtils.GetAssetPath($"Prefabs/UIAB{prefabName}.prefab");
            GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
            return Object.Instantiate(prefab);
        }

        private static void ConvertImageToShape<T>(MenuCommand menuCommand) where T : ShapeRenderer
        {
            Image image = (Image)menuCommand.context;
            GameObject go = image.gameObject;
            Color mainColor = image.color;
            GameObject.DestroyImmediate(image);
            T rectangle = go.AddComponent<T>();
            rectangle.SetFillColor(mainColor);
            rectangle.RenderShader();
        }
    }
}
#region REFERENCE
/*
        // Add a menu item named "Do Something" to MyMenu in the menu bar.
        [MenuItem("MyMenu/Do Something")]
        static void DoSomething()
        {
            Debug.Log("Doing Something...");
        }

        // Add a menu item named "Log Selected Transform Name" to MyMenu in the menu bar.
        // We want this to be validated menu item: an item that is only enabled if specific conditions are met.
        // To achieve this, we use a second function below to validate the menu item.
        // so it will only be enabled if we have a transform selected.
        [MenuItem("MyMenu/Log Selected Transform Name")]
        static void LogSelectedTransformName()
        {
            Debug.Log("Selected Transform is on " + Selection.activeTransform.gameObject.name + ".");
        }

        // Validate the menu item defined by the function above.
        // The "Log Selected Transform Name" menu item is disabled if this function returns false.
        // We tell the Editor that this is a validation function by decorating it with a MenuItem attribute
        // and passing true as the second parameter.
        // This invokes the MenuItem(string itemName, bool isValidateFunction) attribute constructor
        // resulting in the function being treated as the validator for "Log Selected Transform Name" menu item.
        [MenuItem("MyMenu/Log Selected Transform Name", true)]
        static bool ValidateLogSelectedTransformName()
        {
            // Return false if no transform is selected.
            return Selection.activeTransform != null;
        }

        // Add a menu item named "Do Something with a Shortcut Key" to MyMenu in the menu bar
        // and give it a shortcut (ctrl-g on Windows, cmd-g on macOS).
        [MenuItem("MyMenu/Do Something with a Shortcut Key %g")]
        static void DoSomethingWithAShortcutKey()
        {
            Debug.Log("Doing something with a Shortcut Key...");
        }

        // Add a menu item called "Double Mass" to a Rigidbody's context menu.
        [MenuItem("CONTEXT/Rigidbody/Double Mass")]
        static void DoubleMass(MenuCommand command)
        {
            Rigidbody body = (Rigidbody)command.context;
            body.mass = body.mass * 2;
            Debug.Log("Doubled Rigidbody's Mass to " + body.mass + " from Context Menu.");
        }

        // Add a menu item to create custom GameObjects.
        // Priority 10 ensures it is grouped with the other menu items of the same kind
        // and propagated to the hierarchy dropdown and hierarchy context menus.
        [MenuItem("GameObject/MyCategory/Custom Game Object", false, 10)]
        static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject go = new GameObject("Custom Game Object");
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
*/
#endregion