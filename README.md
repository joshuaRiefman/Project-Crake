# Project-Crake

## What is Project Crake?

Project Crake, which began as a school project and is named after Margaret Atwood's _Oryx and Crake_, is a project built in Unity which is capable of modeling simple evolution in a bacteria and its predator, an amoeba. This simulation began, and still is, an exercise in realizing the beauty of emergence. Truly, the simulation isn't that complicated. Yet, some strikingly realistic evolution can take place naturally, and as you, the user, artifically increases or decreases the pressure of natural selection through the modification of environment variables.  

<br>

Every lifeform in the simulation has a unique set of genes which mutate as they reproduce, and the average for each gene's "level of expression" is outputted to a CSV in "Assets/Report/Data.csv", meaning that you can graph and watch the bacteria and amoeba evolve over time. In time, I'd like to plot the populations of both lifeforms in a sort of "phase space", and visualize the divergence at points, as well. 


## How to use Project Crake

Project Crake is a Unity project built in 2022.1.6f1 on Apple Silicon. Clone this repository and then open the project root directory with Unity Hub.. Unlike most Unity projects, Project Crake is **not** designed to be built into an executable, but rather ran with the Unity Editor so that you are given significantly more control and have more real-time data exposed. 

## What's next?

Project Crake is functionally complete, yet in need of a visual update. In addition, the integration of a graphing package into the project to visualize simulation progress while it is ongoing will be implemented in the future.
