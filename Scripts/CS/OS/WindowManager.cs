using Godot;
using System;
using System.Collections.Generic;

public class WindowManager {
    private const string WindowsDirectory = "res://Scenes/Windows/";
    private const string DialogsDirectory = "res://Scenes/Windows/Dialog/";
    private const string WindowSuffix = ".res";

    private readonly List<Window> _windows = new List<Window>();
    public List<DialogWindow> dialogWindows = new List<DialogWindow>();

    private Window _windowLimitWindow;
    private readonly Control _windowsNode;

    public WindowManager(DisplayManager displayManager) {
        _windowsNode = new Control {Name = "Windows"};
        _windowsNode.SetAnchorsAndMarginsPreset(Control.LayoutPreset.Wide);
        _windowsNode.MouseFilter = Control.MouseFilterEnum.Ignore;
        displayManager.viewport.AddChild(_windowsNode);
    }

    public void CreateWindow(string windowName) {
        if (_windows.Count > 3) {
            CreateDialog("WindowLimitDialog");
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
        _windows.Add(windowClass);
        windowClass?.Open();
    }

    public void CloseAllWindows() {
        foreach (var window in _windows) {
            window.Close();
        }
    }

    public void CreateDialog(string windowName) {
        if (_windowLimitWindow != null) {
            try {
                _windowLimitWindow.CenterWindow();
                _windowLimitWindow.BringToTop();
                _windowLimitWindow.velocity = Vector2.Zero;
            } catch (ObjectDisposedException) {
                _windowLimitWindow = null;
            }
        }

        if (_windowLimitWindow != null) return;
        if (!(ResourceLoader.Load(DialogsDirectory + windowName + WindowSuffix) is PackedScene newWindow)) {
            GD.PrintErr("Could not find window dialog: " + windowName + WindowSuffix + " in dialogs folder.");
            return;
        }

        var windowScene = newWindow.Instance();
        _windowsNode.AddChild(windowScene);
        _windowLimitWindow = windowScene as Window;

        var windowClass = windowScene as Window;
        windowClass?.Open();
    }
}
