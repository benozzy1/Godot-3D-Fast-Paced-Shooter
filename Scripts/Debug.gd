extends Spatial

var player;

func _ready():
	player = get_parent();

func _process(delta):
	#DrawLine3d.DrawLine(player.GetKinematicBody().translation, 
		#player.GetKinematicBody().translation + player.velocity,
		#Color.red);
	
	#DrawLine3d.DrawLine(player.GetKinematicBody().translation,
		#player.grapplePoint,
		#Color.blue);
	pass;
