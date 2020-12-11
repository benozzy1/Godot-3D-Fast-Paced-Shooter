using Godot;

public class Settings : Control {
    private const string SavePath = "res://settings.cfg";

    public static float musicVolume = 0.5f;
    public static float mouseSensitivity = 1;

    private static ConfigFile config = new ConfigFile();

    private Player _player;
    private string _configText;

    public override void _Ready() {
        _player = GetTree().Root.GetNode("World").GetNode("Player") as Player;
        UpdateUI();
    }

    public static void SaveConfig() {
        config.SetValue("Main", "MouseSensitivity", mouseSensitivity);
        config.SetValue("Main", "MusicVolume", musicVolume);
        config.Save(SavePath);
        GD.Print("Saving data to file: " + SavePath);
    }

    public static void LoadConfig() {
        var error = config.Load(SavePath);
        if (error == Error.Ok) {
            GD.Print("Config file found! Loading it...");
            musicVolume = (float)config.GetValue("Main", "MusicVolume");
            mouseSensitivity = (float)config.GetValue("Main", "MouseSensitivity");
        } else {
            GD.Print("Config file not found! Creating a new one...");
            SaveConfig();
        }
    }

    public override void _Process(float delta) {
        if (Input.IsActionJustPressed("pause")) {
            if (Visible) {
                Visible = false;
                Input.SetMouseMode(Input.MouseMode.Captured);
                _player.SetProcessInput(true);
                Settings.SaveConfig();
            } else {
                Visible = true;
                Input.SetMouseMode(Input.MouseMode.Visible);
                _player.SetProcessInput(false);
                UpdateUI();
            }
        }

        if (Input.IsActionJustPressed("restart")) {
            GetTree().ReloadCurrentScene();
        }
    }

    private void _OnSensitivitySliderValueChanged(float value) {
        mouseSensitivity = value;
        UpdateUI();
    }

    private void _OnQuitButtonPressed() {
        GetTree().Quit();
    }

    private void _OnAudioSliderValueChanged(float value) {
        musicVolume = value;
        UpdateUI();
    }

    public void UpdateUI() {
        var sensitivitySlider = GetNode("Panel/VBoxContainer/SensitivitySlider") as Slider;
        sensitivitySlider.Value = mouseSensitivity;
        ((Label)sensitivitySlider.GetNode("Label")).Text = mouseSensitivity.ToString() + "%";

        var volumeSlider = GetNode("Panel/VBoxContainer/VolumeSlider") as Slider; 
        volumeSlider.Value = musicVolume;
        _player.SetVolume(musicVolume);
        ((Label)volumeSlider.GetNode("Label")).Text = (musicVolume * 100).ToString() + "%";
    }
}
