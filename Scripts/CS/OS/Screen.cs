using Godot;

public class Screen : Control {
    [Signal] public delegate bool loaded();
    [Signal] public delegate bool unloaded();

    [Export] public float fadeInTime = 0.5f;

    private Tween _fadeTween;

    public override void _Ready() {
        Modulate = new Color(1, 1, 1, 0);
        _fadeTween = new Tween();
        AddChild(_fadeTween);
    }

    public virtual void Load() {
        _fadeTween.Start();
        _fadeTween.InterpolateProperty(this,
            "modulate",
            new Color(1, 1, 1, 0),
            new Color(1, 1, 1, 1),
            fadeInTime,
            Tween.TransitionType.Sine);
        _fadeTween.Connect("tween_completed", this, "_OnTweenCompleted");
    }

    public virtual void Unload() {
        EmitSignal("unloaded");
        QueueFree();
    }
    
    private void _OnTweenCompleted(object obj, NodePath key) {
        if (GetParent().GetChildCount() > 1) {
            EmitSignal("loaded");

            var lastScreen = GameManager.screenManager.previousScreen;
            if (lastScreen != null)
                GameManager.screenManager.CloseScreen(lastScreen);
        }
    }
}