extends Control

enum ResizeDir {
	TOP,
	BOTTOM,
	LEFT,
	RIGHT,
	BOTTOM_RIGHT,
	BOTTOM_LEFT,
	TOP_RIGHT,
	TOP_LEFT,
}

export var resizable = false;

var dragging = false;
var resizing = false;

var drag_offset = Vector2();

var resize_offset = Vector2();
var original_rect_position = rect_position;
var original_rect_size = rect_size;
var resize_dir;

var mouse_pos;
var mouse_motion;

var deaccel = 8;
var throw_multiplier = 800;

var velocity = Vector2();

func _ready():
	set_process_input(true);
	
	$BorderDraggables.visible = resizable;

func _process(delta):
	velocity = velocity.linear_interpolate(Vector2.ZERO, deaccel * delta);
	velocity = velocity.clamped(2000);
	
	rect_position += velocity * delta;
	
	if rect_position.x > get_viewport_rect().size.x - rect_size.x:
		velocity.x *= -0.5;
	if rect_position.x < 0:
		velocity.x *= -0.5;
	if rect_position.y > get_viewport_rect().size.y - rect_size.y:
		velocity.y *= -0.5;
	if rect_position.y < 0:
		velocity.y *= -0.5;
	
	rect_position.x = clamp(rect_position.x, 0, get_viewport_rect().size.x - rect_size.x);
	rect_position.y = clamp(rect_position.y, 0, get_viewport_rect().size.y - rect_size.y);

func _input(event):
	if event is InputEventMouseButton:
		if !event.pressed:
			if dragging:
				velocity = mouse_motion * throw_multiplier
			
			dragging = false;
			resizing = false;
	
	if event is InputEventMouseMotion:
		mouse_pos = event.position;
		mouse_motion = event.relative;
		
		print(mouse_pos - (original_rect_position + drag_offset))
		
		if dragging:
			rect_position = event.position + drag_offset;
		elif resizing:
			match resize_dir:
				ResizeDir.LEFT:
					resize_left(event.position);
				ResizeDir.RIGHT:
					resize_right(event.position);
				ResizeDir.TOP:
					resize_top(event.position);
				ResizeDir.BOTTOM:
					resize_bottom(event.position);

				ResizeDir.TOP_RIGHT:
					#var mouse_offset = event.position.y - (original_rect_position.y + resize_offset.y);
					#rect_size.x = event.position.x - original_rect_position.x;
					#rect_size.y = original_rect_size.y - mouse_offset;
					
					#rect_position.x = original_rect_size.x + resize_offset.x - (rect_size.x - original_rect_position.x);
					#rect_position.y = original_rect_size.y + resize_offset.y - (rect_size.y - original_rect_position.y);
					resize_top(event.position);
					resize_right(event.position);
				ResizeDir.TOP_LEFT:
					#var mouse_offset = event.position - (original_rect_position + resize_offset);
					#rect_size.x = original_rect_size.x - mouse_offset.x;
					#rect_position.x = original_rect_size.x + resize_offset.x - (rect_size.x - original_rect_position.x);
					
					#rect_size.y = original_rect_size.y - mouse_offset.y;
					#rect_position.y = original_rect_size.y + resize_offset.y - (rect_size.y - original_rect_position.y);
					resize_top(event.position);
					resize_left(event.position);
				ResizeDir.BOTTOM_LEFT:
					#var mouse_offset = event.position.x - (original_rect_position.x + resize_offset.x);
					#rect_size.x = original_rect_size.x - mouse_offset;
					#rect_position.x = original_rect_size.x + resize_offset.x - (rect_size.x - original_rect_position.x);
					
					#rect_size.y = event.position.y - original_rect_position.y;
					resize_bottom(event.position);
					resize_left(event.position);
				ResizeDir.BOTTOM_RIGHT:
					#rect_size = event.position - original_rect_position;
					resize_bottom(event.position);	
					resize_right(event.position);
		
		rect_position.x = clamp(rect_position.x, 0, get_viewport_rect().size.x - rect_size.x);
		rect_position.y = clamp(rect_position.y, 0, get_viewport_rect().size.y - rect_size.y);

func resize_top(event_pos):
	var mouse_offset = event_pos.y - (original_rect_position.y + resize_offset.y);
	rect_size.y = original_rect_size.y - mouse_offset;
	rect_position.y = original_rect_size.y + resize_offset.y - (rect_size.y - original_rect_position.y) - 1;

func resize_bottom(event_pos):
	rect_size.y = (event_pos.y - original_rect_position.y) + 1;

func resize_left(event_pos):
	var mouse_offset = event_pos.x - (original_rect_position.x + resize_offset.x);
	rect_size.x = original_rect_size.x - mouse_offset;
	rect_position.x = original_rect_size.x + resize_offset.x - (rect_size.x - original_rect_position.x) - 1;

func resize_right(event_pos):
	rect_size.x = (event_pos.x - original_rect_position.x) + 1;

func _on_CloseButton_pressed():
	queue_free();

func _on_Titlebar_gui_input(event):	
	if event is InputEventMouseButton:
		if event.button_index == BUTTON_LEFT:
			if event.pressed:
				bring_to_top();
				
				original_rect_position = rect_position;
				dragging = true;
				drag_offset = rect_position - mouse_pos;
				
				velocity = Vector2.ZERO;

func _on_Border_gui_input(event, border_side):
	if event is InputEventMouseButton:
		if event.button_index == BUTTON_LEFT and event.pressed:
			bring_to_top();
			
			original_rect_size = rect_size;
			original_rect_position = rect_position;
			resizing = true;
			resize_offset = rect_position - mouse_pos;
			
			if (resizing):
				match border_side:
					"top":
						resize_dir = ResizeDir.TOP;
					"bottom":
						resize_dir = ResizeDir.BOTTOM;
					"left":
						resize_dir = ResizeDir.LEFT;
					"right":
						resize_dir = ResizeDir.RIGHT;
					"top_right":
						resize_dir = ResizeDir.TOP_RIGHT;
					"bottom_right":
						resize_dir = ResizeDir.BOTTOM_RIGHT;
					"top_left":
						resize_dir = ResizeDir.TOP_LEFT;
					"bottom_left":
						resize_dir = ResizeDir.BOTTOM_LEFT;

func bring_to_top():
	var parent = get_parent();
	parent.remove_child(self);
	parent.add_child(self);


func _on_Content_gui_input(event):
	if event is InputEventMouseButton and event.pressed and event.button_index == BUTTON_LEFT:
		bring_to_top();
