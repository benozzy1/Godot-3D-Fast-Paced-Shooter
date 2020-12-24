public class StateMachine {
    private IState _currentState;

    public void Update(float delta) {
        _currentState?.Update(delta);
    }

    public void ChangeState(IState newState) {
        _currentState?.Exit();

        _currentState = newState;
        _currentState.Enter();
    }

    public void HandleTransitions() {
        _currentState.HandleTransitions();
    }
}

public interface IState {
    void Enter();
    void Update(float delta);
    void Exit();
    void HandleTransitions();
}