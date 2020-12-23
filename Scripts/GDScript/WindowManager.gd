extends Node

onready var window_manager = get_node("/root/ParentScene/ViewportContainer/Viewport/ScreenManager/WindowManager");

func create_window(window_name):
	window_manager.CreateWindow(window_name);
