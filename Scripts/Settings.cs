using Godot;

public class Settings : Control {
    public static float mouseSensitivity = 1;

    private Player _player;

    public override void _Ready() {
        _player = GetTree().Root.GetNode("World").GetNode("Player") as Player;
    }

    public override void _Process(float delta) {
        if (Input.IsActionJustPressed("pause")) {
            if (Visible) {
                Visible = false;
                Input.SetMouseMode(Input.MouseMode.Captured);
                _player.SetProcessInput(true);
            } else {
                Visible = true;
                Input.SetMouseMode(Input.MouseMode.Visible);
                _player.SetProcessInput(false);
            }
        }

        if (Input.IsActionJustPressed("restart")) {
            GetTree().ReloadCurrentScene();
        }
    }

    private void _OnSensitivitySliderValueChanged(float value) {
        mouseSensitivity = value;
        ((Label)GetNode("Panel/VBoxContainer/SensitivitySlider/Label")).Text = mouseSensitivity.ToString() + "%";
    }

    private void _OnQuitButtonPressed() {
        GetTree().Quit();
    }
}
