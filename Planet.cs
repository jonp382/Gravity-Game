using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Planet : RigidBody2D
{
	// Called when the node enters the scene tree for the first time.

	public static int MaxSpeed = 600;

	public static readonly float ReferenceMass = 100000f; // Earth mass
	public static readonly float ReferenceScale = 1.0f;

	public Color color;

	public bool shouldRemove = false;

	private Player player;

	private int ticksTouchingPlanet = 0;
	public override void _Ready()
	{
		// float RandomFactor = Mathf.Clamp(GD.Randf() * 20, 1.0f, 20.0f);
		Sprite2D Sprite =  GetNode<Sprite2D>("Sprite2D");
		CollisionShape2D Collision = GetNode<CollisionShape2D>("CollisionShape2D");


		Mass = 1;
		// Vector2 ScaleFactor = new Vector2(Mathf.Max(RandomFactor/10, 0.5f), Mathf.Max(RandomFactor/10, 0.5f));
		// Sprite.ApplyScale(ScaleFactor);
		// Collision.ApplyScale(ScaleFactor);

	}

	public void ApplyColor(Color Col)
	{
		foreach(var node in GetChildren())
		{
			if(node is Sprite2D sprite)
			{
				sprite.Modulate = Col;
				color = Col;
				return;
			}
		}
	}

    public override void _PhysicsProcess(double delta)
    {
        if(!Main.gameInitialized) return;
		if(player == null) player = Main.player;


    }

	public void UpdateScale()
	{

		float uniformScale = Mathf.Clamp(Mass/ReferenceMass, 0.1f, 10f);

		foreach(var Node in GetChildren())
		{
			if(Node is Sprite2D sprite) 
			{
				sprite.ApplyScale(new Vector2(uniformScale, uniformScale));
				return;
			}
			else if (Node is CollisionShape2D collision)
			{
				collision.ApplyScale(new Vector2(uniformScale, uniformScale));
			}
			
		}
	}

	public void OnBodyEntered(Node NodeTouched)
	{
		if(NodeTouched is Player) return;
		if(NodeTouched is Asteroid) return;
		if(NodeTouched is Planet planet)
		{
			if(Object.ReferenceEquals(planet, this)) return;
			GD.Print($"Touching {planet.Name}");

			ticksTouchingPlanet++;
			if(ticksTouchingPlanet % 2 != 0) return;

			
			List<Planet> BothPlanets = new List<Planet> {
				this,
				planet
			};
			BothPlanets = BothPlanets.OrderBy(n => n.Mass).ToList();

			Planet LargerPlanet = BothPlanets.Last();
			Planet SmallerPlanet = BothPlanets.First();

			float MassDiff = SmallerPlanet.Mass / 2;
			if(MassDiff <= 0) return;

			if(SmallerPlanet.Mass - MassDiff < 50)
			{
				// MassDiff *= 2;
				SmallerPlanet.shouldRemove = true;
			}

			GD.Print($"B4 Larger Planet {LargerPlanet.Name} Mass: {LargerPlanet.Mass}, Smaller Planet {SmallerPlanet.Name} Mass: {SmallerPlanet.Mass}");

			LargerPlanet.Mass += MassDiff;
			SmallerPlanet.Mass -= MassDiff;
			SmallerPlanet.UpdateScale();
			LargerPlanet.UpdateScale();

			GD.Print($"AF Larger Planet {LargerPlanet.Name} Mass: {LargerPlanet.Mass}, Smaller Planet {SmallerPlanet.Name} Mass: {SmallerPlanet.Mass}");

			
			

		}
		else
		{
			GD.Print($"Contacting {NodeTouched.Name}");
		}
	
	}
}
