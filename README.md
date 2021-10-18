## ClaySimVR

#### Goals
Simulate the physicallity of working with a malluable, dynamic material.
Allow users to tear, indent, and mold the material using their hands as they would in real life.

---


#### Current (technical) approach
Create a Eularian/Langrarian hybrid particle sim which simulates the physical properties of clay.

---


#### Primary Technical Obstacles
- Performance
  - Simulating enough particles to create a adequetly high fidelity simulation.
  -  Rendering the particles to create a uniform surface that can also tear apart.
- Creating a convincing clay particle sim


###### Current Solutions
- Data Oriented (struct and buffer based) algo

###### Future improvements or alternative solutions
- Burst Compile tight loops and multithread tehm
- Use Compute shadesr > jobs multithreading



