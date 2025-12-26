using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class Main : Node
{
	// Don't forget to rebuild the project so the editor knows about the new export variable.

	[Export]
	public PackedScene AsteroidScene { get; set; }

	public static Player player;
	public static int MaxNumberOfAsteroids = 1000;

	private int _score;

	public static float GravConstant = 6.67430f * Mathf.Pow(10, -18.2f); // actual exponent is -11

	public static bool gameInitialized = false;

	private Godot.Collections.Array<Node> AsteroidNodes = [];

	public static List<Planet> AllPlanets = [];

	Vector2 ScreenSize;

	public override void _Ready()
	{
		GD.Print("Running Main.CS Ready()...");
		ScreenSize = DisplayServer.WindowGetSize();

		
		SetWorldBoundaries(false); // set to false to disable world borders.

		for(int i = 0; i < MaxNumberOfAsteroids; i++)
		{
			// GD.Print($"Generating asteroid {i}");
			GenerateAsteroid();	
		}
		AsteroidNodes = GetTree().GetNodesInGroup("asteroids");
		player = GetNode<Player>("Player");

		GeneratePlanet("Earth", Mathf.Pow(10,24) * 5.972f, new Color(0, 1, 0.2f, 1), Vector2.Zero, 1.0f);
		GeneratePlanet("Moon", Mathf.Pow(10, 22)* 7.34767309f, new Color(0.5f, 0.5f, 0.5f, 1), new Vector2(0, 200), 0.5f);

		gameInitialized = true;
		
	}
	private void GeneratePlanet(String Name, float Mass, Color color, Vector2 InitialVelocity, float Scale)
	{
		var Planet = new Planet();
		Planet = GetNode<Planet>(Name);
		Planet.Mass = Mass;
		Planet.ApplyColor(color);

		Planet.UpdateScale();

		Planet.LinearVelocity = InitialVelocity;

		AllPlanets.Add(Planet);

	}
	private void ScaleRigidBody(RigidBody2D Body, float uniformScale)
	{
		foreach(var Node in Body.GetChildren())
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

	public override void _PhysicsProcess(double delta)
	{
		if(!gameInitialized) return;

		var AllBodies = new List<RigidBody2D>
		{
			player
		};
		AllBodies.AddRange(AllPlanets);

		AllBodies.AddRange(AsteroidNodes.Cast<RigidBody2D>());
		
		int n = AllBodies.Count;

		Vector2[] positions = new Vector2[n];
		Vector2[] Forces = new Vector2[n];
		float[] masses = new float[n];
		Color[] Colors = new Color[n];


		for(int i = 0; i < n; i++)
		{

			RigidBody2D Body = AllBodies[i];
			if(Body == null) 
			{
				AllBodies.RemoveAt(i);
				n--;
				continue;
			}
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
			RigidBody2D Body1;

			Body1 = AllBodies[i];
			if(Body1 == null) continue;

			Vector2 Force = Forces[i];

			if(Body1 is Player player && player.NegativeMass) Force = -Force;

			Body1.ApplyCentralForce(Force);
			// Sprite2D Sprite1 = GetNode<Sprite2D>(Body1.Name + "/Sprite2D");
			// Sprite1.Modulate
			if(Body1 is not Planet) Body1.Modulate = Colors[i];
			
		}

	}

	private void SetWorldBoundaries(bool Enabled = true)
	{

		CollisionShape2D TopCollision = GetNode<CollisionShape2D>("Walls/TopWall/TopCollision");
		CollisionShape2D BottomCollision = GetNode<CollisionShape2D>("Walls/BottomWall/BottomCollision");
		CollisionShape2D LeftCollision = GetNode<CollisionShape2D>("Walls/LeftWall/LeftCollision");
		CollisionShape2D RightCollision = GetNode<CollisionShape2D>("Walls/RightWall/RightCollision");
		
		if (!Enabled)
		{
			List<CollisionShape2D> AllBorders = new List<CollisionShape2D>
			{
				TopCollision,
				BottomCollision,
				LeftCollision,
				RightCollision
			};

			foreach(var border in AllBorders)
			{
				GD.Print($"Found node {border.Name}");
				border.SetDeferred("disabled", true);				
			}

			return;
		}

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

	public Asteroid GenerateAsteroid()
	{
		Asteroid asteroid = AsteroidScene.Instantiate<Asteroid>();

		Vector2 asteroidspawnLocation = new(GD.Randf() * ScreenSize.X, GD.Randf() * ScreenSize.Y);

		if (asteroidspawnLocation.X <= 10) asteroidspawnLocation.X += 10;
		if (asteroidspawnLocation.X >= 1910) asteroidspawnLocation.X -= 10;

		if (asteroidspawnLocation.Y <= 10) asteroidspawnLocation.Y += 10;
		if (asteroidspawnLocation.Y >= 1070) asteroidspawnLocation.Y -= 10;

		asteroid.Position = asteroidspawnLocation;

		float RandomVelocity = 500f;

		asteroid.LinearVelocity = new Vector2(GD.Randf() * RandomVelocity - RandomVelocity/2, GD.Randf() * RandomVelocity - RandomVelocity/2);
		
		AddChild(asteroid);

		return asteroid;

		

	}
}
