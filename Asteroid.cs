using Godot;

public partial class Asteroid : RigidBody2D
{

	public static int MaxSpeed = 600;

	private Player player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		float RandomFactor = Mathf.Clamp(GD.Randf() * 20, 1.0f, 20.0f);
		Sprite2D Sprite =  GetNode<Sprite2D>("Sprite2D");
		CollisionShape2D Collision = GetNode<CollisionShape2D>("CollisionShape2D");


		Mass = RandomFactor;
		Vector2 ScaleFactor = new Vector2(Mathf.Max(RandomFactor/10, 0.5f), Mathf.Max(RandomFactor/10, 0.5f));
		Sprite.ApplyScale(ScaleFactor);
		Collision.ApplyScale(ScaleFactor);
		player = Main.player; 
	}
}
