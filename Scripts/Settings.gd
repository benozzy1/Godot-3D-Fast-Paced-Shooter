class_name Settings
extends Control

var mouseSensitivity;


func _on_HSlider_value_changed(value):
	mouseSensitivity = value;
	$Panel/VBoxContainer/HSlider/Label.text = str(value) + '%';


func _on_Button_pressed():
	get_tree().quit();
