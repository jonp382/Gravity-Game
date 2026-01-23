using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class Main : Node
{
	[Export]
	public PackedScene AsteroidScene { get; set; }

	[Export] public PackedScene PlanetScene { get; set; }

	public static Player player;
	public static int MaxNumberOfAsteroids = 1000;
	public static int MaxNumberOfPlanets = 20;

	private int _score;

	public static float GravConstant = 200;

	public static bool gameInitialized = false;

	public static readonly Rect2 Bounds = new Rect2(-10000, -10000, 20000, 20000); // (posX, posY, sizeX, sizeY)

	private Godot.Collections.Array<Node> AsteroidNodes = [];

	public static List<Planet> AllPlanets = [];

	Vector2 ScreenSize;

	public override void _Ready()
	{
		GD.Print("Running Main.CS Ready()...");
		ScreenSize = DisplayServer.WindowGetSize();

		SetWorldBoundaries(true); // set to false to disable world borders.

		for(int i = 0; i < MaxNumberOfAsteroids; i++)
		{
			// GD.Print($"Generating asteroid {i}");
			GenerateAsteroid();	
		}
		AsteroidNodes = GetTree().GetNodesInGroup("asteroids");
		player = GetNode<Player>("Player");

		for(int i = 0; i < MaxNumberOfPlanets; i++)
		{
			String name = "Planet " + i;
			float mass = Math.Clamp(GD.Randf() * 500000, 10000, 500000);
			Color col = new Color(GD.Randf(), GD.Randf(), GD.Randf(), 1);
			Vector2 position = new Vector2(GD.Randf() * Bounds.Size.X - Bounds.Size.X/2, GD.Randf() * Bounds.Size.Y - Bounds.Size.Y/2);
			Vector2 initialVelocity = new Vector2(GD.Randf()*200-100, GD.Randf()*200-100);

			GeneratePlanet(name, mass, col, position, initialVelocity);
		}
		// GeneratePlanet("Earth", 100000, new Color(0, 1, 0.2f, 1), Vector2.Zero, 1.0f);
		// GeneratePlanet("Moon", 20000, new Color(0.5f, 0.5f, 0.5f, 1), new Vector2(0, 250), 0.5f);

		gameInitialized = true;
		
	}
	private void GeneratePlanet(String name, float mass, Color color, Vector2 Position, Vector2 initialVelocity)
	{
		Planet planet = PlanetScene.Instantiate<Planet>();

		planet.Name = name;
		planet.Mass = mass;
		planet.Position = Position;
		planet.LinearVelocity = initialVelocity;
		
		planet.ApplyColor(color);

		planet.UpdateScale();

		GD.Print($"Generated planet {planet.Name} @ {planet.Position} with velocity {planet.LinearVelocity}, mass {planet.Mass}, color {planet.color} and scale {planet.childScale}");
		AllPlanets.Add(planet);
		AddChild(planet);

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

		AllBodies.AddRange(AsteroidNodes.Cast<Asteroid>());
		
		int n = AllBodies.Count;

		Vector2[] positions = new Vector2[n];
		Vector2[] Forces = new Vector2[n];
		float[] masses = new float[n];
		Color[] Colors = new Color[n];

		List<RigidBody2D> BodiesToRemove = [];
		foreach(RigidBody2D body in AllBodies)
		{
			if(body == null) BodiesToRemove.Add(body);

			if(body is Asteroid asteroid)
			{
				if (asteroid.shouldRemove)
				{
					BodiesToRemove.Add(asteroid);
					try { asteroid.QueueFree(); } catch {}
				}
			}
			if(body is Planet planet)
			{
				if (planet.shouldRemove)
				{
					BodiesToRemove.Add(planet);
					AllPlanets.Remove(planet);
					try { planet.QueueFree(); } catch {}
				}
			}
		}

		AllBodies = AllBodies.Except(BodiesToRemove).ToList();
		n = AllBodies.Count;

		for(int i = 0; i < n; i++)
		{
			RigidBody2D Body = AllBodies[i];

			positions[i] = Body.Position;
			masses[i] = Body.Mass;
			// Colors[i] = Color.FromHsv(Mathf.Clamp(Body.LinearVelocity.Length()/200, 0, 0.65f), 1, 1, 1);
		}

		n = AllBodies.Count;

		Parallel.For(0, n, i => 
		{
			RigidBody2D Body1 = (RigidBody2D)AllBodies[i];
			Vector2 ResultForce = Vector2.Zero;

			// call these once to save compute time
			Vector2 Pos1 = positions[i];
			float mass1 = masses[i];

			bool isBodyOneAsteroid = AllBodies[i] is Asteroid;

			 

			for(int j = 0; j < n; j++)
			{
				if(i == j) continue;
				if (AllPlanets.Count > 0 && isBodyOneAsteroid && AllBodies[j] is Asteroid) continue;
				
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
			// if(Body1 is not Planet) Body1.Modulate = Colors[i];
			
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
			if(Collision.Shape is RectangleShape2D rectangleCollision) rectangleCollision.Size = new Vector2(Bounds.Size.X, 5);
		}

		foreach (CollisionShape2D Collision in VerticalWorldBorders)
		{
			if(Collision.Shape is RectangleShape2D rectangleCollision) rectangleCollision.Size = new Vector2(5, Bounds.Size.Y);
		}

		GD.Print($"Bounds size: {Bounds.Size}");

		TopCollision.Position = new Vector2(0, Bounds.Position.Y);
		GD.Print($"Placed Top World Border at {TopCollision.Position}");
		BottomCollision.Position = new Vector2(0, Bounds.Position.Y + Bounds.Size.Y);
		GD.Print($"Placed Bottom World Border at {BottomCollision.Position}");
		LeftCollision.Position = new Vector2(Bounds.Position.X, 0);
		GD.Print($"Placed Left World Border at {LeftCollision.Position}");
		RightCollision.Position = new Vector2(Bounds.Position.X + Bounds.Size.X, 0);
		GD.Print($"Placed Right World Border at {RightCollision.Position}");
	}

	public Asteroid GenerateAsteroid()
	{
		Asteroid asteroid = AsteroidScene.Instantiate<Asteroid>();

		Vector2 asteroidspawnLocation = new(GD.Randf() * Bounds.Size.X - Bounds.Size.X/2, GD.Randf() * Bounds.Size.Y - Bounds.Size.Y/2);

		// if (asteroidspawnLocation.X <= 10) asteroidspawnLocation.X += 10;
		// if (asteroidspawnLocation.X >= 1910) asteroidspawnLocation.X -= 10;

		// if (asteroidspawnLocation.Y <= 10) asteroidspawnLocation.Y += 10;
		// if (asteroidspawnLocation.Y >= 1070) asteroidspawnLocation.Y -= 10;

		asteroid.Position = asteroidspawnLocation;

		float RandomVelocity = 500f;

		float randFactor = Mathf.Clamp(GD.Randf(), 0.4f, 1.0f);
		Color newCol = new(randFactor, randFactor, randFactor, 1);
		asteroid.Modulate = new Color(newCol);

		asteroid.LinearVelocity = new Vector2(GD.Randf() * RandomVelocity - RandomVelocity/2, GD.Randf() * RandomVelocity - RandomVelocity/2);
		
		AddChild(asteroid);

		return asteroid;

		

	}
}
