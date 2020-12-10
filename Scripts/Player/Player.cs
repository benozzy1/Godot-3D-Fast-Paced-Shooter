using Godot;
using System;

public class Player : Spatial {
    [Export] public float speed = 10f;
    [Export] public float acceleration = 12f;
    [Export] public float airAcceleration = 2f;
    [Export] public float gravity = 30f;
    [Export] public float wallGravity = 10f;
    [Export] public float jumpForce = 10f;

    [Export] public float cameraTilt = 0.25f;

    public StateMachine stateMachine;

    public Vector3 velocity;
    public Vector3 gravityVec;

    private KinematicBody _kinematicBody;
    private Spatial _headParent;
    private Spatial _head;
    private Camera _camera;

    public override void _Ready() {
        Input.SetMouseMode(Input.MouseMode.Captured);

        _kinematicBody = GetNode("KinematicBody") as KinematicBody;
        _headParent = GetNode("Smoothing/HeadParent") as Spatial;
        _head = _headParent.GetNode("Head") as Spatial;
        _camera = _head.GetNode("Camera") as Camera;

        InitializeStates();
    }

    public override void _Process(float delta) {
        ProcessInput();

        if (Input.IsActionJustPressed("fullscreen"))
            OS.WindowFullscreen = !OS.WindowFullscreen;
        //if (Input.IsKeyPressed((int)KeyList.Escape))
            //GetTree().Quit();
    }

    public override void _PhysicsProcess(float delta) {
        stateMachine.Update(delta);
        velocity = _kinematicBody.MoveAndSlide(velocity, Vector3.Up);
        stateMachine.HandleTransitions();

        AnimateCamera(delta);
    }

    public override void _Input(InputEvent @event) {
        if (@event is InputEventMouseMotion mouseMotion) {
            _headParent.RotateY((-mouseMotion.Relative.x / 100) * 0.01f * Settings.mouseSensitivity);

            _head.RotateX((-mouseMotion.Relative.y / 100) * 0.01f * Settings.mouseSensitivity);
            _head.RotationDegrees = new Vector3(
                Mathf.Clamp(_head.RotationDegrees.x, -90, 90),
                _head.RotationDegrees.y,
                _head.RotationDegrees.z
            );
        }
    }

    public IState groundState;
    public IState airState;
    public IState wallState;
    private void InitializeStates() {
        groundState = new PlayerGroundState(this);
        airState = new PlayerAirState(this);
        wallState = new PlayerWallState(this);

        stateMachine = new StateMachine();
        stateMachine.ChangeState(airState);
    }

    private Vector3 _cameraRotOffset;
    private void AnimateCamera(float delta) {
        var localVelocity = GetLocalVelocity();
        _cameraRotOffset = _cameraRotOffset.LinearInterpolate(
            new Vector3(localVelocity.z - localVelocity.y, 0, -localVelocity.x) * cameraTilt,
            acceleration * delta
        );

        _camera.RotationDegrees = _cameraRotOffset;
    }

    private Vector3 _moveDirection;
    private bool _jumpQueue;
    public void ProcessInput() {
        _moveDirection.x = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        _moveDirection.z = Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward");
        _moveDirection = _moveDirection.Normalized();
        
        if (Input.IsActionJustPressed("jump"))
            _jumpQueue = true;
        
        if (_moveDirection.Length() > 0 && !Gameplay.speedrunTimerStarted) {
            Gameplay.speedrunTimerStarted = true;
        }
    }

    public void MoveHorizontallyRelative(float speed, float accel) {
        var dir = _moveDirection.Rotated(Vector3.Up, _headParent.Rotation.y);

        var hVel = velocity.LinearInterpolate(dir * speed, accel);
        velocity.x = hVel.x;
        velocity.z = hVel.z;
    }

    public void Crouch(float delta) {
        _camera.Translation = _camera.Translation.LinearInterpolate(new Vector3(0, -1, 0), acceleration * delta);
    }

    public void Uncrouch(float delta) {
        _camera.Translation = _camera.Translation.LinearInterpolate(new Vector3(0, 0, 0), acceleration * delta);
    }

    public void ResetJumpQueue() => _jumpQueue = false;

    public Vector3 GetLocalVelocity() => velocity.Rotated(Vector3.Up, -_headParent.Rotation.y);

    public KinematicBody GetKinematicBody() => _kinematicBody;
    public bool CanJump() => _jumpQueue;
    public bool CanCrouch() => true;
    public bool CanUncrouch() => true;
}
