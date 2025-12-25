using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class Main : Node
{
	// Don't forget to rebuild the project so the editor knows about the new export variable.

	[Export]
	public PackedScene MobScene { get; set; }

	public static Player player;
	public static int MaxNumberOfMobs = 1000;

	private int _score;

	public static float GravConstant = 100f;

	public static bool gameInitialized = false;

	private Godot.Collections.Array<Node> MobNodes = [];

	Vector2 ScreenSize;

	public override void _Ready()
	{
		GD.Print("Running Main.CS Ready()...");
		ScreenSize = DisplayServer.WindowGetSize();

		SetWorldBoundaries();

		for(int i = 0; i < MaxNumberOfMobs; i++)
		{
			GD.Print($"Generating mob {i}");
			GenerateMobs();	
		}
		MobNodes = GetTree().GetNodesInGroup("mobs");
		player = GetNode<Player>("Player");
		gameInitialized = true;
		
	}

	public override void _PhysicsProcess(double delta)
	{
		if(!gameInitialized) return;

		var AllBodies = new List<RigidBody2D>();
		AllBodies.Add(player);
		AllBodies.AddRange(MobNodes.Cast<RigidBody2D>());
		
		int n = AllBodies.Count;

		Vector2[] positions = new Vector2[n];
		Vector2[] Forces = new Vector2[n];
		float[] masses = new float[n];
		Color[] Colors = new Color[n];


		for(int i = 0; i < n; i++)
		{

			RigidBody2D Body = AllBodies[i];
			if(Body == null) GD.Print($"Error with {i}");

			positions[i] = Body.Position;
			masses[i] = Body.Mass;
			Colors[i] = Color.FromHsv(Mathf.Clamp(Body.LinearVelocity.Length()/200, 0, 0.65f), 1, 1, 1);
			

		}


		Parallel.For(0, n, i => 
		{
			RigidBody2D Body1 = (RigidBody2D)AllBodies[i];
			Vector2 ResultForce = Vector2.Zero;

			// call these once to save compute time
			Vector2 Pos1 = positions[i];
			float mass1 = masses[i];

			for(int j = 0; j < n; j++)
			{
				if(i == j) continue;
				
				Vector2 r = positions[j] - Pos1;

				// add eps to make sure no divide by 0 errors
				float rSquared = r.LengthSquared() + Mathf.Epsilon;

				float invRCubed = Main.GravConstant / (rSquared * MathF.Sqrt(rSquared));

				ResultForce += invRCubed * mass1 * masses[j] * r;

				if(AllBodies[j] is Player player && player.NegativeMass)
				{
					ResultForce = -ResultForce;
				}
			}

			Forces[i] = ResultForce;
			
		});


		for(int i = 0; i < n; i++)
		{

			RigidBody2D Body1 = (RigidBody2D)AllBodies[i];
			Vector2 Force = Forces[i];

			if(Body1 is Player player && player.NegativeMass) Force = -Force;

			Body1.ApplyCentralForce(Force);
			// Sprite2D Sprite1 = GetNode<Sprite2D>(Body1.Name + "/Sprite2D");
			// Sprite1.Modulate
			Body1.Modulate = Colors[i];
			
		}

	}

	private void SetWorldBoundaries()
	{
		CollisionShape2D TopCollision = GetNode<CollisionShape2D>("Walls/TopWall/TopCollision");
		CollisionShape2D BottomCollision = GetNode<CollisionShape2D>("Walls/BottomWall/BottomCollision");
		CollisionShape2D LeftCollision = GetNode<CollisionShape2D>("Walls/LeftWall/LeftCollision");
		CollisionShape2D RightCollision = GetNode<CollisionShape2D>("Walls/RightWall/RightCollision");

		List<CollisionShape2D> HorizontalWorldBorders = [TopCollision, BottomCollision];
		List<CollisionShape2D> VerticalWorldBorders = [LeftCollision, RightCollision];

		foreach (CollisionShape2D Collision in HorizontalWorldBorders)
		{
			if(Collision.Shape is RectangleShape2D rectangleCollision) rectangleCollision.Size = new Vector2(ScreenSize.X, 5);
		}

		foreach (CollisionShape2D Collision in VerticalWorldBorders)
		{
			if(Collision.Shape is RectangleShape2D rectangleCollision) rectangleCollision.Size = new Vector2(5, ScreenSize.Y);
		}

		GD.Print($"Screen Size: {ScreenSize}");

		TopCollision.Position = new Vector2(ScreenSize.X / 2, 5);
		BottomCollision.Position = new Vector2(ScreenSize.X / 2, ScreenSize.Y - 5);
		LeftCollision.Position = new Vector2(5, ScreenSize.Y / 2);
		RightCollision.Position = new Vector2(ScreenSize.X - 5, ScreenSize.Y / 2);
	}

	private void GenerateMobs()
	{

		Mob mob = MobScene.Instantiate<Mob>();

		Vector2 mobSpawnLocation = new(GD.Randf() * ScreenSize.X, GD.Randf() * ScreenSize.Y);

		if (mobSpawnLocation.X <= 10) mobSpawnLocation.X += 10;
		if (mobSpawnLocation.X >= 1910) mobSpawnLocation.X -= 10;

		if (mobSpawnLocation.Y <= 10) mobSpawnLocation.Y += 10;
		if (mobSpawnLocation.Y >= 1070) mobSpawnLocation.Y -= 10;

		mob.Position = mobSpawnLocation;

		float RandomVelocity = 50f;

		mob.LinearVelocity = new Vector2(GD.Randf() * RandomVelocity - RandomVelocity/2, GD.Randf() * RandomVelocity - RandomVelocity/2);
		
		AddChild(mob);

		

	}
}
