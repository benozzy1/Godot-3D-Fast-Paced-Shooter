using Godot;
using System;

internal enum ResizeDir {
	Top,
	Bottom,
	Left,
	Right,
	BottomRight,
	BottomLeft,
	TopRight,
	TopLeft,
}

public class Window : Control {
    [Signal] public delegate bool focused();

    [Signal]
    public delegate bool unfocused();
    
    [Export] public bool resizable;
    [Export] public float deaccel = 8;
    [Export] public float throwForce = 600;
    [Export] public bool quitClosesWindow = true;

    private bool _focused;
    
    private bool _dragging;
    private bool _resizing;

    private Vector2 _dragOffset;

    private Vector2 _resizeOffset;
    private Vector2 _originalRectPos;
    private Vector2 _originalRectSize;
    private ResizeDir _resizeDir;

    private Vector2 _mousePos;
    private Vector2 _mouseMotion;

    public Vector2 velocity;

    private Tween _tween;

    public override void _Ready() {
        Modulate = new Color(1, 1, 1, 0);

        SetProcessInput(true);
        
        _tween = GetNode("Tween") as Tween;

        _originalRectPos = RectPosition;
        _originalRectSize = RectSize;

        ((Control)GetNode("BorderDraggables")).Visible = resizable;
    }

    public override void _Process(float delta) {
        velocity = velocity.LinearInterpolate(Vector2.Zero, deaccel * delta);
        velocity = velocity.Clamped(2000);
        
        RectPosition += velocity * delta;
        
        if (RectPosition.x > GetViewportRect().Size.x - RectSize.x)
            velocity.x *= -0.5f;
        if (RectPosition.x < 0)
            velocity.x *= -0.5f;
        if (RectPosition.y > GetViewportRect().Size.y - RectSize.y)
            velocity.y *= -0.5f;
        if (RectPosition.y < 0)
            velocity.y *= -0.5f;

        if (GetParent() is Control parent) {
            RectPosition = new Vector2(
                Mathf.Clamp(RectPosition.x, 0, parent.RectSize.x - RectSize.x),
                Mathf.Clamp(RectPosition.y, 0, parent.RectSize.y - RectSize.y)
            );
        }
    }

    public override void _Input(InputEvent e) {
        switch (e) {
            case InputEventMouseButton btn: {
                if (!btn.Pressed) {
                    if (_dragging)
                        velocity = _mouseMotion * throwForce;
                
                    _dragging = false;
                    _resizing = false;
                }

                break;
            }
            case InputEventMouseMotion mouse: {
                _mousePos = mouse.Position;
                _mouseMotion = mouse.Relative.Round();
            
                if (_dragging)
                    RectPosition = mouse.Position + _dragOffset;
                else if (_resizing) {
                    switch (_resizeDir) {
                        case ResizeDir.Left:
                            ResizeLeft(mouse.Position);
                            break;
                        case ResizeDir.Right:
                            ResizeBottom(mouse.Position);
                            break;
                        case ResizeDir.Top:
                            ResizeTop(mouse.Position);
                            break;
                        case ResizeDir.Bottom:
                            ResizeBottom(mouse.Position);
                            break;
                    
                        case ResizeDir.TopRight:
                            ResizeTop(mouse.Position);
                            ResizeRight(mouse.Position);
                            break;
                        case ResizeDir.TopLeft:
                            ResizeTop(mouse.Position);
                            ResizeLeft(mouse.Position);
                            break;
                        case ResizeDir.BottomLeft:
                            ResizeBottom(mouse.Position);
                            ResizeLeft(mouse.Position);
                            break;
                        case ResizeDir.BottomRight:
                            ResizeBottom(mouse.Position);
                            ResizeRight(mouse.Position);
                            break;
                    }
            
                }

                //RectPosition = new Vector2(
                //Mathf.Clamp(RectPosition.x, 0, GetViewportRect().Size.x - RectSize.x),
                //Mathf.Clamp(RectPosition.y, 0, GetViewportRect().Size.y - RectSize.y)
                //);
                break;
            }
        }
    }
    
    public void CenterWindow() {
        RectPosition = GetViewport().Size / 2 - RectSize / 2;
    }

    #region Resize Methods

