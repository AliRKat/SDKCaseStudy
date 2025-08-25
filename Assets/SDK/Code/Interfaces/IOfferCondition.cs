namespace SDK.Code.Interfaces {

    public interface IOfferCondition {
        bool Evaluate(IGameStateProvider state);
    }

}