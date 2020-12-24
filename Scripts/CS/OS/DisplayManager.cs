using Godot;
using System;
using System.Collections.Generic;

public class DisplayManager : Control {
    private const string ScreensDirectory = "res://Scenes/Screens/";
    private const string ScreenSuffix = ".scn";

    private static List<Screen> _activeScreens = new List<Screen>();
    public Viewport viewport;

    private static Control _screensNode;

    private static WindowManager _windowManager;

    public override void _Ready() {
        SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
        CreateViewport();

        _screensNode = new Control {Name = "Screens"};
        _screensNode.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
        _screensNode.MouseFilter = MouseFilterEnum.Ignore;
        viewport.AddChild(_screensNode);

        _windowManager = new WindowManager(this);

        LoadScreen("BootScreen");
    }

    private void CreateViewport() {
        var viewportContainer = new ViewportContainer {Name = "ViewportContainer"};

        viewportContainer.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);

        viewportContainer.Stretch = true;
        viewportContainer.StretchShrink = 2;
        AddChild(viewportContainer);

        viewport = new Viewport {Name = "Viewport", Size = new Vector2(640, 360), TransparentBg = true};
        viewportContainer.AddChild(viewport);
    }

    private static void LoadScreen(string screenName) {
        var screenScene = ResourceLoader.Load(ScreensDirectory + screenName + ScreenSuffix) as PackedScene;
        if (screenScene == null) {
            GD.PrintErr("Could not find screen: " + screenName + ScreenSuffix + " in screens folder.");
            return;
        }

        CloseAllWindows();

        var newScreen = screenScene.Instance() as Screen;
        _activeScreens.Add(newScreen);
        _screensNode.AddChild(newScreen);
        //_screensNode.MoveChild(_currentScreen, 0);

        Screen activeScreen = null;
        if (_activeScreens.Count > 1)
            activeScreen = _activeScreens[_activeScreens.IndexOf(newScreen) - 1];
        newScreen?.Load(activeScreen);
    }

    public static WindowManager GetWindowManager() => _windowManager;

    public static void CreateWindow(string windowName) => _windowManager.CreateWindow(windowName);
    public static void CreateDialog(string windowName) => _windowManager.CreateDialog(windowName);

    private static void CloseAllWindows() => _windowManager.CloseAllWindows();

    public static void CloseScreen(Screen screen) {
        screen.Unload();
        _activeScreens.Remove(screen);
    }
}
