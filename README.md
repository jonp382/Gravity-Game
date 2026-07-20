# Gravity Game

This is a simple 2D gravity simulator in Godot that uses Newtonian physics. Asteroids and Planets can fly around in space and contact each other transferring their mass. The player controls a small starship that can thrust forwards/backwards (up/down arrows), turn (left/right arrows), and invert its own gravity (space). 

The program spawns in a couple planets and many asteroids that all use Newtonian physics. Some optimizations and concessions are made. For example, the asteroids do not attract other asteroids (for performance) if there are any planets in play.

The primary objective was to learn more about Godot. 



## How to Run

1. Make sure Godot is installed as this is not a standalone program, it was just for learning and fun
2. Clone the project
3. Import the `project.godot` file into Godot.
4. Hit "Run" on the side bar