using Godot;
using System;

public class DisplayManager : Control {
    const string ScreensDirectory = "res://Scenes/Screens/";
    const string ScreenSuffix = ".scn";

    public static Node currentScreen;
    public Viewport viewport;

    private static Control _screensNode;

    private string[] screens;

    private static WindowManager _windowManager;

    public override void _Ready() {
        SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
        CreateViewport();

        _screensNode = new Control();
        _screensNode.Name = "Screens";
        _screensNode.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
        _screensNode.MouseFilter = Control.MouseFilterEnum.Ignore;
        viewport.AddChild(_screensNode);

        _windowManager = new WindowManager(this);

        LoadScreen("BootScreen");
    }

    private void CreateViewport() {
        var viewportContainer = new ViewportContainer();
        viewportContainer.Name = "ViewportContainer";
        
        viewportContainer.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);

        viewportContainer.Stretch = true;
        viewportContainer.StretchShrink = 2;
        AddChild(viewportContainer);

        viewport = new Viewport();
        viewport.Name = "Viewport";
        viewport.Size = new Vector2(640, 360);
        viewport.TransparentBg = true;
        viewportContainer.AddChild(viewport);
    }

    public static void LoadScreen(string screenName) {
        var newScreen = ResourceLoader.Load(ScreensDirectory + screenName + ScreenSuffix) as PackedScene;
        if (newScreen == null) {
            GD.PrintErr("Could not find screen: " + screenName + ScreenSuffix + " in screens folder.");
            return;
        }

        CloseAllWindows();
        if (currentScreen != null)
            currentScreen.QueueFree();
        currentScreen = newScreen.Instance();
        _screensNode.AddChild(currentScreen);
    }

    public static WindowManager GetWindowManager() => _windowManager;

    public static void CreateWindow(string windowName) => _windowManager.CreateWindow(windowName);

    public static void CloseAllWindows() => _windowManager.CloseAllWindows();
}
