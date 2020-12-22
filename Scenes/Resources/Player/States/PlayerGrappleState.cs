using Godot;

public class PlayerGrappleState : IState {
    private Player _player;

    private float minYAccel = -50f;
    private float maxYAccel = 50f;

    public PlayerGrappleState(Player player) {
        _player = player;
    }

    public void Enter() {
        GD.Print("ENTER GRAPPLE STATE");
        _player.Grapple();
    }

    public void Update(float delta) {
        _player.velocity += Vector3.Down * _player.gravity * delta;
        _player.velocity.y = Mathf.Clamp(_player.velocity.y, minYAccel, maxYAccel);

        // _player.MoveRelative(_player.speed, _player.airAcceleration * delta);

        // var norVel = _player.velocity.Normalized();
        // if (norVel != Vector3.Zero)
        //     _player.SetWallCastDir(norVel);
        GD.Print(_player.grapplePoint);
    }

    public void HandleTransitions() {
        if (Input.IsActionJustReleased("use_grapple"))
            _player.stateMachine.ChangeState(_player.stateMachine.airState);
    }

    public void Exit() {
    }
}