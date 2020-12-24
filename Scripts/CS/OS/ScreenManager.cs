using Godot;
using System;
using System.Collections.Generic;

public class ScreenManager : Control {
    private const string ScreensDirectory = "res://Scenes/Screens/";
    private const string ScreenSuffix = ".scn";

    public Screen activeScreen;
    public Screen previousScreen;

    private Control _screensNode;

    #region Start
    public override void _Ready() {
        SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
        CreateScreensNode();

        LoadScreen("BootScreen");
    }

    private void CreateScreensNode() {
        _screensNode = new Control {Name = "Screens"};
        _screensNode.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
        _screensNode.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_screensNode);
    }
    #endregion

    public Screen LoadScreen(string screenName) {
        var screenScene = ResourceLoader.Load(ScreensDirectory + screenName + ScreenSuffix) as PackedScene;
        if (screenScene == null) {
            GD.PrintErr("Could not find screen: " + screenName + ScreenSuffix + " in screens folder.");
            return null;
        }

        GameManager.windowManager.CloseAllWindows();

        previousScreen = activeScreen;
        
        var newScreen = screenScene.Instance() as Screen;
        activeScreen = newScreen;
        _screensNode.AddChild(newScreen);
        //_screensNode.MoveChild(_currentScreen, 0);
        
        newScreen?.Load();

        return newScreen;
    }

    public void CloseScreen(Screen screen) {
        screen.Unload();
    }
}
