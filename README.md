# Gravity Game

This is a simple 2D gravity simulator in Godot that uses Newtonian physics. Asteroids and Planets can fly around in space and contact each other transferring their mass. The player controls a small starship that can thrust forwards/backwards (up/down arrows), turn (left/right arrows), and invert its own gravity (space). 

The program spawns in a couple planets and many asteroids that all use Newtonian physics. Some optimizations and concessions are made. For example, the asteroids do not attract other asteroids (for performance) if there are any planets in play.

The primary objective was to learn more about Godot. 

<img width="1375" height="937" alt="image" src="https://github.com/user-attachments/assets/5f086eb1-faee-4003-b431-62a80c189f62" />

## How to Run

1. Make sure Godot is installed as this is not a standalone program, it was just for learning and fun
2. Clone the project
3. Import the `project.godot` file into Godot.
4. Hit "Run" on the side bar

## Technical Info
Using Godot's physics engine, asteroids, planets and the player's space ship can collide with each other, with the resulting displacements depending on the mass of the contacting entities. A heavy asteroid will knock the ship far off course, but is negligible to a planet.

The objects are pulled together using Newton's gravity equation

$$
F = G \frac{m_1 m_2}{r^2}
$$

With F being the force, G being the gravitational constant (which is a configurable property in this game), $m_1$ and $m_2$ being the mass of the two objects being pulled together, and $r^2$ being the squared distance between them.

This equation gives rise to phenomena like orbital mechanics, which can be seen in the videos below. Note that this game does not feature any atmosphere, atmospheric drag etc., so true orbital decay is not simulated.

## Showcase

https://github.com/user-attachments/assets/b87096d4-a773-42f2-bef8-5999566e8b84



https://github.com/user-attachments/assets/62ec8085-4ab8-48d6-874d-fb35bc8168e5

