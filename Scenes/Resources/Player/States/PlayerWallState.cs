using Godot;

public class PlayerWallState : IState {
    private Player _player;

    private float minYAccel = -2f;
    private float maxYAccel = 3f;

    public PlayerWallState(Player player) {
        _player = player;
    }

    public void Enter() {
        GD.Print("ENTER WALL STATE");
    }

    public void Update(float delta) {
        var kinematicBody = _player.GetKinematicBody();
        if (_player.WallCastColliding()) {
            _player.velocity += Vector3.Down * _player.wallGravity * delta;
            _player.velocity.y = Mathf.Clamp(_player.velocity.y, minYAccel, maxYAccel);

            var normal = _player.GetWallNormal();
            //_player.gravityVelocity = -normal;

            _player.SetWallCastDir(-normal);

            if (Input.IsActionPressed("jump") && _player.CanJump()) {
                _player.velocity = normal * _player.jumpForce;
                _player.velocity.y = _player.jumpForce;
                _player.ResetJumpQueue();
            }
        }

        _player.MoveRelative(_player.speed, _player.airAcceleration * delta);
    }

    public void HandleTransitions() {
        var kinematicBody = _player.GetKinematicBody();
        if (kinematicBody.IsOnFloor())
            _player.stateMachine.ChangeState(_player.groundState);
        else if (!_player.WallCastColliding())
            _player.stateMachine.ChangeState(_player.airState);
    }

    public void Exit() {
    }
}