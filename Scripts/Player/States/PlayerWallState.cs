using Godot;

public class PlayerWallState : IState {
    private Player _player;

    private float minYAccel = -3f;
    private float maxYAccel = 3f;

    public PlayerWallState(Player player) {
        _player = player;
    }

    public void Enter() {
        GD.Print("ENTER WALL STATE");
    }

    public void Update(float delta) {
        _player.MoveHorizontallyRelative(_player.speed, _player.airAcceleration * delta);
        _player.velocity += Vector3.Down * _player.wallGravity * delta;
        _player.velocity.y = Mathf.Clamp(_player.velocity.y, minYAccel, maxYAccel);

        var kinematicBody = _player.GetKinematicBody();
        if (kinematicBody.GetSlideCount() > 0) {
            var normal = _player.GetKinematicBody().GetSlideCollision(0).Normal;
            _player.velocity -= normal;

            if (Input.IsActionPressed("jump") && _player.CanJump()) {
                _player.velocity += normal * _player.jumpForce;
                _player.velocity.y = _player.jumpForce;
                _player.ResetJumpQueue();
            }
        }
    }

    public void HandleTransitions() {
        var kinematicBody = _player.GetKinematicBody();
        if (kinematicBody.IsOnFloor())
            _player.stateMachine.ChangeState(_player.groundState);
        else if (!kinematicBody.IsOnWall())
            _player.stateMachine.ChangeState(_player.airState);
    }

    public void Exit() {
    }
}