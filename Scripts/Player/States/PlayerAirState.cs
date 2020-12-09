using Godot;

public class PlayerAirState : IState {
    private Player _player;

    private float minYAccel = -50f;
    private float maxYAccel = 50f;

    public PlayerAirState(Player player) {
        _player = player;
    }

    public void Enter() {
        GD.Print("ENTER AIR STATE");
    }

    public void Update(float delta) {
        _player.velocity += Vector3.Down * _player.gravity * delta;
        _player.velocity.y = Mathf.Clamp(_player.velocity.y, minYAccel, maxYAccel);

        _player.MoveHorizontallyRelative(_player.speed, _player.airAcceleration * delta);
    }

    public void HandleTransitions() {
        var kinematicBody = _player.GetKinematicBody();
        if (kinematicBody.IsOnFloor())
            _player.stateMachine.ChangeState(_player.groundState);
        else if (kinematicBody.IsOnWall()) {
            _player.stateMachine.ChangeState(_player.wallState);
        }
    }

    public void Exit() {
    }
}