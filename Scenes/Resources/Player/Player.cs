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
    [Export] public float weaponSmoothing = 18f;
    [Export] public float walkAnimSpeed = 1.25f;

    [Export] public AudioStream[] footstepSounds;    

    public Vector3 velocity;

    public KinematicBody kinematicBody;
    public Camera camera;
    public PlayerStateMachine stateMachine;
    public PlayerCameraAnimator cameraAnimator;

    private Spatial _headParent;
    private Spatial _head;
    private AudioStreamPlayer _audioStreamPlayer;
    private Vector3 _weaponHolderStartPos;
    private ImmediateGeometry _debugLine;
    private Vector3 _lastFloorNormal;
    private RayCast _wallCast;
    private RayCast _grappleCast;

    private bool temp;

    public override void _Ready() {
        Input.SetMouseMode(Input.MouseMode.Captured);

        kinematicBody = GetNode("KinematicBody") as KinematicBody;
        _wallCast = kinematicBody.GetNode("WallCast") as RayCast;

        _headParent = GetNode("Smoothing/HeadParent") as Spatial;
        _head = _headParent.GetNode("Head") as Spatial;
        camera = _head.GetNode("Camera") as Camera;
        _grappleCast = camera.GetNode("GrappleCast") as RayCast;
        _audioStreamPlayer = GetNode("AudioStreamPlayer") as AudioStreamPlayer;

        _weaponOffset = GetNode("Smoothing/WeaponOffset") as Spatial;
        _weaponHolder = _weaponOffset.GetNode("WeaponHolder") as Spatial;
        _weaponHolderStartPos = _weaponHolder.Translation;

        _debugLine = GetNode("ImmediateGeometry") as ImmediateGeometry;

        var components = GetNode("AdditionalComponents");
        stateMachine = components.GetNode("StateMachine") as PlayerStateMachine;
        cameraAnimator = components.GetNode("CameraAnimator") as PlayerCameraAnimator;

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
        
        SwayWeapon(delta);
    }
 
    public bool justLanded;
    public override void _PhysicsProcess(float delta) {
        //DetectCollision();

        var wasOnFloor = kinematicBody.IsOnFloor();
        var lastVelocity = velocity;

        stateMachine.Update(delta);
        velocity = kinematicBody.MoveAndSlide(velocity, Vector3.Up);
        stateMachine.HandleTransitions();

        if (kinematicBody.GetFloorNormal() != Vector3.Zero)
            _lastFloorNormal = kinematicBody.GetFloorNormal();
        else
            _lastFloorNormal = Vector3.Up;
        
        if (kinematicBody.IsOnWall())
            SetWallCastDir(-kinematicBody.GetSlideCollision(0).Normal);
        
        if (kinematicBody.IsOnFloor() && !wasOnFloor) {
            Land(lastVelocity.y);
        }
    }

    private void DetectCollision() {
        if (kinematicBody.GetSlideCount() > 0) {
            var normal = kinematicBody.GetSlideCollision(0).Normal;
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

    private Vector3 _lastCameraRot;
    private Vector3 _deltaCameraRot;


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

        var localVelocity = GetLocalVelocity();
        var posOffset = 
            -camera.GlobalTransform.basis.z * 1f + _headParent.GlobalTransform.basis.z * -(localVelocity.z / (gravity * 2))
            + camera.GlobalTransform.basis.x * 1f + _headParent.GlobalTransform.basis.x * -(localVelocity.x / (gravity * 2))
            + camera.GlobalTransform.basis.y * -0.75f + _headParent.GlobalTransform.basis.y * -(localVelocity.y / (gravity * 2));

        _weaponHolder.Translation = _weaponHolder.Translation.LinearInterpolate(posOffset, 35 * delta);
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

        if (input.Length() > 0 && !Gameplay.speedrunTimerStarted && kinematicBody.IsOnFloor() && !temp) {
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
        if (kinematicBody.IsOnFloor())
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
        camera.Translation = camera.Translation.LinearInterpolate(new Vector3(0, -1, 0), acceleration * delta);
    }

    public void Uncrouch(float delta) {
        camera.Translation = camera.Translation.LinearInterpolate(new Vector3(0, 0, 0), acceleration * delta);
    }

    public void ResetJumpQueue() => _jumpQueue = false;

    public Vector3 GetLocalVelocity() => velocity.Rotated(Vector3.Up, -_headParent.Rotation.y);

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

    public bool WallCastColliding() => _wallCast.IsColliding() || kinematicBody.IsOnWall();
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

    public void Land(float landForce) {
        cameraAnimator.Land(landForce);
    }
}
