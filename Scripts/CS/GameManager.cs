using Godot;
using System;

public class GameManager : Node {
    public static ScreenManager screenManager;
    public static WindowManager windowManager;

    public static Viewport viewport;

    public override void _Ready() {
        CreateViewport();

        screenManager = new ScreenManager {Name = "ScreenManager", MouseFilter = Control.MouseFilterEnum.Ignore};
        viewport.AddChild(screenManager);

        windowManager = new WindowManager {Name = "WindowManager", MouseFilter = Control.MouseFilterEnum.Ignore};
        viewport.AddChild(windowManager);
    }

    private void CreateViewport() {
        var viewportContainer = new ViewportContainer {Name = "ViewportContainer"};

        viewportContainer.SetAnchorsAndMarginsPreset(Control.LayoutPreset.Wide);

        viewportContainer.Stretch = true;
        viewportContainer.StretchShrink = 2;
        GetNode("/root/ParentScene/").AddChild(viewportContainer);

        viewport = new Viewport {Name = "Viewport", Size = new Vector2(640, 360), TransparentBg = true};
        viewportContainer.AddChild(viewport);
    }
}