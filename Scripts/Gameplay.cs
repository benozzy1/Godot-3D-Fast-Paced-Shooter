using Godot;
using System;

public class Gameplay : Node {
    public static float speedrunTimer;
    public static bool speedrunTimerStarted;

    public override void _Process(float delta) {
        if (speedrunTimerStarted)
            speedrunTimer += delta;
    }
}
