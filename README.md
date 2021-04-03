## ClaySimVR

#### Goals
Simulate the physicallity of working with a malluable, dynamic material.
Allow users to tear, indent, and mold the material using their hands as they would in real life.


---


#### Current (technical) approach
Create a Lagrangian particle sim which simulates the physical properties of clay.
Render the particles uniformly using a Ray Marching, smoothed signed distance field function. 
(The effect would be each "clay" particle would be a sort of 3D metaball)


---


#### Primary Technical Obstacles
- Performance
  - Simulating enough particles to create a adequetly high fidelity simulation.
  -  Rendering the particles to create a uniform surface that can also tear apart.
- Creating a convincing clay particle sim


###### Current Solutions
- Data Oriented (struct and buffer based) spatial partition Algo --> Octree
- DOTS Job System (for multi threading the algorithim)
- Sending and using spatial partition algo shader-side in the raymarching algo (vertex buffer and/or texture indirection)


###### Future improvements or alternative solutions
- compute shader instead of jobs multithreading
- only update particles which have moved recently, have octree persist between frames
- Eulerian simulation > Lagragian
    - only use particles for rendering (raymarching)
- don't render via SDF and raymarching --> use a sort of screen-based blurring effect of the particles
  - render each particles as a 2D blurred thing to an offscreen texture
  - metaball/threshold the thing in 2D
  - blit back to main render texture



