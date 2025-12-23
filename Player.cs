using Godot;

public partial class Player : RigidBody2D
{
	[Signal]
	public delegate void HitEventHandler();

	[Export]
	public int Speed = 400;
	public Vector2 ScreenSize;
	private float turnSpeed = 0.05f;
	private float rotationDir = 0;
	private Vector2 thrust = new(0,-40000);

	public bool NegativeMass = false;

	private float targetAngle = 0;

	private Sprite2D Sprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ScreenSize = GetViewportRect().Size;
		Hide();
		Mass = 1000;
		Start(new Vector2(ScreenSize.X / 2, ScreenSize.Y / 2));
		Sprite = GetNode<Sprite2D>("Sprite2D");
	}

	public void OnBodyEntered(Node2D Body)
	{
		// Hide();
		// EmitSignal(SignalName.Hit);
		// GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	}

	public void Start(Vector2 position)
	{
		Position = position;
		Show();
		GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

    public override void _PhysicsProcess(double delta)
    {
        

		AdjustRotation();


		if(Input.IsActionPressed("move_up")) 	ApplyCentralForce(thrust.Rotated(Rotation));
		if(Input.IsActionPressed("move_down")) 	ApplyCentralForce(thrust.Rotated(Rotation + Mathf.Pi));

		if(Input.IsActionPressed("spacebar")) 
		{
			NegativeMass = true;
		}
		else
		{
			NegativeMass = false;
		}
		
    }

	public void AdjustRotation()
	{
		if(Input.IsActionPressed("move_right")) AngularVelocity += turnSpeed;
		if(Input.IsActionPressed("move_left")) AngularVelocity -= turnSpeed;

	}
}
