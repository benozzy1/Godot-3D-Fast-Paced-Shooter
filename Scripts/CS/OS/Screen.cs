using Godot;

public class Screen : Control {
    [Signal] public delegate bool loaded();

    [Export] public bool fadeIn = true;

    private static WindowManager _windowManager;
    private Tween _fadeTween;

    public override void _Ready() {
        //_windowManager = new WindowManager(this);
        
        if (!fadeIn) return;
        Modulate = new Color(1, 1, 1, 0);
        _fadeTween = new Tween();
        AddChild(_fadeTween);
        _fadeTween.InterpolateProperty(this,
            "modulate",
            new Color(1, 1, 1, 0),
            new Color(1, 1, 1, 1),
            0.5f);
        _fadeTween.Connect("tween_completed", this, "_OnTweenCompleted");
    }

    private Screen _lastScreen;
    public virtual void Load(Screen lastScreen) {
        if (!fadeIn) {
            EmitSignal("loaded");
            return;
        }
        _fadeTween.Start();
        _lastScreen = lastScreen;
    }

    public virtual void Unload() {
        QueueFree();
    }
    
    private void _OnTweenCompleted(object obj, NodePath key) {
        GD.Print("CLOSE");
        if (GetParent().GetChildCount() > 1) {
            EmitSignal("loaded");
            
            if (_lastScreen != null)
                DisplayManager.CloseScreen(_lastScreen);
        }
    }
}