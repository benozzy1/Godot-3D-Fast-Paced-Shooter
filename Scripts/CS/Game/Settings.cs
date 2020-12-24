using Godot;

public class Settings : Control {
    private const string SavePath = "res://settings.cfg";

    public static float musicVolume = 0.5f;
    public static float mouseSensitivity = 1;

    private static readonly ConfigFile Config = new ConfigFile();

    private Player _player;
    private string _configText;

    public Settings(string configText) {
        _configText = configText;
    }

    public override void _Ready() {
        _player = GetParent().GetParent().GetParent() as Player;
        UpdateUi();
    }

    private static void SaveConfig() {
        Config.SetValue("Main", "MouseSensitivity", mouseSensitivity);
        Config.SetValue("Main", "MusicVolume", musicVolume);
        Config.Save(SavePath);
        GD.Print("Saving data to file: " + SavePath);
    }

    public static void LoadConfig() {
        var error = Config.Load(SavePath);
        if (error == Error.Ok) {
            GD.Print("Config file found! Loading it...");
            musicVolume = (float)Config.GetValue("Main", "MusicVolume");
            mouseSensitivity = (float)Config.GetValue("Main", "MouseSensitivity");
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
                UpdateUi();
            }
        }

        if (Input.IsActionJustPressed("restart")) {
            GetTree().ReloadCurrentScene();
        }
    }

    private void _OnSensitivitySliderValueChanged(float value) {
        mouseSensitivity = value;
        UpdateUi();
    }

    private void _OnQuitButtonPressed() {
        GetTree().ChangeScene("res://Scenes/Menus/MainMenu.scn");
    }

    private void _OnAudioSliderValueChanged(float value) {
        musicVolume = value;
        UpdateUi();
    }

    private void UpdateUi() {
        return;

        var sensitivitySlider = GetNode("Panel/VBoxContainer/SensitivitySlider") as Slider;
        sensitivitySlider.Value = mouseSensitivity;
        ((Label)sensitivitySlider.GetNode("Label")).Text = mouseSensitivity.ToString() + "%";

        var volumeSlider = GetNode("Panel/VBoxContainer/VolumeSlider") as Slider; 
        volumeSlider.Value = musicVolume;
        ((Label)volumeSlider.GetNode("Label")).Text = (musicVolume * 100).ToString() + "%";
        _player.UpdateVolume();
    }
}
