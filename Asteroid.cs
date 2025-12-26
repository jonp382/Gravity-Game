using System.Collections.Generic;
using Godot;

public partial class Asteroid : RigidBody2D
{

	public static int MaxSpeed = 600;

	private Player player;

	public bool shouldRemove = false;

	public int ticksTouchingPlanet = 0;

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

	public override void _PhysicsProcess(double delta)
	{
		if(!Main.gameInitialized) return;

		
	}

	public void OnBodyEntered(Node NodeTouched)
	{
		if(NodeTouched is Player) return;
		if(NodeTouched is Asteroid) return;
		if(NodeTouched is Planet planet)
		{
			ticksTouchingPlanet++;
			if(ticksTouchingPlanet > 0) 
			{

				// Safe to assume the asteroid will always be smaller than the planet.
				
				planet.Mass += this.Mass;
				planet.UpdateScale();

				GD.Print($"Asteroid touching {planet.Name} - adding {this.Mass} - new mass: {planet.Mass}");
				this.shouldRemove = true;
			}
		}
		else
		{
			GD.Print($"Contacting {NodeTouched.Name}");
		}
	
	}
}
