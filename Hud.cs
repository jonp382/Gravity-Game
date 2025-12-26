using Godot;
using System;
using System.Linq;

public partial class Hud : CanvasLayer
{

	[Signal]
	public delegate void StartGameEventHandler();

	private Player player;

	private int compass_tick = 0;

	private Planet ClosestPlanet;

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

	}

	public void UpdateCompass()
	{
		if(compass_tick % 10 == 0 || ClosestPlanet == null) ClosestPlanet = FindClosestPlanet();

		float angle = 0f;

		Sprite2D Icon = GetNode<Sprite2D>("Compass_Icon");
		Sprite2D Arrow = GetNode<Sprite2D>("Compass_Arrow");

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

	public void ShowMessage(string text)
	{
		var message = GetNode<Label>("Message");
		message.Text = text;
		message.Show();

		GetNode<Timer>("MessageTimer").Start();
	}

	public void UpdateSpeed()
	{
		var Speed = player.LinearVelocity.Length();
		ShowMessage($"Current Speed: {Speed:F0}");
	}

}