    private void ResizeTop(Vector2 eventPos) {
        var newPos = RectPosition;
        var newSize = RectSize;
        
        var mouseOffset = eventPos.y - (_originalRectPos.y + _resizeOffset.y);
        newSize.y = _originalRectSize.y - mouseOffset;
        newPos.y = _originalRectSize.y + _resizeOffset.y - (RectSize.y - _originalRectPos.y) - 1;

        RectPosition = newPos;
        RectSize = newSize;
    }
    private void ResizeBottom(Vector2 eventPos) {
        var newSize = RectSize;

        newSize.y = (eventPos.y - _originalRectPos.y) + 1;

        RectSize = newSize;
    }
    private void ResizeLeft(Vector2 eventPos) {
        var newPos = RectPosition;
        var newSize = RectSize;

        var mouseOffset = eventPos.x - (_originalRectPos.x + _resizeOffset.x);
        newSize.x = _originalRectSize.x - mouseOffset;
        newPos.x = _originalRectSize.x + _resizeOffset.x - (RectSize.x - _originalRectPos.x) - 1;

        RectPosition = newPos;
        RectSize = newSize;
    }
    private void ResizeRight(Vector2 eventPos) {
        var newSize = RectSize;

        newSize.x = (eventPos.x - _originalRectPos.x) + 1;

        RectSize = newSize;
    }

    #endregion

    #region Gui Input Methods
    
    private void _on_CloseButton_pressed() {
        if (quitClosesWindow)
            GameManager.windowManager.CloseWindow(this);
    }
    private void _OnTitlebarGuiInput(InputEvent e) {
        if (!(e is InputEventMouseButton btn)) return; // Don't run if event is not mouse button
        if (btn.ButtonIndex != (int) ButtonList.Left) return; // Don't run if mouse button is not left
        if (!btn.Pressed) return; // Don't run if mouse button is not pressed
        
        BringToTop();
                    
        _originalRectPos = RectPosition;
        _dragging = true;
        _dragOffset = RectPosition - _mousePos;
                    
        velocity = Vector2.Zero;
    }
    private void _on_Border_gui_input(InputEvent e, string borderSide) {
        if (!(e is InputEventMouseButton btn)) return; // Don't run if event is not mouse button
        if (btn.ButtonIndex != (int) ButtonList.Left || !btn.Pressed) return; // Don't run if mouse button is not left
        
        BringToTop();
                
        _originalRectSize = RectSize;
        _originalRectPos = RectPosition;
        _resizing = true;
        _resizeOffset = RectPosition - _mousePos;

        if (!_resizing) return; // Don't run if not resizing
        switch (borderSide) {
            case "top":
                _resizeDir = ResizeDir.Top;
                break;
            case "bottom":
                _resizeDir = ResizeDir.Bottom;
                break;
            case "left":
                _resizeDir = ResizeDir.Left;
                break;
            case "right":
                _resizeDir = ResizeDir.Right;
                break;
            case "top_right":
                _resizeDir = ResizeDir.TopRight;
                break;
            case "bottom_right":
                _resizeDir = ResizeDir.BottomRight;
                break;
            case "top_left":
                _resizeDir = ResizeDir.TopLeft;
                break;
            case "bottom_left":
                _resizeDir = ResizeDir.BottomLeft;
                break;
        }
    }
    private void _on_Content_gui_input(InputEvent e) {
        if (!(e is InputEventMouseButton btn)) return;
        if (btn.Pressed && btn.ButtonIndex == 0)
            BringToTop();
    }

    #endregion
    
    public void BringToTop() {
        var parent = GetParent();
        parent.RemoveChild(this);
        parent.AddChild(this);
    }

    public void Open() {
        //GameManager.screenManager.GetWindowManager().AddWindow(this);
        _tween.Start();
        _tween.InterpolateProperty(this, "modulate", Modulate, new Color(1, 1, 1, 1), 0.5f, Tween.TransitionType.Sine);
        CenterWindow();
    }

    public void Close() {
        _tween.Start();
        _tween.InterpolateProperty(this, "modulate", Modulate, new Color(1, 1, 1, 0), 0.5f, Tween.TransitionType.Sine);
        _tween.Connect("tween_completed", this, "_OnTweenCompleted");
    }

    private void _OnTweenCompleted(object obj, NodePath key) {
        QueueFree();
    }
}