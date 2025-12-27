using Godot;
using System;

public partial class Camera2d : Camera2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (Input.IsActionPressed("zoom_in"))
		{
			float newZoomValue = Mathf.Clamp(Zoom.X + 0.1f, 0.1f, 2f);
			Zoom = new Vector2(newZoomValue, newZoomValue);
		}
		if (Input.IsActionPressed("zoom_out"))
		{
			float newZoomValue = Mathf.Clamp(Zoom.X - 0.1f, 0.1f, 2f);
			Zoom = new Vector2(newZoomValue, newZoomValue);
		}
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
