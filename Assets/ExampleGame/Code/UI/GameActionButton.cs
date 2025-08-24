using Code.Core;

namespace Code.UI.Buttons
{
    public class GameActionButton : ActionButton<GameAction>
    {
        protected override void InvokeAction(GameAction action, object data = null)
        {
            UIManager.Instance.HandleGameAction(action, data);
        }
    }
}
