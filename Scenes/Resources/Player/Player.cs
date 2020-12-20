using Godot;
using System;

public class Player : Spatial {
    [Export] public float speed = 8f;
    [Export] public float acceleration = 12f;
    [Export] public float airAcceleration = 2f;
    [Export] public float gravity = 30f;
    [Export] public float wallGravity = 10f;
    [Export] public float jumpForce = 10f;

    [Export] public float cameraTilt = 0.25f;
    [Export] public float weaponSmoothing = 16f;
    [Export] public float walkAnimSpeed = 1.25f;

    [Export] public AudioStream[] footstepSounds;

    public StateMachine stateMachine;

    public Vector3 velocity;

    private KinematicBody _kinematicBody;
    private Spatial _headParent;
    private Spatial _head;
    private Camera _camera;
    private AudioStreamPlayer _audioStreamPlayer;
    private Vector3 _weaponHolderStartPos;
    private ImmediateGeometry _debugLine;
    private Vector3 _lastFloorNormal;
    private RayCast _wallCast;
    private RayCast _grappleCast;

    private bool temp;

    public override void _Ready() {
        Input.SetMouseMode(Input.MouseMode.Captured);

        _kinematicBody = GetNode("KinematicBody") as KinematicBody;
        _wallCast = _kinematicBody.GetNode("WallCast") as RayCast;

        _headParent = GetNode("Smoothing/HeadParent") as Spatial;
        _head = _headParent.GetNode("Head") as Spatial;
        _camera = _head.GetNode("Camera") as Camera;
        _grappleCast = _camera.GetNode("GrappleCast") as RayCast;
        _audioStreamPlayer = GetNode("AudioStreamPlayer") as AudioStreamPlayer;

        _weaponOffset = GetNode("Smoothing/WeaponOffset") as Spatial;
        _weaponHolder = _weaponOffset.GetNode("WeaponHolder") as Spatial;
        _weaponHolderStartPos = _weaponHolder.Translation;

        _debugLine = GetNode("ImmediateGeometry") as ImmediateGeometry;

        InitializeStates();

        Settings.LoadConfig();
        Gameplay.speedrunTimerStarted = false;
        Gameplay.speedrunTimer = 0;

        UpdateVolume();
    }

    public override void _Process(float delta) {
        ProcessInput();

        if (Input.IsActionJustPressed("fullscreen")) {
            OS.WindowFullscreen = !OS.WindowFullscreen;
        }
        //if (Input.IsKeyPressed((int)KeyList.Escape))
            //GetTree().Quit();

        AnimateCamera(delta);
        SwayWeapon(delta);
    }
    
    public override void _PhysicsProcess(float delta) {
        //DetectCollision();

        stateMachine.Update(delta);
        velocity = _kinematicBody.MoveAndSlide(velocity, Vector3.Up);
        stateMachine.HandleTransitions();

        if (_kinematicBody.GetFloorNormal() != Vector3.Zero)
            _lastFloorNormal = _kinematicBody.GetFloorNormal();
        else
            _lastFloorNormal = Vector3.Up;
        
        if (_kinematicBody.IsOnWall())
            SetWallCastDir(-_kinematicBody.GetSlideCollision(0).Normal);
    }

    private void DetectCollision() {
        if (_kinematicBody.GetSlideCount() > 0) {
            var normal = _kinematicBody.GetSlideCollision(0).Normal;
            velocity -= normal * velocity.Dot(normal);
        }
    }

    public override void _Input(InputEvent @event) {
        if (@event is InputEventMouseMotion mouseMotion) {
            _headParent.RotateY((-mouseMotion.Relative.x / 100) * 0.01f * Settings.mouseSensitivity);

            _head.RotateX((-mouseMotion.Relative.y / 100) * 0.01f * Settings.mouseSensitivity);
            _head.RotationDegrees = new Vector3(
                Mathf.Clamp(_head.RotationDegrees.x, -89, 89),
                _head.RotationDegrees.y,
                _head.RotationDegrees.z
            );
        }
    }

