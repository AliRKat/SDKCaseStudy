using Code.Core;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif
namespace Code.UI.Buttons
{
    public class GameActionButton : ActionButton<GameAction>
    {
        protected override void InvokeAction(GameAction action, object data = null)
        {
            GameManager.Instance.UIManager.HandleGameAction(action, data);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GameActionButton))]
    public class GameActionButtonEditor : ButtonEditor
    {
        private SerializedProperty buttonTypeProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            buttonTypeProperty = serializedObject.FindProperty("_buttonType");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(buttonTypeProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
