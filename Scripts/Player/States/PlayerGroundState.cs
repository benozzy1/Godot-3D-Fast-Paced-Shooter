using Godot;

public class PlayerGroundState : IState {
    private Player _player;

    public PlayerGroundState(Player player) {
        _player = player;
    }

    public void Enter() {
        GD.Print("ENTER GROUND STATE");
    }

    public void Update(float delta) {
        _player.velocity += Vector3.Down * _player.gravity * delta;
        _player.MoveHorizontallyRelative(_player.speed, _player.acceleration * delta);

        if (Input.IsActionPressed("jump") && _player.CanJump()) {
            _player.velocity.y = _player.jumpForce;
            _player.ResetJumpQueue();
        } else if (Input.IsActionPressed("crouch") && _player.CanCrouch()) {
            _player.Crouch(delta);
        } else if (!Input.IsActionPressed("crouch") && _player.CanUncrouch()) {
            _player.Uncrouch(delta);
        }
    }

    public void HandleTransitions() {
        var kinematicBody = _player.GetKinematicBody();
        if (!kinematicBody.IsOnFloor())
            _player.stateMachine.ChangeState(_player.airState);
    }

    public void Exit() {
    }
}