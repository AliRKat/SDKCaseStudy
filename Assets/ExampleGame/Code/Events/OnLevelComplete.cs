using Core;

namespace ExampleGame.Code.Events {

    public struct OnLevelComplete : IEvent {
        public int LevelIndex;

        public OnLevelComplete(int levelIndex) {
            LevelIndex = levelIndex;
        }
    }

}