extends Node

var windows = {};

func create_window(scene_name):
	var new_window = load("res://Scenes/Windows/" + scene_name + ".res");
