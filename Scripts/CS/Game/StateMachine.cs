public class StateMachine {
    public IState currentState;

    public void Update(float delta) {
        if (currentState != null) {
            currentState.Update(delta);
        }
    }

    public void ChangeState(IState newState) {
        if (currentState != null)
            currentState.Exit();
        
        currentState = newState;
        currentState.Enter();
    }

    public void HandleTransitions() {
        currentState.HandleTransitions();
    }
}

public interface IState {
    void Enter();
    void Update(float delta);
    void Exit();
    void HandleTransitions();
}