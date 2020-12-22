using Godot;
using System;

public class PlayerCameraAnimator : Node {
    public Player player;
    public Camera camera;

    [Export] private NodePath _playerPath;
    [Export] private NodePath _cameraPath;

    [Export] public bool enabled = true;

    private Tween _landTween;

    public override void _Ready() {
        player = GetNode(_playerPath) as Player;
        camera = GetNode(_cameraPath) as Camera;

        _landTween = GetNode("LandTween") as Tween;
    }

    public override void _PhysicsProcess(float delta) {
        if (enabled)
            AnimateCamera(delta);
    }

    private Vector3 _cameraRotOffset;
    private Vector3 _cameraPosOffset;
    private float _walkStep;
    private void AnimateCamera(float delta) {
        var localVelocity = player.GetLocalVelocity();

        var yVel = localVelocity.y;
        if (player.kinematicBody.IsOnFloor())
            yVel = 0;
        _cameraRotOffset = _cameraRotOffset.LinearInterpolate(
            new Vector3(localVelocity.z - yVel, 0, -localVelocity.x) * player.cameraTilt,
            player.acceleration * delta
        );

        var hVel = player.velocity;
        hVel.y = 0;

        var stepSpeed = player.kinematicBody.IsOnFloor() ? hVel.Length() / 3 : 0;

        var lastWalkStep = _walkStep;
        if (player.kinematicBody.IsOnFloor())
            _walkStep += stepSpeed * player.speed * delta;

        var defaultPos = Vector3.Zero;
        var swayPos = new Vector3(Mathf.Sin(_walkStep), Mathf.Abs(Mathf.Cos(_walkStep)), 0) * 0.05f;

        _cameraPosOffset = _cameraPosOffset.LinearInterpolate(defaultPos + swayPos * stepSpeed, player.acceleration * delta);

        camera.RotationDegrees = _cameraRotOffset;
        camera.Translation = _cameraPosOffset;

        if (Mathf.Sign(Mathf.Cos(_walkStep * 1f)) != Mathf.Sign(Mathf.Cos(lastWalkStep * 1f))) {
            player.PlayRandomFootstepSound();
        }
    }

    private float _landForce;
    public void Land(float landForce) {
        return;

        _landForce = landForce;
        GD.Print("LAND");
        _landTween.Start();
        //_cameraPosOffset.y = landForce / player.gravity;
        _landTween.InterpolateProperty(
            this,
            "_cameraPosOffset",
            new Vector3(_cameraPosOffset.x, 0, _cameraPosOffset.z),
            new Vector3(_cameraPosOffset.x, _landForce / player.gravity, _cameraPosOffset.z),
            0.125f,
            Tween.TransitionType.Sine,
            Tween.EaseType.Out);
        _landTween.Connect("tween_completed", this, "_OnTweenCompleted");
    }

    private void _OnTweenCompleted(object obj, NodePath key) {
        _landTween.Disconnect("tween_completed", this, "_OnTweenCompleted");
        _landTween.InterpolateProperty(
            this,
            "_cameraPosOffset",
            new Vector3(_cameraPosOffset.x, _landForce / player.gravity, _cameraPosOffset.z),
            new Vector3(_cameraPosOffset.x, 0, _cameraPosOffset.z),
            0.125f,
            Tween.TransitionType.Sine,
            Tween.EaseType.Out);
    }
}
