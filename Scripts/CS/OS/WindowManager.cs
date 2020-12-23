using Godot;
using System;
using System.Collections.Generic;

public class WindowManager {
    const string WindowsDirectory = "res://Scenes/Windows/";
    const string WindowSuffix = ".res";

    public List<Window> windows = new List<Window>();

    private Window _windowLimitWindow;
    private Control _windowsNode;
    private DisplayManager _displayManager;

    public WindowManager(DisplayManager displayManager) {
        _displayManager = displayManager;

        _windowsNode = new Control();
        _windowsNode.Name = "Windows";
        _windowsNode.SetAnchorsAndMarginsPreset(Control.LayoutPreset.Wide);
        _windowsNode.MouseFilter = Control.MouseFilterEnum.Ignore;
        _displayManager.viewport.AddChild(_windowsNode);
    }

    public void CreateWindow(string windowName) {
        if (windows.Count > 3) {
            CreateWindowLimitWindow();
            return;
        }

        var newWindow = ResourceLoader.Load(WindowsDirectory + windowName + WindowSuffix) as PackedScene;
        if (newWindow == null) {
            GD.PrintErr("Could not find window: " + windowName + WindowSuffix + " in windows folder.");
            return;
        }

        var windowScene = newWindow.Instance();
        _windowsNode.AddChild(windowScene);

        var windowClass = windowScene as Window;
        windows.Add(windowClass);
        windowClass.Open();
    }

    public void CloseAllWindows() {
        foreach (var window in windows) {
            window.Close();
        }
    }

    private void CreateWindowLimitWindow() {
        if (_windowLimitWindow != null) {
            try {
                _windowLimitWindow.RectPosition = _windowLimitWindow.GetViewport().Size / 2 - _windowLimitWindow.RectSize / 2;
                _windowLimitWindow.velocity = Vector2.Zero;
            } catch (ObjectDisposedException e) {
                _windowLimitWindow = null;
            }
        }

        if (_windowLimitWindow == null) {
            var newWindow = ResourceLoader.Load(WindowsDirectory + "WindowLimitWindow" + WindowSuffix) as PackedScene;
            if (newWindow == null) {
                GD.PrintErr("Could not find Window Limit window.");
                return;
            }

            var windowScene = newWindow.Instance();
            _windowsNode.AddChild(windowScene);
            _windowLimitWindow = windowScene as Window;

            var windowClass = windowScene as Window;
            windowClass.Open();
        }
    }
}