    public IState groundState;
    public IState airState;
    public IState wallState;
    public IState grappleState;
    private void InitializeStates() {
        groundState = new PlayerGroundState(this);
        airState = new PlayerAirState(this);
        wallState = new PlayerWallState(this);
        grappleState = new PlayerGrappleState(this);

        stateMachine = new StateMachine();
        stateMachine.ChangeState(airState);
    }

    private Vector3 _cameraRotOffset;
    private Vector3 _cameraPosOffset;
    private float _walkStep;

    private Vector3 _lastCameraRot;
    private Vector3 _deltaCameraRot;
    private void AnimateCamera(float delta) {
        var localVelocity = GetLocalVelocity();

        var yVel = localVelocity.y;
        if (_kinematicBody.IsOnFloor())
            yVel = 0;
        _cameraRotOffset = _cameraRotOffset.LinearInterpolate(
            new Vector3(localVelocity.z + yVel, 0, -localVelocity.x) * cameraTilt,
            acceleration * delta
        );

        var hVel = velocity;
        hVel.y = 0;

        var stepSpeed = _kinematicBody.IsOnFloor() ? (hVel.Length() / speed) * walkAnimSpeed : 0;

        var lastWalkStep = _walkStep;
        if (_kinematicBody.IsOnFloor())
            _walkStep += stepSpeed * speed * delta;

        var defaultPos = Vector3.Zero;
        var swayPos = new Vector3(Mathf.Sin(_walkStep), Mathf.Abs(Mathf.Cos(_walkStep)), 0) * 0.125f;

        _cameraPosOffset = _cameraPosOffset.LinearInterpolate(defaultPos + swayPos * stepSpeed, acceleration * delta);

        _lastCameraRot = _camera.RotationDegrees;

        _camera.RotationDegrees = _cameraRotOffset;
        _camera.Translation = _cameraPosOffset;

        _deltaCameraRot = _camera.RotationDegrees - _lastCameraRot;

        if (Mathf.Sign(Mathf.Sin(_walkStep * 1.25f)) != Mathf.Sign(Mathf.Sin(lastWalkStep * 1.25f))) {
            //PlayRandomFootstepSound();
        }
    }

    private Spatial _weaponHolder;
    private Spatial _weaponOffset;
    private void SwayWeapon(float delta) {
        //_weaponOffset.Rotation = _weaponOffset.Rotation.LinearInterpolate(_head.Rotation +  _headParent.Rotation, weaponSmoothing * delta);
        //_weaponHolder.Translation = _weaponHolder.ToLocal(Vector3.Zero);
        //_weaponHolder.Translation = _weaponHolderStartPos;

        //_weaponOffset.Rotation = _head.Rotation + _headParent.Rotation;

        _weaponHolder.Rotation = new Vector3(
            Mathf.LerpAngle(_weaponHolder.Rotation.x, _head.Rotation.x + _headParent.Rotation.x, weaponSmoothing * delta),
            Mathf.LerpAngle(_weaponHolder.Rotation.y, _head.Rotation.y + _headParent.Rotation.y, weaponSmoothing * delta),
            0
        );

        var posOffset = 
            -_camera.GlobalTransform.basis.z * 1f 
            + _camera.GlobalTransform.basis.x * 1f
            + _camera.GlobalTransform.basis.y * -0.75f;

        _weaponHolder.Translation = _weaponHolder.Translation.LinearInterpolate(posOffset, 18 * delta);
        // var transform = _weaponHolder.GlobalTransform;
        // transform.origin = transform.origin.LinearInterpolate(_camera.GlobalTransform.origin - posOffset, acceleration * 3 * delta);
        // _weaponHolder.GlobalTransform = transform;
        //_weaponHolder.Rotation = -new Vector3(_head.Rotation.x, _headParent.Rotation.y, 0);
        
    }

