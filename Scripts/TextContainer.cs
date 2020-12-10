using Godot;
using System;

public class TextContainer : HBoxContainer {
    private Label _timerText;
    private Label _fpsText;

    public override void _Ready() {
        _timerText = GetNode("TimerText") as Label;
        _fpsText = GetNode("FPSText") as Label;
    }

    public override void _Process(float delta) {
        _timerText.Text = "Timer: " + Gameplay.speedrunTimer;
        _fpsText.Text = "FPS: " + Engine.GetFramesPerSecond();
    }
}
