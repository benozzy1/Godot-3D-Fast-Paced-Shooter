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
        //_player.velocity += -_player.kinematicBody.GetFloorNormal() * _player.gravity * delta;
        _player.velocity += -_player.kinematicBody.GetFloorNormal();
        //GD.Print(_player.kinematicBody.GetFloorNormal());
        _player.MoveRelative(_player.speed, _player.acceleration * delta);

        if (Input.IsActionPressed("jump") && _player.CanJump()) {
            var normal = _player.kinematicBody.GetFloorNormal();
            _player.velocity += normal * _player.jumpForce;
            _player.velocity.y = _player.jumpForce * normal.y;

            _player.ResetJumpQueue();
        } else if (Input.IsActionPressed("crouch") && _player.CanCrouch()) {
            _player.Crouch(delta);
        } else if (!Input.IsActionPressed("crouch") && _player.CanUncrouch()) {
            _player.Uncrouch(delta);
        }

        var norVel = _player.velocity.Normalized();
        if (norVel != Vector3.Zero)
            _player.SetWallCastDir(norVel);
    }

    public void HandleTransitions() {
        var kinematicBody = _player.kinematicBody;
        if (!kinematicBody.IsOnFloor())
            _player.stateMachine.ChangeState(_player.stateMachine.airState);
        else if (Input.IsActionJustPressed("use_grapple"))
            _player.stateMachine.ChangeState(_player.stateMachine.grappleState);
    }

    public void Exit() {
    }
}