namespace Core {
    public interface IEvent {}

    public interface IBaseEventReceiver{
        public void OnEvent(IEvent @event);
    }

    public interface IEventReceiver<T> : IBaseEventReceiver where T : struct, IEvent {
        public void OnEvent(T @event);
    }
}