    private Vector3 _moveDirection;
    private bool _jumpQueue;
    public void ProcessInput() {
        var input = new Vector2(
            Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
            Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward")
        );
        input = input.Clamped(1);

        if (input.Length() > 0 && !Gameplay.speedrunTimerStarted && _kinematicBody.IsOnFloor() && !temp) {
            temp = true;
            Gameplay.speedrunTimerStarted = true;
            _audioStreamPlayer.Play();
        }

        if (!temp)
            return;

        _moveDirection.x = input.x;
        _moveDirection.z = input.y;
        
        if (Input.IsActionJustPressed("jump"))
            _jumpQueue = true;
        
        var lookAxis = new Vector2(
            Input.GetActionStrength("look_up") - Input.GetActionStrength("look_down"),
            Input.GetActionStrength("look_left") - Input.GetActionStrength("look_right")
        );
        _headParent.RotateY((lookAxis.y / 10) * 0.01f * Settings.mouseSensitivity);

        _head.RotateX((lookAxis.x / 10) * 0.01f * Settings.mouseSensitivity);
        _head.RotationDegrees = new Vector3(
            Mathf.Clamp(_head.RotationDegrees.x, -89, 89),
            _head.RotationDegrees.y,
            _head.RotationDegrees.z
        );
    }

    private Vector3 _relativeVelocity;
    public void MoveRelative(float speed, float accel) {
        //var dir = _moveDirection.Rotated(Vector3.Up, _headParent.Rotation.y);
        var dir = _moveDirection.Rotated(Vector3.Up, _headParent.Rotation.y).Rotated(Vector3.Up, Mathf.Deg2Rad(90)).Cross(_lastFloorNormal);
        //_relativeVelocity = _relativeVelocity.LinearInterpolate(dir * speed, accel);

        velocity.x = Mathf.Lerp(velocity.x, dir.x * speed, accel);
        velocity.z = Mathf.Lerp(velocity.z, dir.z * speed, accel);
        if (_kinematicBody.IsOnFloor())
            velocity.y = Mathf.Lerp(velocity.y, dir.y * speed, accel);
    }

    private void DrawLine(Vector3 from, Vector3 to, Color color) {
        _debugLine.Clear();
        _debugLine.Begin(PrimitiveMesh.PrimitiveType.Lines);
        _debugLine.SetColor(color);
        _debugLine.AddVertex(from);
        _debugLine.AddVertex(to);
        _debugLine.End();
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

    public void UpdateVolume() {
        if (_audioStreamPlayer != null)
            _audioStreamPlayer.VolumeDb = (Settings.musicVolume * 80) - 80;
    }

    private void _OnFinishAreaEntered(KinematicBody body) {
        _audioStreamPlayer.Stop();
        Gameplay.speedrunTimerStarted = false;
    }

    public void Land() {
        _cameraPosOffset.y = 0;
    }

    public bool WallCastColliding() => _wallCast.IsColliding() || _kinematicBody.IsOnWall();
    public void SetWallCastDir(Vector3 dir) {
        _wallCast.Rotation = Vector3.Up * (Mathf.Atan2(-dir.z, dir.x) - Mathf.Deg2Rad(90));
    }

    public Vector3 GetWallNormal() => _wallCast.GetCollisionNormal();

    public Vector3 GetMoveDirection() => _moveDirection;

    public Vector3 grapplePoint;
    public void Grapple() {
        if (_grappleCast.IsColliding())
            grapplePoint = _grappleCast.GetCollisionPoint();
    }

    public void PlayRandomFootstepSound() {
        var rand = new RandomNumberGenerator();
        rand.Randomize();

        var randInt = rand.RandiRange(0, footstepSounds.Length - 1);
        PlayAudio(footstepSounds[randInt]);
    }

    public void PlayAudio(AudioStream audioStream) {
        var streamPlayerScene = ResourceLoader.Load("res://Scenes/Resources/PortableAudioStreamPlayer.res") as PackedScene;
        var streamPlayer = streamPlayerScene.Instance() as AudioStreamPlayer;
        GetTree().Root.AddChild(streamPlayer);
        streamPlayer.Stream = audioStream;
        streamPlayer.Play();
    }
}
