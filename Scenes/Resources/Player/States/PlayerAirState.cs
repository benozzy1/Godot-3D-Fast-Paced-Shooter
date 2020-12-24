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

        _player.MoveRelative(_player.speed, _player.airAcceleration * delta);

        var norVel = _player.velocity.Normalized();
        if (norVel != Vector3.Zero)
            _player.SetWallCastDir(norVel);
            
        //if (Input.IsActionJustReleased("jump") && _player.velocity.y > 5)
            //_player.velocity.y = 6;
    }

    public void HandleTransitions() {
        var kinematicBody = _player.kinematicBody;
        if (kinematicBody.IsOnFloor())
            _player.stateMachine.ChangeState(_player.stateMachine.groundState);
        else if (_player.WallCastColliding()) {
            _player.stateMachine.ChangeState(_player.stateMachine.wallState);
        } else if (Input.IsActionJustPressed("use_grapple"))
            _player.stateMachine.ChangeState(_player.stateMachine.grappleState);
    }

    public void Exit() {
    }
}