using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Hud : CanvasLayer
{

	[Signal]
	public delegate void StartGameEventHandler();

	[Export] public uint HoverCollisionLayer = 1;

	private Player player;

	private int compass_tick = 0;

	private Planet ClosestPlanet;

	private Node2D CurrentHovered = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public override void _PhysicsProcess(double delta)
	{
		if(!Main.gameInitialized) return;
		if(player == null) player = Main.player;

		compass_tick++;
		UpdateCompass();
		UpdateSpeed();
		UpdateTarget();
		
		
	}

	public Node2D GetMouseoverNode()
	{
		var Viewport = GetViewport();
		var SpaceState = PhysicsServer2D.SpaceGetDirectState(Viewport.World2D.Space);
		var MousePos_Viewport = Viewport.GetMousePosition();
		var Camera = Viewport.GetCamera2D();

		Vector2 MousePos_Global = MousePos_Viewport;

		if (Camera != null)
		{
			// MousePos_Global = Camera.GetGlobalTransform().AffineInverse() * MousePos_Viewport; // WTF DOES THIS DO?
			MousePos_Global = Viewport.CanvasTransform.AffineInverse() * MousePos_Viewport;
		}

		// if no camera, viewport mouse pos == global mouse pos.

		var query = new PhysicsPointQueryParameters2D
		{
			Position = MousePos_Global,
			CollisionMask = 1u << ((int)HoverCollisionLayer - 1), // WTF DOES THIS DO
			CollideWithAreas = true,
			CollideWithBodies = true,
		};

		var hits = SpaceState.IntersectPoint(query, 4);
		// GD.Print($"Global Pos: {MousePos_Global} -- Planet Pos: {ClosestPlanet.Position} -- Player Pos: {player.Position} -- Hits: {hits.Count}");


		Node2D newHovered = null;

		if (hits.Count > 0)
		{
			var colliderObj = hits[0]["collider"].Obj;
			if(colliderObj is Node2D collider)
			{
				newHovered = collider;
			}
		}

		return newHovered;
		
	}


	public void UpdateCompass()
	{
		if(compass_tick % 40 == 0 || ClosestPlanet == null) ClosestPlanet = FindClosestPlanet();

		float angle = 0f;

		Sprite2D Icon = GetNode<Sprite2D>("Compass_Icon");
		Sprite2D Arrow = GetNode<Sprite2D>("Compass_Arrow");
		Label Distance = GetNode<Label>("Compass_Distance");

		if (ClosestPlanet == null)
		{
			Icon.Hide();
			Arrow.Hide();
			return;
		}
		else
		{
			Icon.Show();
			Arrow.Show();
		}

		Vector2 r = ClosestPlanet.Position - player.Position;
		angle = r.Angle();

		Arrow.Rotation = angle + Mathf.Pi / 2f;
		Distance.Text = ClosestPlanet.Position.DistanceTo(player.Position).ToString("F0");

		Icon.Modulate = ClosestPlanet.color;
	}

	public Planet FindClosestPlanet()
	{

		var posPlayer = player.Position;

		Planet closestPlanet = null;
		float closestDistance = Mathf.Inf;
		foreach(Planet planet in Main.AllPlanets)
		{
			float distance = planet.Position.DistanceTo(posPlayer);
			if (distance < closestDistance)
			{
				closestPlanet = planet;
				closestDistance = distance;
			}
		}
		return closestPlanet;
	}

	public void UpdateSpeed()
	{
		var Speed = player.LinearVelocity.Length();
		var text = $"Current Speed: {Speed:F0}";

		var messageNode = GetNode<Label>("CurrentSpeed");
		messageNode.Text = text;
		messageNode.Show();
	}

	public void UpdateTarget()
	{
		Node2D result = GetMouseoverNode(); // returns global CurrentHovered node.
		if(result is RigidBody2D) CurrentHovered = result;

		if(CurrentHovered == null) return;

		Label label = GetNode<Label>("Target");
		List<String> TextList = [];

		try
		{
			TextList.Add($"Target: {CurrentHovered.Name}");
			if(CurrentHovered is RigidBody2D Body) 
			{
				TextList.Add($"Mass: {Body.Mass:F0}");
				TextList.Add($"Vel:  {Body.LinearVelocity.Length():F0}");
			}
			if(CurrentHovered is Planet planet)
			{
				TextList.Add($"Scale: {planet.childScale:F3}");
			}

			label.Text = String.Join("\n", TextList);
		}
		catch
		{
			label.Text = "Target: Object was deleted";
		}
		
		

	}

}
