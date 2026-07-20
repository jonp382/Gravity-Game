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

	public float childScale;

	public bool shouldRemove = false;

	private Player player;

	private int ticksTouchingPlanet = 0;
	public override void _Ready()
	{

	}

	public void ApplyColor(Color Col)
	{
		Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");
		sprite.Modulate = Col;
		color = Col;
	}

	public override void _PhysicsProcess(double delta)
	{
		if(!Main.gameInitialized) return;
		if(player == null) player = Main.player;


	}

	public void UpdateScale()
	{
		// R/RRef = (M/MRef) ^ (1/3.7)
		// R/RRef => Scale (i think?)
		// Scale = (M/MRef) ^ (1/3.7)
		// (1/3.7) is the empirical formula. adjusting for gameplay reasons.
		// larger denominator means smaller scale adjustment.
		float uniformScale = Mathf.Clamp((float)Mathf.Pow(Mass/ReferenceMass, 1/3.7f), 0.1f, 10f);
		// GD.Print($"Mass: {Mass} - Ratio: {Mass/ReferenceMass} - Scale: {uniformScale}");
		childScale = uniformScale;
		foreach(var Node in GetChildren())
		{
			if(Node is Sprite2D sprite) 
			{
				sprite.Scale = new Vector2(uniformScale, uniformScale);
				return;
			}
			else if (Node is CollisionShape2D collision)
			{
				collision.Scale = new Vector2(uniformScale, uniformScale);
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
			// GD.Print($"Touching {planet.Name}");
			
			List<Planet> BothPlanets = new List<Planet> {
				this,
				planet
			};
			BothPlanets = BothPlanets.OrderBy(n => n.Mass).ToList();

			Planet LargerPlanet = BothPlanets.Last();
			Planet SmallerPlanet = BothPlanets.First();

			float MassDiff = SmallerPlanet.Mass;
			if(MassDiff <= 0) return;

			LargerPlanet.Mass += MassDiff;
			SmallerPlanet.Mass -= MassDiff;
			SmallerPlanet.UpdateScale();
			LargerPlanet.UpdateScale();

			SmallerPlanet.shouldRemove = true;

			// GD.Print($"B4 Larger Planet {LargerPlanet.Name} Mass: {LargerPlanet.Mass}, Smaller Planet {SmallerPlanet.Name} Mass: {SmallerPlanet.Mass}");

			// GD.Print($"AF Larger Planet {LargerPlanet.Name} Mass: {LargerPlanet.Mass}, Smaller Planet {SmallerPlanet.Name} Mass: {SmallerPlanet.Mass}");

			
			

		}
		else
		{
			GD.Print($"Contacting {NodeTouched.Name}");
		}
	
	}
}
